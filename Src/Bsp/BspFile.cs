using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BspLib.Bsp.Exceptions;
using BspLib.Wad;
using BspLib.Vector;
using BspLib.Colliders;

namespace BspLib.Bsp
{
    public class BspFile
    {
        public BspFile()
        {
        }

        public enum BspVersion : uint
        {
            Unknown = 0,
            Quake1 = 29,
            GoldSource = BspLib.Bsp.GoldSource.Bsp.Version,
            /// <summary>
            /// 0x56 = V,
            /// 0x42 = B,
            /// 0x53 = S,
            /// 0x50 = P
            /// </summary>
            ValveBSP = 0x50534256,
            /// <summary>
            /// 0x72 = R,
            /// 0x42 = B,
            /// 0x53 = S,
            /// 0x50 = P
            /// </summary>
            RespawnBSP = 0x50534272, //TODO validate
        }

        #region Variables

        #region Textures

        public List<Texture> PackedTextures
        {
            get;
        } = new List<Texture>();

        public List<string> UsedTextures
        {
            get;
        } = new List<string>();

        public Dictionary<string, Vector2> TextureDimensions
        {
            get;
        } = new Dictionary<string, Vector2>();

        #endregion

        public List<Dictionary<string, string>> Entities
        {
            get;
        } = new List<Dictionary<string, string>>();

        public List<Model> Models
        {
            get;
        } = new List<Model>();

        public List<Collider> Colliders
        {
            get;
        } = new List<Collider>();

        #endregion

        [Flags]
        public enum LoadFlags
        {
            Visuals = 1 << 0,
            Colliders = 1 << 1,
            Entities = 1 << 2,
            /// <summary>
            /// Not supported in most formats
            /// </summary>
            PackedTextures = 1 << 3,
        }

        public const LoadFlags AllLoadFlags = (LoadFlags)int.MaxValue;//0b111111...

        #region Temporary classes

        public class Polygon
        {
            public Polygon(string textureName, int[] indices)
            {
                if (indices == null)
                    throw new ArgumentOutOfRangeException(nameof(indices));

                this.TextureName = textureName;
                this.Indices = indices;
            }

            public string TextureName
            {
                get;
            }

            public int[] Indices
            {
                get;
            }

            /// <summary>
            /// Converts this polygon into triangles.
            /// WARNING: Works only for convex polygons.
            /// </summary>
            public uint[] ToTriangles()
            {
                if (Indices.Length < 3)
                    return new uint[0];

                var points = new List<uint>();

                int p0 = Indices[0];
                for (int i = 2; i < Indices.Length; i++)
                {
                    points.Add((uint)p0);
                    points.Add((uint)Indices[i - 1]);
                    points.Add((uint)Indices[i]);
                }

                return points.ToArray();
            }
        }

        public class Model
        {
            public Model(Dictionary<string, uint[]> triangles, Vector3f[] positions, Vector2f[] textureCoordinates)
            {
                this.Triangles = triangles;

                if (positions.Length != textureCoordinates.Length)
                    throw new ArgumentOutOfRangeException(string.Format("{0} and {1}", nameof(positions), nameof(TextureCoordinates)));

                this.Positions = positions;
                this.TextureCoordinates = textureCoordinates;
            }

            /// <summary>
            /// Key = texture name,
            /// Value = polygons
            /// </summary>
            /// <value>The polygons.</value>
            public Dictionary<string, uint[]> Triangles
            {
                get;
            }

            public Vector3f[] Positions
            {
                get;
            }

            public Vector2f[] TextureCoordinates
            {
                get;
            }

            /// <summary>
            /// Groups polygons by their texture.
            /// </summary>
            public static Dictionary<string, List<Polygon>> GroupPolygonsByTexture(List<Polygon> polygons)
            {
                Dictionary<string, List<Polygon>> grouped = new Dictionary<string, List<Polygon>>();
                foreach (var p in polygons)
                {
                    if (!grouped.ContainsKey(p.TextureName))
                        grouped.Add(p.TextureName, new List<Polygon>());

                    grouped[p.TextureName].Add(p);
                }

                return grouped;
            }

