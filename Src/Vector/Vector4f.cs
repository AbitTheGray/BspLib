using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 4D Vector made of <see cref="System.Single"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector4f
    {
        public const int MemorySize = 4 * sizeof(int);

        private readonly float _x, _y, _z, _w;

        /// <summary>
        /// X Coordinate
        /// </summary>
        public float X
        {
            get
            {
                return _x;
            }
        }

        /// <summary>
        /// Y Coordinate
        /// </summary>
        public float Y
        {
            get
            {
                return _y;
            }
        }

        /// <summary>
        /// Z Coordinate
        /// </summary>
        public float Z
        {
            get
            {
                return _z;
            }
        }

        /// <summary>
        /// W (4th) Coordinate
        /// </summary>
        public float W
        {
            get
            {
                return _w;
            }
        }

        /// <summary>
        /// Create new instance of 4D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        /// <param name="z">Z Coordinate.</param>
        /// <param name="w">W Coordinate.</param>
        public Vector4f(float x, float y, float z, float w)
        {
            this._x = x;
            this._y = y;
            this._z = z;
            this._w = w;
        }

        /// <summary>
        /// Create new instance of 4D Vector
        /// </summary>
        /// <param name="xyz">X, Y and Z Coordinates.</param>
        /// <param name="w">W Coordinate.</param>
        public Vector4f(Vector3f xyz, float w)
        {
            this._x = xyz.X;
            this._y = xyz.Y;
            this._z = xyz.Z;
            this._w = w;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}]", X, Y, Z, W);
        }

        #region Compare

        public override int GetHashCode()
        {
            return (int)X + (int)Y + (int)Z + (int)W;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4f)
                return Vector4f.Equals(this, (Vector4f)obj);
            else if (obj is Vector4)
                return Vector4f.Equals(this, (Vector4)obj);
            else
                return false;
        }

        public static bool Equals(Vector4f v1, Vector4f v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X && v1.Z == v2.Z && v1.W == v2.W;
        }

        #endregion

        #region Functions

        public static float Dot(Vector4f v1, Vector4f v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector4 v)
        {
            float x = v.X - this.X;
            float y = v.Y - this.Y;
            float z = v.Z - this.Z;
            float w = v.W - this.W;
            return Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector4f v)
        {
            float x = v.X - this.X;
            float y = v.Y - this.Y;
            float z = v.Z - this.Z;
            float w = v.W - this.W;
            return Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector4d v)
        {
            double x = v.X - this.X;
            double y = v.Y - this.Y;
            double z = v.Z - this.Z;
            double w = v.W - this.W;
            return Math.Sqrt(x * x + y * y + z * z + w * w);
        }


        /// <summary>
        /// Length of this vector
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
            }
        }

        /// <summary>
        /// Length of this vector casted to <see cref="System.Single"/>
        /// </summary>
        public float LengthF
        {
            get
            {
                return (float)Length;
            }
        }

        /// <summary>
        /// Get Normalized version of this vector.<br>
        /// Normalized vector has Length equal to 1.
        /// </summary>
        public Vector4d Normalized
        {
            get
            {
                double length = Length;
                return new Vector4d(X / length, Y / length, Z / length, W / length);
            }
        }

        /// <summary>
        /// Get Normalized version of this vector.<br>
        /// Normalized vector has Length equal to 1.
        /// </summary>
        public Vector4f NormalizedF
        {
            get
            {
                double length = Length;
                return new Vector4f((float)(X / length), (float)(Y / length), (float)(Z / length), (float)(W / length));
            }
        }

        #endregion

        #region Operators

        public static Vector4f operator -(Vector4f v)
        {
            return new Vector4f(-v.X, -v.Y, -v.Z, -v.W);
        }

        public static Vector4f operator +(Vector4f v1, Vector4f v2)
        {
            return new Vector4f(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
        }

        public static Vector4f operator -(Vector4f v1, Vector4f v2)
        {
            return new Vector4f(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
        }

        public static Vector4f operator *(Vector4f v, float f)
        {
            return new Vector4f(v.X * f, v.Y * f, v.Z * f, v.W * f);
        }

        public static Vector4f operator /(Vector4f v, float f)
        {
            return new Vector4f(v.X / f, v.Y / f, v.Z / f, v.W / f);
        }

        public static bool operator ==(Vector4f v1, Vector4f v2)
        {
            return Vector4f.Equals(v1, v2);
        }

        public static bool operator !=(Vector4f v1, Vector4f v2)
        {
            return !Vector4f.Equals(v1, v2);
        }

        #endregion

        #region Vector4f = Vector4

        public static implicit operator Vector4f(Vector4 v)
        {
            return new Vector4f(v.X, v.Y, v.Z, v.W);
        }

        public static explicit operator Vector4(Vector4f v)
        {
            return new Vector4((int)v.X, (int)v.Y, (int)v.Z, (int)v.W);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0, 0, 0]
        /// </summary>
        public static readonly Vector4f Zero = new Vector4f(0, 0, 0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1, 1, 1]
        /// </summary>
        public static readonly Vector4f One = new Vector4f(1, 1, 1, 1);

        /// <summary>
        /// Half Vector.<br>
        /// [0.5, 0.5, 0.5, 0.5]
        /// </summary>
        public static readonly Vector4f Half = new Vector4f(0.5f, 0.5f, 0.5f, 0.5f);

        /// <summary>
        /// X+ Vector.<br>
        /// [1, 0, 0, 0]
        /// </summary>
        public static readonly Vector4f UnitX = new Vector4f(1, 0, 0, 0);

        /// <summary>
        /// Y+ Vector.<br>
        /// [0, 1, 0, 0]
        /// </summary>
        public static readonly Vector4f UnitY = new Vector4f(0, 1, 0, 0);

        /// <summary>
        /// Z+ Vector.<br>
        /// [0, 0, 1, 0]
        /// </summary>
        public static readonly Vector4f UnitZ = new Vector4f(0, 0, 1, 0);

        /// <summary>
        /// W+ Vector.<br>
        /// [0, 0, 0, 1]
        /// </summary>
        public static readonly Vector4f UnitW = new Vector4f(0, 0, 0, 1);

        #endregion
    }
}