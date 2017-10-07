using System;
namespace BspLib.OpenGL
{
    public class OpenGLIndicesBufferWithTextureId
    {
        public OpenGLIndicesBufferWithTextureId(int textureId, uint[] indices)
        {
            this.TextureId = textureId;

            this.Indices = new uint[indices.Length];
            System.Array.Copy(indices, Indices, indices.Length);
        }

        public readonly int TextureId;

        public readonly uint[] Indices;
    }
}