            /// <summary>
            /// Convert polygons into triangles.
            /// WARNING: Polygons will loose their texture information
            /// </summary>
            public static uint[] PolygonsToTrianglesPoints(List<Polygon> polygons)
            {
                var points = new List<uint>();
                foreach (var pol in polygons)
                    points.AddRange(pol.ToTriangles());
                return points.ToArray();
            }
            /// <summary>
            /// Convert polygons into triangles.
            /// WARNING: Polygons will loose their texture information
            /// </summary>
            public static Dictionary<string, uint[]> PolygonsToTrianglesPoints(Dictionary<string, List<Polygon>> polygons)
            {
                var triangles = new Dictionary<string, uint[]>(polygons.Count);
                foreach (var kvp in polygons)
                    triangles.Add(kvp.Key, PolygonsToTrianglesPoints(kvp.Value));
                return triangles;
            }

            /// <summary>
            /// Optimize positions and textureCoordinates by removing unused ones.
            /// </summary>
            public static void Optimize(Dictionary<string, int[]> triangles, ref Vector3f[] positions, ref Vector2f[] textureCoordinates)
            {
                var used = new bool[positions.Length];

                foreach (var t in triangles)
                    foreach (var i in t.Value)
                        used[i] = true;

                //All indices found -> no need to remove unused ones
                if (!used.Any((u) => !u))
                    return;

                //default(int) is left for unused but there is not need to take care about it (we will not look at it's index)
                int[] optimizedIndices = new int[used.Length];
                int optimizedLength = 0;
                {
                    for (int i = 0; i < used.Length; i++)
                    {
                        if (used[i])
                        {
                            optimizedIndices[i] = optimizedLength;
                            optimizedLength++;
                        }
                    }
                }

                //Replacing indices
                foreach (var t in triangles)
                {
                    var indices = t.Value;
                    for (int i = 0; i < indices.Length; i++)
                        indices[i] = optimizedIndices[indices[i]];
                }

                //Removing unused values in arrays
                {
                    var newPositions = new Vector3f[optimizedLength];
                    var newTextureCoordinates = new Vector2f[optimizedLength];

                    int lastValidIndex = 0;
                    for (int i = 0; i < used.Length; i++)
                    {
                        if (used[i])
                        {
                            newPositions[lastValidIndex] = positions[i];
                            newTextureCoordinates[lastValidIndex] = textureCoordinates[i];
                        }
                    }

                    positions = newPositions;
                    textureCoordinates = newTextureCoordinates;
                }
            }
        }

        #endregion

        public void SaveVisualsAsObjAndExportMtlFile(string objPath, string textureDirectory = "textures", bool saveTextures = true, bool replaceTextureNames = false, string textureExtension = "png") => SaveVisualsAsObjAndExportMtlFile(objPath, string.Format("{0}.mtl", Path.GetFileNameWithoutExtension(objPath)), textureDirectory, saveTextures, replaceTextureNames, textureExtension);

        public void SaveVisualsAsObjAndExportMtlFile(string objPath, string mtlPath, string textureDirectory = "textures", bool saveTextures = true, bool replaceTextureNames = false, string textureExtension = "png")
        {
            var nameDictionary = new Dictionary<string, string>();
            if (replaceTextureNames)
            {
                var names = new List<string>();

                foreach (var m in Models)
                    foreach (var t in m.Triangles)
                        if (!Regex.IsMatch(t.Key, "^[a-zA-Z0-9_\\-]+$"))
                            if (!names.Contains(t.Key))
                                names.Add(t.Key);

                int lastID = 0;

                foreach (var n in names)
                    nameDictionary.Add(n, string.Format("Texture_{0:0000}", lastID++));
            }

            if (!File.Exists(objPath))
            {
                using (var writer = new StreamWriter(File.OpenWrite(objPath)))
                {
                    SaveVisualsAsObjFile(writer, mtlPath, nameDictionary);
                }
            }

            if (!File.Exists(mtlPath))
            {
                using (var writer = new StreamWriter(File.OpenWrite(mtlPath)))
                {
                    SaveMtlFile(writer, textureDirectory, nameDictionary);
                }
            }

            if (saveTextures)
            {
                if (!string.IsNullOrEmpty(textureDirectory) && !Directory.Exists(textureDirectory))
                    Directory.CreateDirectory(textureDirectory);

                SaveTexturesFile(PackedTextures, textureDirectory, nameDictionary, textureExtension);
            }
        }

