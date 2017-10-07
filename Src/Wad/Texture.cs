using System;
using System.Drawing;

namespace BspLib.Wad
{
    public class Texture
    {
        public Texture(string name, Bitmap bitmap) : this(name)
        {
            if (bitmap == null || bitmap.Width <= 0 || bitmap.Height <= 0)
                throw new ArgumentNullException(nameof(bitmap));

            this.Bitmap = bitmap;
        }
        public Texture(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            this.Name = name;
            this.Bitmap = null;
        }

        public string Name
        {
            get;
        }

        public Bitmap Bitmap
        {
            get;
            protected set;
        }
    }
}