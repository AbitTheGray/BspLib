using System;
using System.Collections.Generic;
using BspLib.Vector;

namespace BspLib.Colliders
{
    public class Collider
    {
        public Collider()
        {
        }

        public static bool InCollision(Collider col0, Collider col1)
        {
            if (col0 is Sphere && col1 is Sphere)
            {
                return ((Sphere)col0).IsInCollision((Sphere)col1);
            }

            if (col0 is AABB && col1 is AABB)
            {
                return ((AABB)col0).IsInCollision((AABB)col1);
            }

            if (col0 is AABB && col1 is Sphere)
            {
                return ((AABB)col0).IsInCollision((Sphere)col1);
            }

            if (col0 is Sphere && col1 is AABB)
            {
                return ((AABB)col1).IsInCollision((Sphere)col0);
            }

            throw new NotImplementedException();
            throw new NotSupportedException();
        }

        public static float RaycastNearest(Vector3f position, Vector3f direction, Collider col)
        {
            throw new NotImplementedException();
            // http://www.miguelcasillas.com/?p=74

        }


        public static Collider RaycastNearest(Vector3f position, Vector3f direction, IEnumerable<Collider> colliders)
        {
            Collider nearest_collider = null;
            float nearest_distance = float.MaxValue;

            foreach (var col in colliders)
            {
                float dist = RaycastNearest(position, direction, col);
                if (nearest_distance < dist)
                {
                    dist = nearest_distance;
                    nearest_collider = col;
                }
            }

            return nearest_collider;
        }
    }
}