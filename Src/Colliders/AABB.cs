using System;
using BspLib.Vector;

namespace BspLib.Colliders
{
    public class AABB : Collider
    {
        public AABB(Vector3f p0, Vector3f p1)
        {
            this.Min = new Vector3f(Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y), Math.Min(p0.Z, p1.Z));
            this.Max = new Vector3f(Math.Max(p0.X, p1.X), Math.Max(p0.Y, p1.Y), Math.Max(p0.Z, p1.Z));

            this.HalfSize = Max - Min;
            this.Center = Min + (HalfSize / 2);
        }

        public AABB(Vector3f center, float halfSizeX, float halfSizeY, float halfSizeZ)
        {
            this.Center = center;
            this.HalfSize = new Vector3f(halfSizeX, halfSizeY, halfSizeZ);

            this.Min = Center - HalfSize;
            this.Max = Center + HalfSize;
        }

        public readonly Vector3f Min, Max;

        public readonly Vector3f Center, HalfSize;

        public bool IsInside(Vector3f point)
        {
            bool aboveMin = Min.X <= point.X && Min.Y <= point.Y && Min.Z <= point.Z;
            bool belowMax = Max.X >= point.X && Max.Y >= point.Y && Max.Z >= point.Z;
            return aboveMin && belowMax;
        }

        public bool IsInCollision(AABB aabb)
        {
            bool collisionX = this.Max.X >= aabb.Min.X && aabb.Max.X >= this.Min.X;
            bool collisionY = this.Max.Y >= aabb.Min.Y && aabb.Max.Y >= this.Min.Y;
            bool collisionZ = this.Max.Z >= aabb.Min.Z && aabb.Max.Z >= this.Min.Z;
            return collisionX && collisionY && collisionZ;
        }

        public bool IsInCollision(Sphere sphere)
        {
            Vector3f closest = this.Center + Clamp(sphere.Center - this.Center, -HalfSize, HalfSize);
            return (closest - sphere.Center).Length < sphere.Radius;
        }

        private Vector3f Clamp(Vector3f val, Vector3f min, Vector3f max)
        {
            return new Vector3f(Clamp(val.X, min.X, max.X), Clamp(val.Y, min.Y, max.Y), Clamp(val.Z, min.Z, max.Z));
        }

        private float Clamp(float val, float min, float max)
        {
            return Math.Max(min, Math.Min(max, val));
        }
    }
}