namespace BspLib.OpenGL
{
    public class OpenGLIndicesBufferWithTextureName
    {
        public OpenGLIndicesBufferWithTextureName(string textureName, uint[] indices)
        {
            this.TextureName = (string)textureName.Clone();

            this.Indices = new uint[indices.Length];
            System.Array.Copy(indices, Indices, indices.Length);
        }

        public readonly string TextureName;

        public readonly uint[] Indices;

        public OpenGLIndicesBufferWithTextureId ToIndicesWithTextureId(System.Collections.Generic.Dictionary<string, int> textureDictionary)
        {
            int textureId;
            if (textureDictionary.TryGetValue(TextureName, out textureId))
                return ToIndicesWithTextureId(textureId);
            else
                return null;
        }

        public OpenGLIndicesBufferWithTextureId ToIndicesWithTextureId(int textureId)
        {
            return new OpenGLIndicesBufferWithTextureId(textureId, Indices);
        }
    }
}