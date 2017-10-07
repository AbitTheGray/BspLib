using System;
using System.Collections.Generic;
using System.Drawing;

namespace BspLib.Wad
{
    public class TextureByteIndexPaletteWithMipmaps : TextureByteIndexPalette
    {
        public TextureByteIndexPaletteWithMipmaps(string name, int width, int height, byte[] indices, Color[] palette) : base(name, width, height, indices, palette)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(string name, byte[,] indices, Color[] palette) : base(name, indices, palette)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(Texture texture) : base(texture)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(Texture texture, Color[] palette) : base(texture, palette)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(string name, Bitmap image) : base(name, image)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(string name, Bitmap image, Color[] palette) : base(name, image, palette)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(string name, int width, int height, Color[] image) : base(name, width, height, image)
        {
        }

        public TextureByteIndexPaletteWithMipmaps(string name, Color[,] image) : base(name, image)
        {
        }

        private List<byte[]> _mipmaps = new List<byte[]>();

        public byte[] GetMipmap(int level)
        {
            if (level < 0)
                throw new ArgumentOutOfRangeException(nameof(level));
            if (level == 0)
                return Indices;
            else
                return _mipmaps[level - 1];
        }

        public void AddMipmap(int level, byte[] indices)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException(nameof(level));
            if (GetWidth(level) * GetHeight(level) != indices.Length)
                throw new ArgumentOutOfRangeException(nameof(indices));

            level--;
            while (_mipmaps.Count <= level)
                _mipmaps.Add(null);
            _mipmaps[level] = indices;
        }
        public void AddMipmap(int level, byte[,] indices)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException(nameof(level));

            if (GetWidth(level) != indices.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(indices));
            if (GetHeight(level) != indices.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(indices));

            byte[] indices_arr = new byte[GetWidth(level) * GetHeight(level)];
            System.Buffer.BlockCopy(indices, 0, indices_arr, 0, indices_arr.Length);

            level--;
            while (_mipmaps.Count < level)
                _mipmaps.Add(null);
            _mipmaps[level] = indices_arr;
        }

        public int GetWidth(int level)
        {
            if (level == 0)
                return Bitmap.Width;
            else
                return base.Bitmap.Width / (int)Math.Pow(2, level);
        }

        public int GetHeight(int level)
        {
            if (level == 0)
                return Bitmap.Height;
            else
                return base.Bitmap.Height / (int)Math.Pow(2, level);
        }
    }
}