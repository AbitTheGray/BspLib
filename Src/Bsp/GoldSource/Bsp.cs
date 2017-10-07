using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BspLib.Wad;
using BspLib.Wad.Wad3;
using BspLib.Vector;
using System.Text.RegularExpressions;
using static BspLib.Bsp.BspFile;

namespace BspLib.Bsp.GoldSource
{
    public static class Bsp
    {
        public const uint Version = 30;

        public static Lump[] LoadBspHeader(BinaryReader reader)
        {
            // Version
            {
                // 4 bytes
                var version = reader.ReadUInt32();

                if (version != Version)
                    return null; // Invalid version 
            }
            // Lumps
            {
                var lumps = new Lump[(int)Lumps._Count];

                for (int l = 0; l < (int)Lumps._Count; l++)
                    lumps[l] = new Lump(reader.ReadInt32(), reader.ReadInt32());

                return lumps;
            }
        }

        #region Visuals

        public static bool LoadVisualsFromData(BspFile bsp, Vector3f[] vertices, Edge[] edges, int[] surfaceEdges, Face[] faces, string[] usedTextures, Dictionary<string, Vector2> textureDimensions, TextureInfo[] textureInfo, ModelData[] modelDatas)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");
            if (modelDatas.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(modelDatas), "No enought models. At least one model is needed.");

            var models = new Model[modelDatas.Length];

            for (int md_i = 0; md_i < modelDatas.Length; md_i++)
                models[md_i] = ModelDataToPolygon(modelDatas[md_i], vertices, edges, surfaceEdges, faces, textureInfo, usedTextures, textureDimensions);

            bsp.Models.AddRange(models);
            return true;
        }

        public static bool LoadVisualsFromStreams(BspFile bsp, Stream vertices_lump, Stream edges_lump, Stream surfaceEdges_lump, Stream faces_lump, Stream textures_lump, Stream textureInfo_lump, Stream models_lump)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var vertices = ReadVerticesLump(vertices_lump);

            var edges = ReadEdgesLump(edges_lump);

            var surfaceEdges = ReadSurfaceEdgesLump(surfaceEdges_lump);

            var faces = ReadFaceLump(faces_lump);

            var textures = ReadTextureLump(textures_lump, loadPackedTextures: false);
            var usedTextures = textures.Item2;
            var textureDimensions = textures.Item3;

            var textureInfo = ReadTextureInfoLump(textureInfo_lump);

            var models = ReadModelsLump(models_lump);