        public void SaveMtlFile(StreamWriter writer, string directory = "textures", Dictionary<string, string> nameDictionary = null, string textureExtension = "png")
        {
            var extension = "." + textureExtension;

            var existingMtl = new List<string>();

            foreach (var m in Models)
                foreach (var t in m.Triangles)
                    if (!existingMtl.Contains(t.Key))
                        existingMtl.Add(t.Key);

            if (nameDictionary != null)
                for (int i = 0; i < existingMtl.Count; i++)
                    existingMtl[i] = nameDictionary.ContainsKey(existingMtl[i]) ? nameDictionary[existingMtl[i]] : existingMtl[i];

            foreach (var mtl in existingMtl)
            {
                var mtl_path = string.IsNullOrEmpty(directory) ? mtl + extension : Path.Combine(directory, mtl + extension);

                writer.WriteLine("newmtl {0}", mtl);
                writer.WriteLine("Ka 1 1 1 # white");
                writer.WriteLine("Kd 1 1 1 # white");
                writer.WriteLine("Ks 0 0 0 # black (off)");
                writer.WriteLine("d 1");
                writer.WriteLine("illum 2");
                writer.WriteLine("map_d {0}", mtl_path);
                writer.WriteLine("map_Ka {0}", mtl_path);
                writer.WriteLine("map_Kd {0}", mtl_path);
                writer.WriteLine("map_Ks {0}", mtl_path);


                writer.Flush();
            }
        }

        public static void SaveTexturesFile(Texture[] textures, string directory = "textures", Dictionary<string, string> nameDictionary = null, string textureExtension = "png")
        {
            var extension = "." + textureExtension;

            foreach (var t in textures)
            {
                var name = nameDictionary != null && nameDictionary.ContainsKey(t.Name) ? nameDictionary[t.Name] : t.Name;

                var path = string.IsNullOrEmpty(directory) ? name + extension : Path.Combine(directory, name + extension);

                if (!File.Exists(path))
                    t.Bitmap.Save(path);
            }
        }
        public static void SaveTexturesFile(IEnumerable<Texture> textures, string directory = "textures", Dictionary<string, string> nameDictionary = null, string textureExtension = "png")
        {
            var extension = "." + textureExtension;

            foreach (var t in textures)
            {
                var name = nameDictionary != null && nameDictionary.ContainsKey(t.Name) ? nameDictionary[t.Name] : t.Name;

                var path = string.IsNullOrEmpty(directory) ? name + extension : Path.Combine(directory, name + extension);

                if (!File.Exists(path))
                    t.Bitmap.Save(path);
            }
        }

        public void SaveTexturesToDirectory(string directory, string textureExtension)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            SaveTexturesFile(this.PackedTextures.ToArray(), directory: directory, textureExtension: textureExtension);
        }

        public void SaveVisualsAsObjFile(string path)
        {
            if (File.Exists(path))
                return;

            using (var writer = new StreamWriter(File.OpenWrite(path)))
            {
                SaveVisualsAsObjFile(writer, string.Format("{0}.mtl", Path.GetFileNameWithoutExtension(path)));
            }
        }

