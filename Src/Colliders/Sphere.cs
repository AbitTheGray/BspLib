using System;
using BspLib.Vector;

namespace BspLib.Colliders
{
    public class Sphere : Collider
    {
        public Sphere(Vector3f center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public Vector3f Center;
        public float Radius;

        public bool IsInside(Vector3f point)
        {
            return Center.DistanceTo(point) <= Radius;
        }

        public bool IsInCollision(Sphere sphere)
        {
            return this.Center.DistanceTo(sphere.Center) <= this.Radius + sphere.Radius;
        }
    }
}