            return LoadVisualsFromData(bsp, vertices, edges, surfaceEdges, faces, usedTextures, textureDimensions, textureInfo, models);
        }

        public static bool LoadVisualsFromBspStream(BspFile bsp, Stream bsp_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!bsp_stream.CanRead || !bsp_stream.CanSeek)
                return false;

            var file_start = bsp_stream.Position;

            var header = LoadBspHeader(new BinaryReader(bsp_stream));
            if (header == null)
                return false;

            foreach (var lump in header)
                if (file_start + lump.Offset + lump.Size > bsp_stream.Length)
                    return false;// Invalid header (will read out of the stream)

            bsp_stream.Position = file_start + header[(int)Lumps.Vertices].Offset;
            var vertices = ReadVerticesLump(bsp_stream, header[(int)Lumps.Vertices].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.Edges].Offset;
            var edges = ReadEdgesLump(bsp_stream, header[(int)Lumps.Edges].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.SurfEdges].Offset;
            var surfaceEdges = ReadSurfaceEdgesLump(bsp_stream, header[(int)Lumps.SurfEdges].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.Faces].Offset;
            var faces = ReadFaceLump(bsp_stream, header[(int)Lumps.Faces].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.Textures].Offset;
            var textures = ReadTextureLump(bsp_stream, header[(int)Lumps.Textures].Size, loadPackedTextures: false);
            var usedTextures = textures.Item2;
            var textureDimensions = textures.Item3;

            bsp_stream.Position = file_start + header[(int)Lumps.TextureInfo].Offset;
            var textureInfo = ReadTextureInfoLump(bsp_stream, header[(int)Lumps.TextureInfo].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.Models].Offset;
            var models = ReadModelsLump(bsp_stream, header[(int)Lumps.Models].Size);

            // Go back to start of the file
            bsp_stream.Position = file_start;

            // "Build" (load) map from loaded data
            return LoadVisualsFromData(bsp, vertices, edges, surfaceEdges, faces, usedTextures, textureDimensions, textureInfo, models);
        }

        public static bool LoadVisualsFromFile(BspFile bsp, string bsp_path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(bsp_path))
                return false;

            return LoadVisualsFromBspStream(bsp, File.Open(bsp_path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public static bool LoadVisualsFromFiles(BspFile bsp, string path_vertices, string path_edges, string path_surfEdges, string path_faces, string path_textures, string path_textureInfo, string path_models)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[(int)Lumps._Count];
            file_paths[(int)Lumps.Vertices] = path_vertices;
            file_paths[(int)Lumps.Edges] = path_edges;
            file_paths[(int)Lumps.SurfEdges] = path_surfEdges;
            file_paths[(int)Lumps.Faces] = path_faces;
            file_paths[(int)Lumps.Textures] = path_textures;
            file_paths[(int)Lumps.TextureInfo] = path_textureInfo;
            file_paths[(int)Lumps.Models] = path_models;

            return LoadVisualsFromFiles(bsp, file_paths);
        }

        public static bool LoadVisualsFromFiles(BspFile bsp, string[] file_paths)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            // Check for existence
            if (!File.Exists(file_paths[(int)Lumps.Vertices]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Edges]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.SurfEdges]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Faces]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Textures]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.TextureInfo]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Models]))
                return false;

            // Create streams
            var vertices_lump = File.Open(file_paths[(int)Lumps.Vertices], FileMode.Open, FileAccess.Read, FileShare.Read);
            var edges_lump = File.Open(file_paths[(int)Lumps.Edges], FileMode.Open, FileAccess.Read, FileShare.Read);
            var surfaceEdges_lump = File.Open(file_paths[(int)Lumps.SurfEdges], FileMode.Open, FileAccess.Read, FileShare.Read);
            var faces_lump = File.Open(file_paths[(int)Lumps.Faces], FileMode.Open, FileAccess.Read, FileShare.Read);
            var textures_lump = File.Open(file_paths[(int)Lumps.Textures], FileMode.Open, FileAccess.Read, FileShare.Read);
            var textureInfo_lump = File.Open(file_paths[(int)Lumps.TextureInfo], FileMode.Open, FileAccess.Read, FileShare.Read);
            var models_lump = File.Open(file_paths[(int)Lumps.Models], FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                // Use streams
                return LoadVisualsFromStreams(bsp, vertices_lump, edges_lump, surfaceEdges_lump, faces_lump, textures_lump, textureInfo_lump, models_lump);
            }
            finally
            {
                // Dispose streams
                vertices_lump.Dispose();
                edges_lump.Dispose();
                surfaceEdges_lump.Dispose();
                faces_lump.Dispose();
                textures_lump.Dispose();
                textureInfo_lump.Dispose();
                models_lump.Dispose();
            }
        }

        public static bool LoadVisualsFromDirectory(BspFile bsp, string directory_path, string[] file_names)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[file_names.Length];

            // Decide whether to use directory path or not
            if (string.IsNullOrEmpty(directory_path))
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(file_names[i]);
            else
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(Path.Combine(directory_path, file_names[i]));

            return LoadVisualsFromFiles(bsp, file_paths);
        }

        public static bool LoadVisualsFromDirectory(BspFile bsp, string directory_path, string file_name, string[] extensions = null)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (extensions == null)
                extensions = DefaultExtensions;
            if (extensions.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(extensions), "Invalid number of extensions. Must be same as number of lumps.");

            // Add file extensions to filenames
            var file_names = new string[(int)Lumps._Count];
            for (int i = 0; i < (int)Lumps._Count; i++)
                file_names[i] = string.Format("{0}.{1}", file_name, extensions[i]);

            return LoadVisualsFromDirectory(bsp, directory_path, file_names);
        }

        #endregion

        #region Colliders

        public static bool LoadCollidersFromData(BspFile bsp, Vector3f[] vertices, Edge[] edges, short[] markSurfaces, Leaf[] leaves, ClipNode[] clipNodes)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var collider = ClipNodesToMeshCollider(vertices, edges, markSurfaces, leaves, clipNodes);
            bsp.Colliders.Add(collider);
            return true;
        }

        public static bool LoadCollidersFromStreams(BspFile bsp, Stream vertices_lump, Stream edges_lump, Stream markSurfaces_lump, Stream leaves_lump, Stream clipNodes_lump)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var vertices = ReadVerticesLump(vertices_lump);

            var edges = ReadEdgesLump(edges_lump);

            var markSurfaces = ReadMarkSurfaceLump(markSurfaces_lump);

            var leaves = ReadLeafLump(leaves_lump);

            var clipNodes = ReadClipNodesLump(clipNodes_lump);

            return LoadCollidersFromData(bsp, vertices, edges, markSurfaces, leaves, clipNodes);
        }

        public static bool LoadCollidersFromBspStream(BspFile bsp, Stream bsp_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!bsp_stream.CanRead || !bsp_stream.CanSeek)
                return false;

            var file_start = bsp_stream.Position;

            var header = LoadBspHeader(new BinaryReader(bsp_stream));
            if (header == null)
                return false;

            foreach (var lump in header)
                if (file_start + lump.Offset + lump.Size > bsp_stream.Length)
                    return false;// Invalid header (will read out of the stream)

            bsp_stream.Position = file_start + header[(int)Lumps.Vertices].Offset;
            var vertices = ReadVerticesLump(bsp_stream, header[(int)Lumps.Vertices].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.Edges].Offset;
            var edges = ReadEdgesLump(bsp_stream, header[(int)Lumps.Edges].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.MarkSurfaces].Offset;
            var markSurfaces = ReadMarkSurfaceLump(bsp_stream, header[(int)Lumps.MarkSurfaces].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.Leaves].Offset;
            var leaves = ReadLeafLump(bsp_stream, header[(int)Lumps.Leaves].Size);

            bsp_stream.Position = file_start + header[(int)Lumps.ClipNodes].Offset;
            var clipNodes = ReadClipNodesLump(bsp_stream, header[(int)Lumps.ClipNodes].Size);

            // Go back to start of the file
            bsp_stream.Position = file_start;

            // "Build" (load) map from loaded data
            return LoadCollidersFromData(bsp, vertices, edges, markSurfaces, leaves, clipNodes);
        }

        public static bool LoadCollidersFromFile(BspFile bsp, string bsp_path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(bsp_path))
                return false;

            return LoadCollidersFromBspStream(bsp, File.Open(bsp_path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public static bool LoadCollidersFromFiles(BspFile bsp, string path_vertices, string path_edges, string path_surfEdges, string path_faces, string path_textures, string path_textureInfo, string path_models)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[(int)Lumps._Count];
            file_paths[(int)Lumps.Vertices] = path_vertices;
            file_paths[(int)Lumps.Edges] = path_edges;
            file_paths[(int)Lumps.SurfEdges] = path_surfEdges;
            file_paths[(int)Lumps.Faces] = path_faces;
            file_paths[(int)Lumps.Textures] = path_textures;
            file_paths[(int)Lumps.TextureInfo] = path_textureInfo;
            file_paths[(int)Lumps.Models] = path_models;

            return LoadCollidersFromFiles(bsp, file_paths);
        }

        public static bool LoadCollidersFromFiles(BspFile bsp, string[] file_paths)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            // Check for existence
            if (!File.Exists(file_paths[(int)Lumps.Vertices]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Edges]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.SurfEdges]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Faces]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Textures]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.TextureInfo]))
                return false;
            if (!File.Exists(file_paths[(int)Lumps.Models]))
                return false;

            // Create streams
            var vertices_lump = File.Open(file_paths[(int)Lumps.Vertices], FileMode.Open, FileAccess.Read, FileShare.Read);
            var edges_lump = File.Open(file_paths[(int)Lumps.Edges], FileMode.Open, FileAccess.Read, FileShare.Read);
            var markSurfaces_lump = File.Open(file_paths[(int)Lumps.MarkSurfaces], FileMode.Open, FileAccess.Read, FileShare.Read);
            var leaves_lump = File.Open(file_paths[(int)Lumps.Leaves], FileMode.Open, FileAccess.Read, FileShare.Read);
            var clipNodes_lump = File.Open(file_paths[(int)Lumps.ClipNodes], FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                // Use streams
                return LoadCollidersFromStreams(bsp, vertices_lump, edges_lump, markSurfaces_lump, leaves_lump, clipNodes_lump);
            }
            finally
            {
                // Dispose streams
                vertices_lump.Dispose();
                edges_lump.Dispose();
                markSurfaces_lump.Dispose();
                leaves_lump.Dispose();
                clipNodes_lump.Dispose();
            }
        }

        public static bool LoadCollidersFromDirectory(BspFile bsp, string directory_path, string[] file_names)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[file_names.Length];

            // Decide whether to use directory path or not
            if (string.IsNullOrEmpty(directory_path))
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(file_names[i]);
            else
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(Path.Combine(directory_path, file_names[i]));

            return LoadCollidersFromFiles(bsp, file_paths);
        }

        public static bool LoadCollidersFromDirectory(BspFile bsp, string directory_path, string file_name, string[] extensions = null)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (extensions == null)
                extensions = DefaultExtensions;
            if (extensions.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(extensions), "Invalid number of extensions. Must be same as number of lumps.");

            // Add file extensions to filenames
            var file_names = new string[(int)Lumps._Count];
            for (int i = 0; i < (int)Lumps._Count; i++)
                file_names[i] = string.Format("{0}.{1}", file_name, extensions[i]);

            return LoadCollidersFromDirectory(bsp, directory_path, file_names);
        }

        #endregion

        #region Entities

        // Ignores duplicies

        public static bool LoadEntitiesFromData(BspFile bsp, List<Dictionary<string, string>> entities)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            bsp.Entities.AddRange(entities);
            return true;
        }

        public static bool LoadEntitiesFromStream(BspFile bsp, Stream entities_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var entities = ReadEntityLump(entities_stream);
            return LoadEntitiesFromData(bsp, entities);
        }

        public static bool LoadEntitiesFromBspStream(BspFile bsp, Stream bsp_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!bsp_stream.CanRead || !bsp_stream.CanSeek)
                return false;

            var file_start = bsp_stream.Position;

            var header = LoadBspHeader(new BinaryReader(bsp_stream));
            if (header == null)
                return false;

            //THINK load only entities lump
            foreach (var lump in header)
                if (file_start + lump.Offset + lump.Size > bsp_stream.Length)
                    return false;// Invalid header (will read out of the stream)

            bsp_stream.Position = file_start + header[(int)Lumps.Entities].Offset;
            var entities = ReadEntityLump(bsp_stream, header[(int)Lumps.Entities].Size);

            // Go back to start of the file
            bsp_stream.Position = file_start;

            return LoadEntitiesFromData(bsp, entities);
        }

        public static bool LoadEntitiesFromFile(BspFile bsp, string bsp_path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(bsp_path))
                return false;

            return LoadEntitiesFromBspStream(bsp, File.Open(bsp_path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public static bool LoadEntitiesFromFiles(BspFile bsp, string[] file_paths)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            // Check for existence
            if (!File.Exists(file_paths[(int)Lumps.Entities]))
                return false;

            // Create streams
            var entities_lump = File.Open(file_paths[(int)Lumps.Entities], FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                // Use streams
                return LoadEntitiesFromStream(bsp, entities_lump);
            }
            finally
            {
                // Dispose streams
                entities_lump.Dispose();
            }
        }

        public static bool LoadEntitiesFromDirectory(BspFile bsp, string directory_path, string[] file_names)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[file_names.Length];

            // Decide whether to use directory path or not
            if (string.IsNullOrEmpty(directory_path))
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(file_names[i]);
            else
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(Path.Combine(directory_path, file_names[i]));

            return LoadEntitiesFromFiles(bsp, file_paths);
        }

        public static bool LoadEntitiesFromDirectory(BspFile bsp, string directory_path, string file_name, string[] extensions = null)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (extensions == null)
                extensions = DefaultExtensions;
            if (extensions.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(extensions), "Invalid number of extensions. Must be same as number of lumps.");

            // Add file extensions to filenames
            var file_names = new string[(int)Lumps._Count];
            for (int i = 0; i < (int)Lumps._Count; i++)
                file_names[i] = string.Format("{0}.{1}", file_name, extensions[i]);

            return LoadEntitiesFromDirectory(bsp, directory_path, file_names);
        }

        #endregion

        #region Packed textures

        public static bool LoadPackedTexturesFromData(BspFile bsp, List<TextureByteIndexPaletteWithMipmaps> textures)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            bsp.PackedTextures.AddRange(textures);
            return true;
        }

        public static bool LoadPackedTexturesFromStream(BspFile bsp, Stream textures_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var tuple = ReadTextureLump(textures_stream);
            return LoadPackedTexturesFromData(bsp, tuple.Item1);
        }

        public static bool LoadPackedTexturesFromBspStream(BspFile bsp, Stream bsp_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!bsp_stream.CanRead || !bsp_stream.CanSeek)
                return false;

            var file_start = bsp_stream.Position;

            var header = LoadBspHeader(new BinaryReader(bsp_stream));
            if (header == null)
                return false;

            //THINK load only entities lump
            foreach (var lump in header)
                if (file_start + lump.Offset + lump.Size > bsp_stream.Length)
                    return false;// Invalid header (will read out of the stream)

            bsp_stream.Position = file_start + header[(int)Lumps.Textures].Offset;
            var tuple = ReadTextureLump(bsp_stream, header[(int)Lumps.Textures].Size);

            // Go back to start of the file
            bsp_stream.Position = file_start;

            return LoadPackedTexturesFromData(bsp, tuple.Item1);
        }

        public static bool LoadPackedTexturesFromFile(BspFile bsp, string bsp_path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(bsp_path))
                return false;

            return LoadPackedTexturesFromBspStream(bsp, File.Open(bsp_path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public static bool LoadPackedTexturesFromFiles(BspFile bsp, string[] file_paths)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            // Check for existence
            if (!File.Exists(file_paths[(int)Lumps.Textures]))
                return false;

            // Create streams
            var textures_lump = File.Open(file_paths[(int)Lumps.Textures], FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                // Use streams
                return LoadPackedTexturesFromStream(bsp, textures_lump);
            }
            finally
            {
                // Dispose streams
                textures_lump.Dispose();
            }
        }

        public static bool LoadPackedTexturesFromDirectory(BspFile bsp, string directory_path, string[] file_names)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[file_names.Length];

            // Decide whether to use directory path or not
            if (string.IsNullOrEmpty(directory_path))
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(file_names[i]);
            else
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(Path.Combine(directory_path, file_names[i]));

            return LoadPackedTexturesFromFiles(bsp, file_paths);
        }

        public static bool LoadPackedTexturesFromDirectory(BspFile bsp, string directory_path, string file_name, string[] extensions = null)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (extensions == null)
                extensions = DefaultExtensions;
            if (extensions.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(extensions), "Invalid number of extensions. Must be same as number of lumps.");

            // Add file extensions to filenames
            var file_names = new string[(int)Lumps._Count];
            for (int i = 0; i < (int)Lumps._Count; i++)
                file_names[i] = string.Format("{0}.{1}", file_name, extensions[i]);

            return LoadPackedTexturesFromDirectory(bsp, directory_path, file_names);
        }

        #endregion

        #region Load by enum

        public static bool LoadAllFromBspStream(BspFile bsp, LoadFlags flags, Stream bsp_stream)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!bsp_stream.CanRead || !bsp_stream.CanSeek)
                return false;

            var file_start = bsp_stream.Position;

            var header = LoadBspHeader(new BinaryReader(bsp_stream));
            if (header == null)
                return false;

            foreach (var lump in header)
                if (file_start + lump.Offset + lump.Size > bsp_stream.Length)
                    return false;// Invalid header (will read out of the stream)

            bool done = true;

            Vector3f[] vertices = null;
            Edge[] edges = null;

            if (flags.HasFlag(LoadFlags.Visuals))
            {
                if (vertices == null)
                {
                    bsp_stream.Position = file_start + header[(int)Lumps.Vertices].Offset;
                    vertices = ReadVerticesLump(bsp_stream, header[(int)Lumps.Vertices].Size);
                }

                if (edges == null)
                {
                    bsp_stream.Position = file_start + header[(int)Lumps.Edges].Offset;
                    edges = ReadEdgesLump(bsp_stream, header[(int)Lumps.Edges].Size);
                }

                bsp_stream.Position = file_start + header[(int)Lumps.SurfEdges].Offset;
                var surfaceEdges = ReadSurfaceEdgesLump(bsp_stream, header[(int)Lumps.SurfEdges].Size);

                bsp_stream.Position = file_start + header[(int)Lumps.Faces].Offset;
                var faces = ReadFaceLump(bsp_stream, header[(int)Lumps.Faces].Size);

                bsp_stream.Position = file_start + header[(int)Lumps.Textures].Offset;
                var textures = ReadTextureLump(bsp_stream, header[(int)Lumps.Textures].Size, false);
                var usedTextures = textures.Item2;
                var textureDimensions = textures.Item3;

                bsp_stream.Position = file_start + header[(int)Lumps.TextureInfo].Offset;
                var textureInfo = ReadTextureInfoLump(bsp_stream, header[(int)Lumps.TextureInfo].Size);

                bsp_stream.Position = file_start + header[(int)Lumps.Models].Offset;
                var models = ReadModelsLump(bsp_stream, header[(int)Lumps.Models].Size);

                done &= LoadVisualsFromData(bsp, vertices, edges, surfaceEdges, faces, usedTextures, textureDimensions, textureInfo, models);
            }

            if (flags.HasFlag(LoadFlags.Colliders))
            {
                if (vertices == null)
                {
                    bsp_stream.Position = file_start + header[(int)Lumps.Vertices].Offset;
                    vertices = ReadVerticesLump(bsp_stream, header[(int)Lumps.Vertices].Size);
                }

                if (edges == null)
                {
                    bsp_stream.Position = file_start + header[(int)Lumps.Edges].Offset;
                    edges = ReadEdgesLump(bsp_stream, header[(int)Lumps.Edges].Size);
                }

                bsp_stream.Position = file_start + header[(int)Lumps.MarkSurfaces].Offset;
                var markSurfaces = ReadMarkSurfaceLump(bsp_stream, header[(int)Lumps.MarkSurfaces].Size);

                bsp_stream.Position = file_start + header[(int)Lumps.Leaves].Offset;
                var leaves = ReadLeafLump(bsp_stream, header[(int)Lumps.Leaves].Size);

                bsp_stream.Position = file_start + header[(int)Lumps.ClipNodes].Offset;
                var clipNodes = ReadClipNodesLump(bsp_stream, header[(int)Lumps.ClipNodes].Size);

                done &= LoadCollidersFromData(bsp, vertices, edges, markSurfaces, leaves, clipNodes);
            }

            if (flags.HasFlag(LoadFlags.Entities))
            {
                bsp_stream.Position = file_start + header[(int)Lumps.Entities].Offset;
                var entities = ReadEntityLump(bsp_stream, header[(int)Lumps.Entities].Size);

                done &= LoadEntitiesFromData(bsp, entities);
            }

            if (flags.HasFlag(LoadFlags.PackedTextures))
            {
                bsp_stream.Position = file_start + header[(int)Lumps.Textures].Offset;
                var tuple = ReadTextureLump(bsp_stream, header[(int)Lumps.Textures].Size);

                done &= LoadPackedTexturesFromData(bsp, tuple.Item1);
            }

            // Go back to start of the file
            bsp_stream.Position = file_start;

            return done;
        }

        public static bool LoadAllFromFile(BspFile bsp, LoadFlags flags, string bsp_path)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (!File.Exists(bsp_path))
                return false;

            return LoadAllFromBspStream(bsp, flags, File.Open(bsp_path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public static bool LoadAllFromFiles(BspFile bsp, LoadFlags flags, string[] file_paths)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            bool done = true;

            if (flags.HasFlag(LoadFlags.Visuals))
                done &= LoadVisualsFromFiles(bsp, file_paths);

            if (flags.HasFlag(LoadFlags.Colliders))
                done &= LoadCollidersFromFiles(bsp, file_paths);

            if (flags.HasFlag(LoadFlags.Entities))
                done &= LoadEntitiesFromFiles(bsp, file_paths);

            if (flags.HasFlag(LoadFlags.PackedTextures))
                done &= LoadPackedTexturesFromFiles(bsp, file_paths);

            return done;
        }

        public static bool LoadAllFromDirectory(BspFile bsp, LoadFlags flags, string directory_path, string[] file_names)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            var file_paths = new string[file_names.Length];

            // Decide whether to use directory path or not
            if (string.IsNullOrEmpty(directory_path))
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(file_names[i]);
            else
                for (int i = 0; i < file_paths.Length; i++)
                    file_paths[i] = Path.GetFullPath(Path.Combine(directory_path, file_names[i]));

            return LoadAllFromFiles(bsp, flags, file_paths);
        }

        public static bool LoadAllFromDirectory(BspFile bsp, LoadFlags flags, string directory_path, string file_name, string[] extensions = null)
        {
            if (bsp == null)
                throw new ArgumentNullException(nameof(bsp), "Target BSP is null.");

            if (extensions == null)
                extensions = DefaultExtensions;
            if (extensions.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(extensions), "Invalid number of extensions. Must be same as number of lumps.");

            // Add file extensions to filenames
            var file_names = new string[(int)Lumps._Count];
            for (int i = 0; i < (int)Lumps._Count; i++)
                file_names[i] = string.Format("{0}.{1}", file_name, extensions[i]);

            return LoadAllFromDirectory(bsp, flags, directory_path, file_names);
        }

        #endregion

        #region Visibility

        //TODO Visibility

        #endregion

        #region Mark Surface

        public static short[] ReadMarkSurfaceLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / sizeof(short));
            var arr = new short[count];

            for (int i = 0; i < count; i++)
                arr[i] = reader.ReadInt16();

            return arr;
        }

        #endregion

        #region Clip Nodes

        public class ClipNode
        {
            public ClipNode(int planeIndex, short children0, short children1)
            {
                this.PlaneIndex = planeIndex;

                this.Children0 = children0;
                this.Children1 = children1;
            }

            /// <summary>
            /// Index into planes
            /// </summary>
            public int PlaneIndex;

            /// <summary>
            /// Negative numbers are contents
            /// </summary>
            public short Children0, Children1;

            public const int MemorySize = sizeof(int) + 2 * sizeof(short);
        }

        public static ClipNode[] ReadClipNodesLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / ClipNode.MemorySize);

            var arr = new ClipNode[count];

            for (int i = 0; i < count; i++)
                arr[i] = new ClipNode(reader.ReadInt32(), reader.ReadInt16(), reader.ReadInt16());

            return arr;
        }

        #endregion

        #region Lighting

        public static Bitmap CreateLightingBitmap(byte[] lightmap, int offset, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int off = offset + (x + (y * width)) * 3;

                    byte r = lightmap[off];
                    byte g = lightmap[off + 1];
                    byte b = lightmap[off + 2];
                    var c = Color.FromArgb(r, g, b);
                    bitmap.SetPixel(x, y, c);
                }
            }
            return bitmap;
        }

        public static Color[,] CreateLighting(byte[] lightmap, int offset, int width, int height)
        {
            var bitmap = new Color[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int off = offset + (x + (y * width)) * 3;

                    byte r = lightmap[off];
                    byte g = lightmap[off + 1];
                    byte b = lightmap[off + 2];
                    var c = Color.FromArgb(r, g, b);
                    bitmap[x, y] = c;
                }
            }
            return bitmap;
        }

        public static byte[] ReadLightingLump(Stream stream, long length = -1)
        {
            if (length < 0)
                length = stream.Length - stream.Position;
            if (length > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(length));

            int length_i = (int)length;
            var bytes = new byte[length_i];
            stream.Read(bytes, 0, length_i);
            return bytes;
        }

        #endregion

        #region Leaves / Leaf

        public enum LeafContent
        {
            Empty = -1,
            Solid = -2,
            Water = -3,
            Slime = -4,
            Lava = -5,
            Sky = -6,
            Origin = -7,
            Clip = -8,
            Current0 = -9,
            Current90 = -10,
            Current180 = -11,
            Current270 = -12,
            CurrentUp = -13,
            CurrentDown = -14,
            Translucent = -15
        }

        public class Leaf
        {
            public LeafContent Content;
            public int VisOffset = -1;
            /// <summary>
            /// Length = 3
            /// </summary>
            public short[] bb_min, bb_max;
            public ushort FirstMarkSurface, MarkSurfaceCount;
            /// <summary>
            /// Length = 4
            /// </summary>
            public byte[] AmbientLevels;

            public const int MemorySize = sizeof(int) * 2 + sizeof(short) * (2 * 3) + sizeof(ushort) * 2 + sizeof(byte) * 4;
        }

        public static Leaf[] ReadLeafLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / Leaf.MemorySize);

            var arr = new Leaf[count];

            for (int i = 0; i < count; i++)
            {
                var leaf = new Leaf()
                {
                    Content = (LeafContent)reader.ReadInt32(),
                    VisOffset = reader.ReadInt32(),

                    bb_min = new[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
                    bb_max = new[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },

                    FirstMarkSurface = reader.ReadUInt16(),
                    MarkSurfaceCount = reader.ReadUInt16(),

                    AmbientLevels = reader.ReadBytes(4)
                };
                arr[i] = leaf;
            }

            return arr;
        }

        #endregion

        #region BspNode

        public static BspNode[] ReadBspNodeLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / BspNode.MemorySize);

            var arr = new BspNode[count];
            for (int i = 0; i < count; i++)
            {
                var node = new BspNode(
                    reader.ReadUInt32(),
                    new[] { reader.ReadInt16(), reader.ReadInt16() },
                    new[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
                    new[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
                    reader.ReadUInt16(),
                    reader.ReadUInt16()
                );
                arr[i] = node;
            }

            return arr;
        }

        public class BspNode
        {
            public BspNode(uint planeLumpIndex, short[] children, short[] bbMin, short[] bbMax, ushort firstFace, ushort faceCount) : this(planeLumpIndex, children[0], children[1], bbMin, bbMax, firstFace, faceCount)
            {
                if (children.Length != 2)
                    throw new ArgumentOutOfRangeException(nameof(children));
            }

            public BspNode(uint planeLumpIndex, short children0, short children1, short[] bbMin, short[] bbMax, ushort firstFace, ushort faceCount)
            {
                if (bbMin.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(bbMin));
                if (bbMax.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(bbMax));

                this.PlaneLumpIndex = planeLumpIndex;

                this.Children0 = children0;
                this.Children1 = children1;

                this.bbMin = bbMin;
                this.bbMax = bbMax;

                this.FirstFace = firstFace;
                this.FaceCount = faceCount;
            }

            public uint PlaneLumpIndex;

            /// <summary>
            /// If the index is larger than 0, the index indicates a child node. If it is equal to or smaller than zero (no valid array index), the bitwise inversed value of the index gives an index into the leaves lump.
            /// </summary>
            public short Children0, Children1;

            /// <summary>
            /// Length = 3
            /// </summary>
            public short[] bbMin, bbMax;

            public ushort FirstFace, FaceCount;

            public const int MemorySize = sizeof(uint) + sizeof(short) * (2 * 3 + 2) + sizeof(ushort) * 2;
        }

        #endregion

        #region Changing to render format

        public static void CreateCollisionPolygons(IList<Vector3f> points, IList<Polygon> polygons, int firstMarkSurface, int markSurfaceCount, Vector3f[] vertices, Edge[] edges, short[] markSurfaces)
        {
            var vertex_indices = new List<int>();
            for (int msi = 0; msi < markSurfaceCount; msi++)
            {
                var mse = markSurfaces[(int)(firstMarkSurface + msi)];
                var edge = edges[Math.Abs(mse)];
                var index = mse >= 0 ? edge.Point0 : edge.Point1;
                vertex_indices.Add(index);
            }

            for (int v = 0; v < vertex_indices.Count; v++)
            {
                var index = vertex_indices[v];
                var position = vertices[index];

                if (!points.Contains(position))
                    points.Add(position);

                vertex_indices[v] = points.IndexOf(position);
            }

            polygons.Add(new Polygon("clip", vertex_indices.ToArray()));
        }

        public static Tuple<Vector3f[], Polygon[]> ClipNodesToPolygons(Vector3f[] vertices, Edge[] edges, short[] markSurfaces, Leaf[] leaves, ClipNode[] clipNodes)
        {
            var leafIndices = new List<int>();
            foreach (var cn in clipNodes)
            {
                if (cn.Children0 < 0)
                    if (!leafIndices.Contains(~cn.Children0))
                        leafIndices.Add(~cn.Children0);
                if (cn.Children1 < 0)
                    if (!leafIndices.Contains(~cn.Children1))
                        leafIndices.Add(~cn.Children1);
            }

            var points = new List<Vector3f>();
            var polygons = new List<Polygon>();

            foreach (var li in leafIndices)
            {
                var leaf = leaves[li];
                CreateCollisionPolygons(points, polygons, leaf.FirstMarkSurface, leaf.MarkSurfaceCount, vertices, edges, markSurfaces);
            }

            return new Tuple<Vector3f[], Polygon[]>(points.ToArray(), polygons.ToArray());
        }

        public static Tuple<Vector3f[], uint[]> ClipNodesToTriangles(Vector3f[] vertices, Edge[] edges, short[] markSurfaces, Leaf[] leaves, ClipNode[] clipNodes)
        {
            var tuple = ClipNodesToPolygons(vertices, edges, markSurfaces, leaves, clipNodes);

            var triangles = new List<uint>();
            for (int i = 0; i < tuple.Item2.Length; i++)
                triangles.AddRange(tuple.Item2[i].ToTriangles());

            return new Tuple<Vector3f[], uint[]>(tuple.Item1, triangles.ToArray());
        }

        public static Colliders.MeshCollider ClipNodesToMeshCollider(Vector3f[] vertices, Edge[] edges, short[] markSurfaces, Leaf[] leaves, ClipNode[] clipNodes)
        {
            var tuple = ClipNodesToTriangles(vertices, edges, markSurfaces, leaves, clipNodes);

            return new Colliders.MeshCollider(tuple.Item1, tuple.Item2);
        }

        public static void CreateVisualPolygons(IList<Point> points, IList<Polygon> polygons, int firstFace, int faceCount, Vector3f[] vertices, Edge[] edges, int[] surface_edges, Face[] faces, TextureInfo[] texture_info, string[] textures_used, Dictionary<string, Vector2> texture_dimensions)
        {
            for (int f = 0; f < faceCount; f++)
            {
                var fi = firstFace + f;
                var face = faces[fi];
                var texture = texture_info[face.TextureInfo];
                var textureName = textures_used[(int)texture.TextureID];
                var textureSize = texture_dimensions[textureName];

                var vertex_indices = new List<int>();
                for (int ei = 0; ei < face.EdgeCount; ei++)
                {
                    var se = surface_edges[(int)(face.FirstEdge + ei)];
                    var edge = edges[Math.Abs(se)];
                    var index = se >= 0 ? edge.Point0 : edge.Point1;
                    vertex_indices.Add(index);
                }

                for (int v = 0; v < vertex_indices.Count; v++)
                {
                    var index = vertex_indices[v];
                    var position = vertices[index];
                    var uv = texture.GetTexelUV(position, textureSize);

                    var point = new Point(position, uv);
                    if (!points.Contains(point))
                        points.Add(point);

                    vertex_indices[v] = points.IndexOf(point);
                }

                polygons.Add(new Polygon(textureName, vertex_indices.ToArray()));
            }
        }

        public static Model ModelDataToPolygon(ModelData model, Vector3f[] vertices, Edge[] edges, int[] surface_edges, Face[] faces, TextureInfo[] texture_info, string[] textures_used, Dictionary<string, Vector2> texture_dimensions)
        {
            var points = new List<Point>();
            var polygons = new List<Polygon>();

            CreateVisualPolygons(points, polygons, model.FirstFace, model.FaceCount, vertices, edges, surface_edges, faces, texture_info, textures_used, texture_dimensions);

            var points_position = new List<Vector3f>();
            var points_uv = new List<Vector2f>();
            for (int p = 0; p < points.Count; p++)
            {
                var point = points[p];
                points_position.Add(point.Position);
                points_uv.Add(point.UV);
            }

            var sorted_polygons = Model.GroupPolygonsByTexture(polygons);
            var triangles = Model.PolygonsToTrianglesPoints(sorted_polygons);
            return new Model(triangles, points_position.ToArray(), points_uv.ToArray());
        }

        public class Point
        {
            public Point(Vector3f position, Vector2f uv)
            {
                this.Position = position;
                this.UV = uv;
            }

            public readonly Vector3f Position;
            public readonly Vector2f UV;

            public override string ToString()
            {
                return string.Format("<{0};{1}>", Position, UV);
            }

            public override bool Equals(object obj)
            {
                return obj is Point && Equals(this, (Point)obj);
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode() + UV.GetHashCode();
            }

            public static bool Equals(Point p0, Point p1)
            {
                if (p0 == null && p1 == null)
                    return true;
                if (p0 == null || p1 == null)
                    return false;
                return p0.Position == p1.Position && p0.UV == p1.UV;
            }
        }

        #endregion

        #region MemoryStream from BinaryReader

        private static MemoryStream CreateStream(BinaryReader reader, Lump lump) => CreateStream(reader, lump.Offset, lump.Size);

        private static MemoryStream CreateStream(BinaryReader reader, int offset, int length)
        {
            reader.BaseStream.Position = offset;
            return CreateStream(reader, length);
        }

        public static MemoryStream CreateStream(BinaryReader reader, int length)
        {
            return new MemoryStream(reader.ReadBytes(length));
        }

        #endregion

        #region Entity Lump

        public static List<Dictionary<string, string>> ReadEntityLump(Stream stream, long length = -1)
        {
            List<Dictionary<string, string>> entities = new List<Dictionary<string, string>>();

            if (length < 0)
                length = stream.Length - stream.Position;
            if (length > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(length));

            using (var mem_stream = CreateStream(new BinaryReader(stream), (int)length))
            {
                var reader = new StreamReader(mem_stream, System.Text.Encoding.ASCII);
                Dictionary<string, string> current = new Dictionary<string, string>();

                string line;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    //Console.WriteLine(line);
                    if (line == "{")
                        current = new Dictionary<string, string>();
                    else if (line == "}")
                    {
                        entities.Add(current);
                        current = null;
                    }
                    else
                    {
                        int key_start = line.IndexOf('"') + 1;
                        if (key_start == 0)
                            continue;

                        int key_length = line.IndexOf('"', key_start) - key_start;
                        if (key_length < 0)
                            continue;
                        string key = line.Substring(key_start, key_length);

                        int value_start = line.IndexOf('"', key_start + key_length + 1) + 1;
                        if (value_start == 0)
                            continue;

                        int value_length = line.IndexOf('"', value_start) - value_start;
                        if (key_length < 0)
                            continue;

                        string value = line.Substring(value_start, value_length);

                        //current.Add(key, value);
                        current[key] = value;
                    }
                }
            }

            return entities;
        }

        #endregion

        #region Texture Lump

        public static Tuple<List<TextureByteIndexPaletteWithMipmaps>, string[], Dictionary<string, Vector2>> ReadTextureLump(Stream stream, long length = -1, bool loadPackedTextures = true)
        {
            var reader = new BinaryReader(stream);

            var startPos = stream.Position;

            if (length < 0)
                length = stream.Length - stream.Position;

            uint count = reader.ReadUInt32();

            int[] offsets = new int[count];
            for (int i = 0; i < count; i++)
                offsets[i] = reader.ReadInt32();

            var texturesUsedNames = new string[count];
            var texturesPacked = new List<TextureByteIndexPaletteWithMipmaps>();
            var textureDimensions = new Dictionary<string, Vector2>();

            for (int i = 0; i < count; i++)
            {
                long texture_offset = offsets[i] + startPos;
                reader.BaseStream.Position = texture_offset;
                var texture_info = TextureLumpInfo.Load(reader);

                texturesUsedNames[i] = texture_info.Name_s;

                textureDimensions[texture_info.Name_s] = new Vector2((int)texture_info.Width, (int)texture_info.Height);

                if (loadPackedTextures)
                    if (texture_info.MipmapOffsets[0] != 0)
                        texturesPacked.Add(texture_info.LoadTexture(reader, texture_offset));
            }

            return new Tuple<List<TextureByteIndexPaletteWithMipmaps>, string[], Dictionary<string, Vector2>>(texturesPacked, texturesUsedNames, textureDimensions);
        }


        #endregion

        #region Texture Info Lump

        public static TextureInfo[] ReadTextureInfoLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / TextureInfo.MemorySize);

            var arr = new TextureInfo[count];

            for (int i = 0; i < count; i++)
                arr[i] = new TextureInfo(
                    new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    reader.ReadSingle(),
                    new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    reader.ReadSingle(),
                    reader.ReadUInt32(),
                    (TexInfoFlags)reader.ReadUInt32()
                );

            return arr;
        }

        public struct TextureInfo
        {
            public TextureInfo(Vector3f s, float s_shift, Vector3f t, float t_shift, uint textureID, TexInfoFlags flags)
            {
                this.S = s;
                this.SShift = s_shift;

                this.T = t;
                this.TShift = t_shift;

                this.TextureID = textureID;
                this.Flags = flags;
            }

            public Vector3f S;
            /// <summary>
            /// Texture shift in s direction
            /// </summary>
            public float SShift;
            public Vector3f T;
            /// <summary>
            /// Texture shift in s direction
            /// </summary>
            public float TShift;

            /// <summary>
            /// Index into textures array
            /// </summary>
            public UInt32 TextureID;

            /// <summary>
            /// Seem to always be 0
            /// </summary>
            public TexInfoFlags Flags;

            public const int MemorySize = (2 * 4) * sizeof(float) + sizeof(uint) + sizeof(uint);

            public Vector2f GetTexelUV(Vector3f position, Vector2 textureSize)
            {
                return new Vector2f(GetTexelU(position, textureSize), GetTexelV(position, textureSize));
            }

            public float GetTexelU(Vector3f position, Vector2 textureSize)
            {
                return (S.X * position.X + S.Y * position.Y + S.Z * position.Z + SShift) / textureSize.X;
            }

            public float GetTexelV(Vector3f position, Vector2 textureSize)
            {
                return (T.X * position.X + T.Y * position.Y + T.Z * position.Z + TShift) / textureSize.Y;
            }
        }

        [Flags]
        public enum TexInfoFlags
        {
            /// <summary>
            /// value will hold the light strength
            /// </summary>
            Light = 0x1,
            /// <summary>
            /// don't draw, indicates we should skylight + draw 2d sky but not draw the 3D skybox
            /// </summary>
            Sky2D = 0x2,
            /// <summary>
            /// don't draw, but add to skybox
            /// </summary>
            Sky = 0x4,
            /// <summary>
            /// turbulent water warp
            /// </summary>
            Warp = 0x8,
            /// <summary>
            /// ??? UNKNOWN ???
            /// Probably transparent / transparency
            /// </summary>
            Trans = 0x10,
            /// <summary>
            /// the surface can not have a portal placed on it
            /// </summary>
            NoPortal = 0x20,
            /// <summary>
            /// This is an xbox hack to work around elimination of trigger surfaces, which breaks occluders
            /// </summary>
            Trigger = 0x40,
            /// <summary>
            /// don't bother referencing the texture
            /// </summary>
            NoDraw = 0x80,
            /// <summary>
            /// make a primary bsp splitter
            /// </summary>
            Hint = 0x100,
            /// <summary>
            /// completely ignore, allowing non-closed brushes
            /// </summary>
            SkipLight = 0x200,
            /// <summary>
            /// Don't calculate light
            /// </summary>
            NoLight = 0x400,
            /// <summary>
            /// calculate three lightmaps for the surface for bumpmapping
            /// </summary>
            BumpLight = 0x800,
            /// <summary>
            /// Don't receive shadows
            /// </summary>
            NoShadows = 0x1000,
            /// <summary>
            /// Don't receive decals
            /// </summary>
            NoDefals = 0x2000,
            /// <summary>
            /// Don't subdivide patches on this surface
            /// </summary>
            NoChop = 0x4000,
            /// <summary>
            /// surface is part of a hitbox
            /// </summary>
            Hitbox = 0x8000
        }

        #endregion

        #region Vertices Lump

        public static Vector3f[] ReadVerticesLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / Vector3f.MemorySize);

            var arr = new Vector3f[count];

            for (int i = 0; i < count; i++)
                arr[i] = new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            return arr;
        }

        #endregion

        #region Models Lump

        public static ModelData[] ReadModelsLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / ModelData.MemorySize);

            var arr = new ModelData[count];

            for (int i = 0; i < count; i++)
                arr[i] = new ModelData(
                    new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    new int[] { reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32() },
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32()
                );

            return arr;
        }

        public class ModelData
        {
            public ModelData(Vector3f mins, Vector3f maxs, Vector3f origin, int[] headnodes, int visLeafs, int firstFace, int faceCount)
            {
                this.Mins = mins;
                this.Maxs = maxs;

                this.Origin = origin;

                this.Headnodes = headnodes;
                this.VisLeafs = visLeafs;

                this.FirstFace = firstFace;
                this.FaceCount = faceCount;
            }

            public Vector3f Mins, Maxs;

            public Vector3f Origin;

            /// <summary>
            /// Length = <see cref="HeadnodesLength"/> (4)
            /// </summary>
            public int[] Headnodes;
            public const int HeadnodesLength = 4;

            public int VisLeafs;

            public int FirstFace, FaceCount;

            public const int MemorySize = 3 * 3 * sizeof(float) + ModelData.HeadnodesLength * sizeof(int) + 3 * sizeof(int);
        }

        #endregion

        #region Plane Lump

        public static PlaneData[] ReadPlaneLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / PlaneData.MemorySize);

            var arr = new PlaneData[count];

            for (int i = 0; i < count; i++)
                arr[i] = new PlaneData(
                    new Vector3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                    reader.ReadSingle(),
                    (PlaneType)reader.ReadInt32()
                );

            return arr;
        }

        public struct PlaneData
        {
            public PlaneData(Vector3f normal, float distance, PlaneType type)
            {
                this.Normal = normal;
                this.Distance = distance;
                this.Type = type;
            }

            public Vector3f Normal;
            public float Distance;
            public PlaneType Type;

            public const int MemorySize = 4 * sizeof(float) + sizeof(int);
        }

        /// <summary>
        /// If PlaneType equals PlaneType.X, then the normal of the plane will be parallel to the X axis, meaning the plane is perpendicular to the X axis.
        /// If PlaneType equals PlaneType.AnyX, then the plane's normal is nearer to the X axis then to any other axis.
        /// </summary>
        public enum PlaneType
        {
            X = 0,
            Y = 1,
            Z = 2,
            AnyX = 3,
            AnyY = 4,
            AnyZ = 5
        }

        #endregion

        #region Edges Lump

        public static Edge[] ReadEdgesLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / Edge.MemorySize);

            var arr = new Edge[count];

            for (int i = 0; i < count; i++)
                arr[i] = new Edge(
                    reader.ReadUInt16(),
                    reader.ReadUInt16()
                );

            return arr;
        }

        public struct Edge
        {
            public Edge(ushort p0, ushort p1)
            {
                this.Point0 = p0;
                this.Point1 = p1;
            }

            public ushort Point0;
            public ushort Point1;

            public const int MemorySize = 2 * sizeof(ushort);
        }

        #endregion

        #region Surface Edges Lump

        public static int[] ReadSurfaceEdgesLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / sizeof(int));

            var arr = new int[count];

            for (int i = 0; i < count; i++)
                arr[i] = reader.ReadInt32();

            return arr;
        }

        #endregion

        #region Face Lump

        public static Face[] ReadFaceLump(Stream stream, long length = -1)
        {
            var reader = new BinaryReader(stream);

            if (length < 0)
                length = stream.Length - stream.Position;

            int count = (int)(length / Face.MemorySize);

            var arr = new Face[count];

            for (int i = 0; i < count; i++)
                arr[i] = new Face(
                    reader.ReadUInt16(),
                    reader.ReadUInt16(),
                    reader.ReadUInt32(),
                    reader.ReadUInt16(),
                    reader.ReadUInt16(),
                    new byte[Face.StylesLength]
                    {
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte()
                    },
                    reader.ReadUInt32()
                );

            return arr;
        }
        public class Face
        {
            public Face(ushort plane, ushort planeSide, uint firstEdge, ushort edges, ushort textureInfo, byte[] styles, uint lightmapOffset)
            {
                this.Plane = plane;
                this.PlaneSide = planeSide;

                this.FirstEdge = firstEdge;
                this.EdgeCount = edges;

                this.TextureInfo = textureInfo;

                if (styles == null)
                    throw new ArgumentNullException(nameof(styles));
                if (styles.Length != StylesLength)
                    throw new ArgumentOutOfRangeException(nameof(styles));

                this.Styles = styles;

                this.LightmapOffset = lightmapOffset;
            }

            /// <summary>
            /// Plane the face is parallel to
            /// </summary>
            public ushort Plane;
            /// <summary>
            /// Set if different normals orientation<br>
            /// 0 = normal vector is same<br>
            /// 1 = normal vector multipled by -1
            /// </summary>
            public ushort PlaneSide;
            /// <summary>
            /// Index of the  first surfedge
            /// </summary>
            public uint FirstEdge;
            /// <summary>
            /// Number of consecutive surfedges
            /// </summary>
            public ushort EdgeCount;
            /// <summary>
            /// Index of the texture info structure
            /// </summary>
            public ushort TextureInfo;
            /// <summary>
            /// Length = StylesLength<br>
            /// Specify lighting styles
            /// </summary>
            public byte[] Styles;
            public const int StylesLength = 4;
            /// <summary>
            /// Offsets into the raw lightmap data
            /// </summary>
            public uint LightmapOffset;

            public const int MemorySize = 2 * sizeof(ushort) + sizeof(uint) + 2 * sizeof(ushort) + Face.StylesLength * sizeof(byte) + sizeof(uint);
        }

        #endregion

        #region Parts

        public static string[] DefaultExtensions
        {
            get
            {
                var ext = new string[(int)Lumps._Count];
                ext[0] = "entities";
                ext[1] = "planes";
                ext[2] = "textures";
                ext[3] = "vertices";
                ext[4] = "visibility";
                ext[5] = "nodes";
                ext[6] = "texture_info";
                ext[7] = "faces";
                ext[8] = "lighting";
                ext[9] = "clip_nodes";
                ext[10] = "leaves";
                ext[11] = "mark_surfaces";
                ext[12] = "edges";
                ext[13] = "surf_edges";
                ext[14] = "models";
                return ext;
            }
        }

        #region Splitting into files

        public static void SplitBspIntoFiles(string bspPath, string folder) => SplitBspIntoFiles(bspPath, folder, DefaultExtensions);

        public static void SplitBspIntoFiles(string bspPath, string folder, string[] extensions)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(bspPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                SplitBspIntoFiles(reader, Path.GetFileNameWithoutExtension(bspPath), folder, extensions);
            }
        }

        public static void SplitBspIntoFiles(BinaryReader reader, string name, string folder) => SplitBspIntoFiles(reader, name, folder, DefaultExtensions);

        public static void SplitBspIntoFiles(BinaryReader reader, string name, string folder, string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            if (extensions.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(extensions));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var startPosition = reader.BaseStream.Position;

            // Version
            {
                var version = reader.ReadUInt32();
                if (version != Version)
                    throw new BspLib.Bsp.Exceptions.BspVersionNotSupportedException(version);
            }

            Lump[] lumps = new Lump[(int)Lumps._Count];
            for (int i = 0; i < lumps.Length; i++)
                lumps[i] = new Lump(reader.ReadInt32(), reader.ReadInt32());

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            for (int i = 0; i < (int)Lumps._Count; i++)
            {
                var ext = extensions[i];
                var path = Path.Combine(folder, string.Format("{0}.{1}", name, ext));

                var lump = lumps[i];
                reader.BaseStream.Position = startPosition + lump.Offset;

                using (var stream = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                {
                    CopyStream(reader.BaseStream, stream, lump.Size);
                    stream.Flush();
                }
            }
        }

        #endregion

        #region Building from files

        //TODO <write>, folder, filename, string[] extensions

        #region Into file

        public static void BuildBspFromFiles(string writeFile, string folder, string[] files)
        {
            using (var writer = new BinaryWriter(File.Open(writeFile, FileMode.CreateNew, FileAccess.Write, FileShare.None)))
            {
                BuildBspFromFiles(writer, files);
            }
        }
        public static void BuildBspFromFiles(string writeFile, string[] files)
        {
            using (var writer = new BinaryWriter(File.Open(writeFile, FileMode.CreateNew, FileAccess.Write, FileShare.None)))
            {
                BuildBspFromFiles(writer, files);
            }
        }
        public static void BuildBspFromFiles(string writeFile, BinaryReader[] files)
        {
            using (var writer = new BinaryWriter(File.Open(writeFile, FileMode.CreateNew, FileAccess.Write, FileShare.None)))
            {
                BuildBspFromFiles(writer, files);
            }
        }
        public static void BuildBspFromFiles(string writeFile, Stream[] files)
        {
            using (var writer = new BinaryWriter(File.Open(writeFile, FileMode.CreateNew, FileAccess.Write, FileShare.None)))
            {
                BuildBspFromFiles(writer, files);
            }
        }

        #endregion

        #region Into binary writer

        public static void BuildBspFromFiles(BinaryWriter writer, string folder, string[] files)
        {
            var fs = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
                fs[i] = Path.Combine(folder, files[i]);

            BuildBspFromFiles(writer, files);
        }
        public static void BuildBspFromFiles(BinaryWriter writer, string[] files)
        {
            var readers = new BinaryReader[files.Length];
            for (int i = 0; i < files.Length; i++)
                readers[i] = new BinaryReader(File.Open(files[i], FileMode.Open, FileAccess.Read, FileShare.Read));

            BuildBspFromFiles(writer, readers);

            foreach (var r in readers)
                r.Dispose();
        }
        public static void BuildBspFromFiles(BinaryWriter writer, BinaryReader[] files)
        {
            var streams = new Stream[files.Length];
            for (int i = 0; i < files.Length; i++)
                streams[i] = files[i].BaseStream;

            BuildBspFromFiles(writer, streams);
        }
        public static void BuildBspFromFiles(BinaryWriter writer, Stream[] files)
        {
            if (files.Length != (int)Lumps._Count)
                throw new ArgumentOutOfRangeException(nameof(files));

            var startPos = writer.BaseStream.Position;
            writer.Write(Version);
            writer.Flush();

            for (int i = 0; i < (int)Lumps._Count; i++)
            {
                writer.Write((int)0);
                writer.Write((int)0);
                writer.Flush();
            }

            var pos = writer.BaseStream.Position;

            for (int i = 0; i < (int)Lumps._Count; i++)
            {
                var stream = files[i];
                CopyStream(stream, writer.BaseStream, (int)stream.Length);
                writer.BaseStream.Flush();

                var nexPos = writer.BaseStream.Position;

                writer.BaseStream.Position = startPos + sizeof(uint) + 2 * i * sizeof(int);
                writer.Write((int)(pos - startPos));// offset from start of file
                writer.Write((int)(nexPos - pos));// length
                writer.Flush();

                pos = nexPos;
                writer.BaseStream.Position = pos;
            }

        }

        #endregion

        #endregion

        #endregion

        public static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                output.Flush();
                bytes -= read;
            }
        }

        #region Used Textures

        public static string[] GetUsedTextures(string path)
        {
            if (!File.Exists(path))
                return new string[0];

            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                return GetUsedTextures(reader);
            }
        }

        public static string[] GetUsedTextures(BinaryReader reader)
        {
            // Version
            {
                var version = reader.ReadUInt32();
                if (version != Version)
                    throw new BspLib.Bsp.Exceptions.BspVersionNotSupportedException(version);
            }

            reader.BaseStream.Position += 2 * sizeof(int) * (int)Lumps.Textures;
            var lump = new Lump(reader.ReadInt32(), reader.ReadInt32());


            // Textures
            using (var textures_stream = CreateStream(reader, lump))
            {
                //load only names
                var tuple = ReadTextureLump(textures_stream, loadPackedTextures: false);

                var usedTextures = tuple.Item2;
                //var textureDimensions = tuple.Item3;

                return usedTextures;
            }
        }

        #endregion

        #region Packed Textures

        public static Texture[] GetPackedTextures(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("BSP file not found", path);

            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                return GetPackedTextures(reader);
            }
        }

        public static Texture[] GetPackedTextures(BinaryReader reader)
        {
            // Version
            {
                var version = reader.ReadUInt32();
                if (version != Version)
                    throw new BspLib.Bsp.Exceptions.BspVersionNotSupportedException(version);
            }

            reader.BaseStream.Position += 2 * sizeof(int) * (int)Lumps.Textures;
            var lump = new Lump(reader.ReadInt32(), reader.ReadInt32());


            // Textures
            using (var textures_stream = CreateStream(reader, lump))
            {
                //load only names
                var tuple = ReadTextureLump(textures_stream);
                var packedTextures = tuple.Item1;
                //var usedTextures = tuple.Item2;
                //var textureDimensions = tuple.Item3;

                return packedTextures.ToArray();
            }
        }

        #endregion
    }
}