        public void SaveVisualsAsObjFile(StreamWriter writer, string mtllibName, Dictionary<string, string> nameDictionary = null)
        {
            var modifiedModels = new List<Model>();

            uint lastIndex = 0;

            // Modifying indices to be placed behind each other
            foreach (var m in Models)
            {
                var newTriangles = new Dictionary<string, uint[]>();
                foreach (var t in m.Triangles)
                {
                    var tIndices = new uint[t.Value.Length];
                    for (int i = 0; i < tIndices.Length; i++)
                        tIndices[i] = t.Value[i] + lastIndex;
                    newTriangles.Add(t.Key, tIndices);
                }

                lastIndex += (uint)m.Positions.Length;

                var newM = new Model(newTriangles, m.Positions, m.TextureCoordinates);
                modifiedModels.Add(newM);
            }

            // Writing positions
            for (int mi = 0; mi < modifiedModels.Count; mi++)
            {
                var m = modifiedModels[mi];
                for (int pi = 0; pi < m.Positions.Length; pi++)
                {
                    var p = m.Positions[pi];
                    // Z coordinate is UP
                    // X coordinate is opposite
                    writer.WriteLine("v {0} {1} {2}", -p.X, p.Z, p.Y);
                }
                writer.Flush();
            }

            // Writing UVs
            for (int mi = 0; mi < modifiedModels.Count; mi++)
            {
                var m = modifiedModels[mi];
                for (int ti = 0; ti < m.TextureCoordinates.Length; ti++)
                {
                    var t = m.TextureCoordinates[ti];
                    writer.WriteLine("vt {0} {1}", t.X, -t.Y);
                }
                writer.Flush();
            }

            // mtllib
            writer.WriteLine("mtllib {0}", mtllibName);
            writer.Flush();

            // Writing Indices
            for (int mi = 0; mi < modifiedModels.Count; mi++)
            {
                writer.WriteLine("# Model with ID={0}", mi);
                writer.WriteLine("o Model_{0}", mi);
                var m = modifiedModels[mi];
                foreach (var t in m.Triangles)
                {
                    writer.WriteLine("# Texture name: {0}", t.Key);

                    var name = nameDictionary != null && nameDictionary.ContainsKey(t.Key) ? nameDictionary[t.Key] : t.Key;

                    writer.WriteLine("g Model_{0}_Texture_{1}", mi, name);
                    writer.WriteLine("usemtl {0}", name);

                    for (int ii = 0; ii < t.Value.Length; ii += 3)
                        writer.WriteLine("f {0}/{0} {1}/{1} {2}/{2}", t.Value[ii + 2] + 1, t.Value[ii + 1] + 1, t.Value[ii + 0] + 1); // All indices are starting from 1 (not from 0)
                    writer.Flush();
                }
                writer.Flush();
            }
        }

        #region Loading

        #region Visuals

        public void LoadVisualsFromFile(string path) => LoadVisualsFromFile(this, path);

