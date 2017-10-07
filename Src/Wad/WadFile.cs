using System;
using System.Collections.Generic;
using System.IO;
using BspLib.Wad.Exceptions;

namespace BspLib.Wad
{
    public class WadFile
    {
        public WadFile()
        {
        }
        public WadFile(params Texture[] textures)
        {
            this.Textures.AddRange(textures);
        }

        public List<Texture> Textures
        {
            get;
        } = new List<Texture>();

        public enum WadVersion : uint
        {
            Unknown = 0,
            Wad2 = Wad.Wad2.Wad.Version,
            Wad3 = Wad.Wad3.Wad.Version,
        }

        #region Loading

        public void Load(string path) => Load(this, path);

        public static void Load(WadFile wad, string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Wad file was not found.");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(wad, stream);
            }
        }

        public void Load(Stream stream) => Load(this, stream);

        public static void Load(WadFile wad, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = reader.BaseStream.Position;

            //4 bytes = 1 int
            uint version = reader.ReadUInt32();

            reader.BaseStream.Position = startPos;


            switch (version)
            {
                // Wad2 (Quake I)
                case (uint)WadVersion.Wad2:
                    {
                        Wad2.Wad.Load(wad, stream);
                    }
                    break;
                // Wad3 (GoldSource)
                case (uint)WadVersion.Wad3:
                    {
                        Wad3.Wad.Load(wad, stream);
                    }
                    break;
                default:
                    //throw new WadVersionNotSupportedException(version);
                    break;
            }
        }

        #endregion

        #region Saving

        public bool Save(string path, WadVersion version) => Save(this, path, version);

        public static bool Save(WadFile wad, string path, WadVersion version)
        {
            if (File.Exists(path))
                return false;

            using (var stream = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                Save(wad, stream, version);
                return true;
            }
        }

        public void Save(Stream stream, WadVersion version) => Save(this, stream, version);

        public static void Save(WadFile wad, Stream stream, WadVersion version)
        {
            switch (version)
            {
                // Wad2 (Quake I)
                case WadVersion.Wad2:
                    {
                        Wad2.Wad.Save(wad, stream);
                        return;
                    }
                // Wad3 (GoldSource)
                case WadVersion.Wad3:
                    {
                        Wad3.Wad.Save(wad, stream);
                        return;
                    }
            }
            throw new WadVersionNotSupportedException((uint)version);
        }

        #endregion

        #region Parsing texture list

        public static string[] GetTextureList(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var reader = new BinaryReader(stream);


            var startPos = reader.BaseStream.Position;

            //4 bytes = 1 int
            uint version = reader.ReadUInt32();

            reader.BaseStream.Position = startPos;


            switch (version)
            {
                // Wad2 (Quake I)
                case (uint)WadVersion.Wad2:
                    {
                        return Wad2.Wad.GetTextureList(stream);
                    }
                // Wad3 (GoldSource)
                case (uint)WadVersion.Wad3:
                    {
                        return Wad3.Wad.GetTextureList(stream);
                    }
            }
            throw new WadVersionNotSupportedException(version);
        }

        #endregion

    }
}