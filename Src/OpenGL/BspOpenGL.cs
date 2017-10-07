using System;
using BspLib.Bsp;

namespace BspLib.OpenGL
{
    public static class BspOpenGL
    {
        public static OpenGLArrayBuffer CreateArrayBuffer(BspFile.Model model)
        {
            return new OpenGLArrayBuffer(model.Positions, model.TextureCoordinates);
        }
        public static OpenGLArrayBuffer CreateArrayBuffer(this BspFile bsp, int modelID)
        {
            var model = bsp.Models[modelID];
            return CreateArrayBuffer(model);
        }

        public static OpenGLArrayBuffer[] CreateArrayBuffers(this BspFile bsp)
        {
            var buffers = new OpenGLArrayBuffer[bsp.Models.Count];
            for (int i = 0; i < buffers.Length; i++)
                buffers[i] = CreateArrayBuffer(bsp, i);
            return buffers;
        }

        public static OpenGLIndicesBufferWithTextureName[] CreateIndicesBuffer(this BspFile bsp, int modelID)
        {
            var model = bsp.Models[modelID];
            var buffers = new OpenGLIndicesBufferWithTextureName[model.Triangles.Count];
            int index = 0;
            foreach (var kvp in model.Triangles)
            {
                var buff = new OpenGLIndicesBufferWithTextureName(kvp.Key, kvp.Value);
                buffers[index++] = buff;
            }
            return buffers;
        }
    }
}