        public static void LoadVisualsFromFile(BspFile bsp, string path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LoadVisualsFromBspStream(bsp, stream);
            }
        }

        public void LoadVisualsFromBspStream(Stream stream) => LoadVisualsFromBspStream(this, stream);

        public static void LoadVisualsFromBspStream(BspFile bsp, Stream stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        GoldSource.Bsp.LoadVisualsFromBspStream(bsp, stream);
                    }
                    break;
                case (uint)BspVersion.ValveBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "ValveBSP (Source engine)");
                    }
                case (uint)BspVersion.RespawnBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "rBSP (Titanfall)");
                    }
                default:
                    throw new BspVersionNotSupportedException(version);
            }
        }

        #endregion

        #region Colliders

        public void LoadCollidersFromFile(string path) => LoadCollidersFromFile(this, path);

        public static void LoadCollidersFromFile(BspFile bsp, string path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LoadCollidersFromBspStream(bsp, stream);
            }
        }

        public void LoadCollidersFromBspStream(Stream stream) => LoadCollidersFromBspStream(this, stream);

        public static void LoadCollidersFromBspStream(BspFile bsp, Stream stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        GoldSource.Bsp.LoadCollidersFromBspStream(bsp, stream);
                    }
                    break;
                case (uint)BspVersion.ValveBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "ValveBSP (Source engine)");
                    }
                case (uint)BspVersion.RespawnBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "rBSP (Titanfall)");
                    }
                default:
                    throw new BspVersionNotSupportedException(version);
            }
        }

        #endregion

        #region Entities

        public void LoadEntitiesFromFile(string path) => LoadEntitiesFromFile(this, path);

        public static void LoadEntitiesFromFile(BspFile bsp, string path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LoadEntitiesFromBspStream(bsp, stream);
            }
        }

        public void LoadEntitiesFromBspStream(Stream stream) => LoadEntitiesFromBspStream(stream);

        public static void LoadEntitiesFromBspStream(BspFile bsp, Stream stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        GoldSource.Bsp.LoadEntitiesFromBspStream(bsp, stream);
                    }
                    break;
                case (uint)BspVersion.ValveBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "ValveBSP (Source engine)");
                    }
                case (uint)BspVersion.RespawnBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "rBSP (Titanfall)");
                    }
                default:
                    throw new BspVersionNotSupportedException(version);
            }
        }

        #endregion

        #region Packed textures

        public void LoadPackedTexturesFromFile(string path) => LoadPackedTexturesFromFile(this, path);

        public static void LoadPackedTexturesFromFile(BspFile bsp, string path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LoadPackedTexturesFromBspStream(bsp, stream);
            }
        }

        public void LoadPackedTexturesFromBspStream(Stream stream) => LoadPackedTexturesFromBspStream(this, stream);

        public static void LoadPackedTexturesFromBspStream(BspFile bsp, Stream stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        GoldSource.Bsp.LoadPackedTexturesFromBspStream(bsp, stream);
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region All

        public void LoadAllFromFile(LoadFlags flags, string path) => LoadAllFromFile(flags, path);

        public static void LoadAllFromFile(BspFile bsp, LoadFlags flags, string path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                LoadAllFromBspStream(bsp, flags, stream);
            }
        }

        public void LoadAllFromBspStream(LoadFlags flags, Stream stream) => LoadAllFromBspStream(this, flags, stream);

        public static void LoadAllFromBspStream(BspFile bsp, LoadFlags flags, Stream stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        GoldSource.Bsp.LoadAllFromBspStream(bsp, flags, stream);
                    }
                    break;
                case (uint)BspVersion.ValveBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "ValveBSP (Source engine)");
                    }
                case (uint)BspVersion.RespawnBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "rBSP (Titanfall)");
                    }
                default:
                    throw new BspVersionNotSupportedException(version);
            }
        }

        #endregion

        #endregion

        #region Version

        public static string GetVersionName(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GetVersionName(stream);
            }
        }

        public static string GetVersionName(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        return "Quake 1 (v29)";
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        return "GoldSource (v30)";
                    }
                case (uint)BspVersion.ValveBSP:
                    {
                        return "ValveBSP (Source engine)";
                        //TODO subversions
                    }
                case (uint)BspVersion.RespawnBSP:
                    {
                        return "rBSP (Titanfall)";
                    }
                default:
                    return "Unknown";
            }
        }

        #endregion

        #region Used Textures

        public static string[] GetUsedTextures(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GetUsedTextures(stream);
            }
        }

        public static string[] GetUsedTextures(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        return GoldSource.Bsp.GetUsedTextures(reader);
                    }
                case (uint)BspVersion.ValveBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "ValveBSP (Source engine)");
                    }
                case (uint)BspVersion.RespawnBSP:
                    {
                        throw new BspVersionNotSupportedException(version, "rBSP (Titanfall)");
                    }
                default:
                    throw new BspVersionNotSupportedException(version);
            }
        }

        #endregion

        #region Used Textures

        public static Texture[] GetPackedTextures(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GetPackedTextures(stream);
            }
        }

        public static Texture[] GetPackedTextures(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = stream.Position;

            //4 bytes = 1 uint
            uint version = reader.ReadUInt32();

            stream.Position = startPos;


            switch (version)
            {
                case (uint)BspVersion.Quake1:
                    {
                        throw new BspVersionNotSupportedException(version, "Quake 1");
                    }
                case (uint)BspVersion.GoldSource:
                    {
                        return GoldSource.Bsp.GetPackedTextures(reader);
                    }
                default:
                    return new Texture[0];
            }
        }

        #endregion

    }
}