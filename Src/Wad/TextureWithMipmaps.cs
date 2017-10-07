using System;
using System.Collections.Generic;
using System.Drawing;

namespace BspLib.Wad
{
    public class TextureWithMipmaps : Texture
    {
        public TextureWithMipmaps(string name) : base(name)
        {
        }
        public TextureWithMipmaps(string name, Bitmap bitmap) : base(name, bitmap)
        {
        }

        private List<Bitmap> _mipmaps = new List<Bitmap>();

        public Bitmap GetMipmap(int level)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException(nameof(level));

            return _mipmaps[level - 1];
        }

        public void AddMipmap(int level, Bitmap bitmap)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException(nameof(level));

            if (GetWidth(level) != bitmap.Width)
                throw new ArgumentOutOfRangeException(nameof(bitmap));
            if (GetHeight(level) != bitmap.Height)
                throw new ArgumentOutOfRangeException(nameof(bitmap));

            level--;
            while (_mipmaps.Count < level)
                _mipmaps.Add(null);
            _mipmaps[level] = bitmap;
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