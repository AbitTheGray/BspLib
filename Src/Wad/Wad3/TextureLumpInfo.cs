using System;
using System.Drawing;
using System.IO;

namespace BspLib.Wad.Wad3
{
    public class TextureLumpInfo
    {
        public TextureLumpInfo(char[] name, uint width, uint height, uint[] mipmapOffsets)
        {
            if (name == null || name.Length != NameLength)
                throw new ArgumentOutOfRangeException(nameof(name));
            if (mipmapOffsets == null || mipmapOffsets.Length != MipmapOffsetsLength)
                throw new ArgumentOutOfRangeException(nameof(mipmapOffsets));

            this.Name = name;

            this.Width = width;
            this.Height = height;
            this.MipmapOffsets = mipmapOffsets;
        }

        public TextureLumpInfo(TextureByteIndexPaletteWithMipmaps texture)
        {
            if (texture.Name.Length > NameLength - 1)
                throw new ArgumentOutOfRangeException(nameof(texture));

            char[] name = new char[NameLength];
            for (int i = 0; i < texture.Name.Length; i++)
                name[i] = texture.Name[i];

            this.Width = (uint)texture.Width;
            this.Height = (uint)texture.Height;

            this.MipmapOffsets = new uint[MipmapOffsetsLength];
        }

        /// <summary>
        /// Length: NameLength (16)
        /// </summary>
        public char[] Name;
        public string Name_s
        {
            get
            {
                return new string(Name, 0, Array.IndexOf(Name, '\0'));
            }
        }

        public uint Width;
        public uint Height;
        /// <summary>
        /// Length: MipmapOffsetsLength (4)
        /// </summary>
        public uint[] MipmapOffsets;

        public const int NameLength = 16;
        public const int MipmapOffsetsLength = 4;

        /// <summary>
        /// Load TextureLumpInfo from stream.
        /// Starts reading from current position.
        /// </summary>
        /// <param name="reader">Source stream.</param>
        public static TextureLumpInfo Load(BinaryReader reader)
        {
            char[] name = new char[NameLength];
            for (int i = 0; i < NameLength; i++)
                name[i] = (char)reader.ReadByte();
            //  name[i] = System.Text.Encoding.ASCII.GetChars(new byte[] { reader.ReadByte() })[0];

            //char[] name = System.Text.Encoding.ASCII.GetChars(reader.ReadBytes(NameLength));

            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();

            uint[] mipmapOffsets = new uint[MipmapOffsetsLength];
            for (int i = 0; i < MipmapOffsetsLength; i++)
                mipmapOffsets[i] = reader.ReadUInt32();

            return new TextureLumpInfo(name, width, height, mipmapOffsets);
        }

        /// <summary>
        /// Saves settings and mipmap data into.
        /// </summary>
        /// <returns>The save.</returns>
        /// <param name="writer">Writer.</param>
        public void Save(BinaryWriter writer)
        {
            // Null-terminated texture name
            for (int i = 0; i < NameLength - 1; i++)
                writer.Write((byte)Name[i]);
            writer.Write((byte)0);// safety null terminator
            writer.Flush();

            // Texture dimensions
            writer.Write(Width);
            writer.Write(Height);
            writer.Flush();

            // Mipmap offsets (generated without spaces)
            // mipmap[0] is texture.bitmap
            // mipmap[1] is first (real) mipmap
            uint[] mipmapOffsets = new uint[MipmapOffsetsLength];
            mipmapOffsets[0] = NameLength * sizeof(byte) + 2 * sizeof(uint);
            for (int i = 1; i < MipmapOffsetsLength; i++)
            {
                var pow = (int)Math.Pow(2, i);
                mipmapOffsets[i] = (uint)(mipmapOffsets[i - 1] + (Width / pow) * (Height / pow));
                writer.Write(mipmapOffsets[i]);
            }
            writer.Flush();
        }

        /// <summary>
        /// Load texture (with mipmaps) of this TextureLumpInfo.
        /// </summary>
        /// <param name="reader">Source stream.</param>
        /// <param name="texture_offset">Offset of this TextureLumpInfo in source stream.</param>
        public TextureByteIndexPaletteWithMipmaps LoadTexture(BinaryReader reader, long texture_offset = 0)
        {
            reader.BaseStream.Position = texture_offset + this.MipmapOffsets[0];

            // Indices to main texture (mipmap[0])
            byte[] indices = reader.ReadBytes((int)(this.Width * this.Height));

            // Loading color palette (after last mipmap)
            int last_mipmap_index = this.MipmapOffsets.Length - 1;
            reader.BaseStream.Position = texture_offset + this.MipmapOffsets[last_mipmap_index] + ((this.Width / (int)Math.Pow(2, last_mipmap_index)) * (this.Height / (int)Math.Pow(2, last_mipmap_index))) + 2;
            Color[] palette = new Color[256];
            for (int p = 0; p < palette.Length; p++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();

                palette[p] = Color.FromArgb(r, g, b);
            }
            // Last index is transparent
            if (palette[255].R == 0 && palette[255].G == 0 && palette[255].B == 255)
                palette[255] = Color.Transparent;

            // Texture instance (with main texture and color palette)
            var texture = new TextureByteIndexPaletteWithMipmaps(this.Name_s, (int)this.Width, (int)this.Height, indices, palette);

            // Adding mipmaps to texture instance
            // mipmap[1] is first mipmap (0 is main texture)
            // Each mipmap has half dimensions then previous one
            for (int mipmap_level = 1; mipmap_level < this.MipmapOffsets.Length; mipmap_level++)
            {
                reader.BaseStream.Position = (long)texture_offset + this.MipmapOffsets[mipmap_level];

                int width = (int)(this.Width / Math.Pow(2, mipmap_level));
                int height = (int)(this.Height / Math.Pow(2, mipmap_level));

                texture.AddMipmap(mipmap_level, reader.ReadBytes(width * height));
            }

            return texture;
        }

        /// <summary>
        /// Save texture data.
        /// This should be run after saving the TextureLumpInfo
        /// </summary>
        /// <param name="writer">Output stream.</param>
        /// <param name="texture">Texture to save. Only transparent color can be palette[255] and must be R=0,G=0,B=255.</param>
        public static void SaveTexture(BinaryWriter writer, TextureByteIndexPaletteWithMipmaps texture)
        {
            // All MipMaps (including main texture)
            for (int m = 0; m < TextureLumpInfo.MipmapOffsetsLength; m++)
            {
                writer.Write(texture.GetMipmap(m));
                writer.Flush();
            }

            writer.Write((ushort)0);//2 dummy bytes
            writer.Flush();

            // Color palette
            {
                // Used colors in palette
                for (int p = 0; p < texture.Palette.Length; p++)
                {
                    var c = texture.Palette[p];
                    writer.Write(c.R);//R
                    writer.Write(c.G);//G
                    writer.Write(c.B);//B
                }

                // Unused colors in palette
                for (int p = texture.Palette.Length; p < 255; p++)
                {
                    writer.Write(0);//R
                    writer.Write(0);//G
                    writer.Write(0);//B
                }

                // Transparent blue if unused
                if (texture.Palette.Length < 255)
                {
                    // Index: 255
                    writer.Write(0);//R
                    writer.Write(0);//G
                    writer.Write(255);//B
                }
            }
            writer.Flush();
        }
    }
}
