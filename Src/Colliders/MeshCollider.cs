using System;
using System.Linq;
using BspLib.Vector;

namespace BspLib.Colliders
{
    public class MeshCollider : Collider
    {
        public MeshCollider(Vector3f[] vertices) : this(vertices, null)
        {
        }
        public MeshCollider(Vector3f[] vertices, uint[] indices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices), "Vertices cannot be null");

            if (vertices.LongLength > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(vertices), "Too many vertices");

            this.Vertices = vertices;

            if (indices == null)
            {
                if (vertices.Length % 3 != 0)
                    throw new ArgumentOutOfRangeException(nameof(vertices), "Invalid number of vertices - cannot create triangles");

                this.Indices = new uint[vertices.LongLength];
                for (uint i = 0; i < this.Indices.LongLength; i++)
                    this.Indices[i] = i;
            }
            else
            {
                if (indices.Length % 3 != 0)
                    throw new ArgumentOutOfRangeException(nameof(indices), "Invalid number of indices - cannot create triangles");

                this.Indices = new uint[vertices.LongLength];
                for (short i = 0; i < indices.Length; i++)
                {
                    uint index = indices[i];
                    if (index > vertices.LongLength)
                        throw new ArgumentOutOfRangeException(nameof(indices), "Vertex index is invalid");
                    /*
					if (index > uint.MaxValue)
						throw new ArgumentOutOfRangeException(nameof(indices), "Vertex index is too big");
					*/
                    this.Indices[i] = index;
                }
            }
        }

        public Vector3f[] Vertices
        {
            get;
        }

        public uint[] Indices
        {
            get;
        }
    }
}
