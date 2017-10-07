using System;
using System.Collections.Generic;
using BspLib.Vector;

namespace BspLib.OpenGL
{
    /// <summary>
    /// { pos.x, pos.y, pos.z, uv.x, uv.y }, {...}, ...
    /// </summary>
    public class OpenGLArrayBuffer
    {
        public OpenGLArrayBuffer(Vector3f[] positions, Vector2f[] uvs)
        {
            if (positions.Length != uvs.Length)
                throw new ArgumentOutOfRangeException(nameof(positions), nameof(uvs));

            this.Count = (ushort)positions.Length;
            var buff = new List<byte>(Count * ItemLength);
            for (int i = 0; i < Count; i++)
            {
                int index = i * ItemLength;

                var pos = positions[i];
                buff.AddRange(BitConverter.GetBytes((float)pos.X));
                buff.AddRange(BitConverter.GetBytes((float)pos.Y));
                buff.AddRange(BitConverter.GetBytes((float)pos.Z));

                var uv = uvs[i];
                buff.AddRange(BitConverter.GetBytes((float)uv.X));
                buff.AddRange(BitConverter.GetBytes((float)uv.Y));
            }
            this.Buffer = buff.ToArray();
        }

        public readonly ushort ItemLength = sizeof(float) * (3 + 2);
        public readonly ushort Count;

        public readonly byte[] Buffer;
    }
}