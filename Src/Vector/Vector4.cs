using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 4D Vector made of <see cref="System.Int32"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector4
    {
        public const int MemorySize = 4 * sizeof(int);

        private readonly int _x, _y, _z, _w;

        /// <summary>
        /// X Coordinate
        /// </summary>
        public int X
        {
            get
            {
                return _x;
            }
        }

        /// <summary>
        /// Y Coordinate
        /// </summary>
        public int Y
        {
            get
            {
                return _y;
            }
        }

        /// <summary>
        /// Z Coordinate
        /// </summary>
        public int Z
        {
            get
            {
                return _z;
            }
        }

        /// <summary>
        /// W Coordinate
        /// </summary>
        public int W
        {
            get
            {
                return _w;
            }
        }

        /// <summary>
        /// Create new instance of 3D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        /// <param name="z">Z Coordinate.</param>
        /// <param name="w">W Coordinate.</param>
        public Vector4(int x, int y, int z, int w)
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
        public Vector4(Vector3 xyz, int w)
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
            return X + Y + Z + W;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4)
                return Vector4.Equals(this, (Vector4)obj);
            else
                return false;
        }

        public static bool Equals(Vector4 v1, Vector4 v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X && v1.Z == v2.Z && v1.W == v2.W;
        }

        #endregion

        #region Functions

        public static int Dot(Vector4 v1, Vector4 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector4 v)
        {
            int x = v.X - this.X;
            int y = v.Y - this.Y;
            int z = v.Z - this.Z;
            int w = v.W - this.W;
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
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
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

        public static Vector4 operator -(Vector4 v)
        {
            return new Vector4(-v.X, -v.Y, -v.Z, -v.W);
        }

        public static Vector4 operator +(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
        }

        public static Vector4 operator -(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
        }

        public static Vector4 operator *(Vector4 v, int m)
        {
            return new Vector4(v.X * m, v.Y * m, v.Z * m, v.W * m);
        }

        public static Vector4 operator /(Vector4 v, int d)
        {
            return new Vector4(v.X / d, v.Y / d, v.Z / d, v.W / d);
        }

        public static Vector4f operator *(Vector4 v, float m)
        {
            return new Vector4f(v.X * m, v.Y * m, v.Z * m, v.W * m);
        }

        public static Vector4f operator /(Vector4 v, float d)
        {
            return new Vector4f(v.X / d, v.Y / d, v.Z / d, v.W / d);
        }

        public static bool operator ==(Vector4 v1, Vector4 v2)
        {
            return Vector4.Equals(v1, v2);
        }

        public static bool operator !=(Vector4 v1, Vector4 v2)
        {
            return !Vector4.Equals(v1, v2);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0, 0, 0]
        /// </summary>
        public static readonly Vector4 Zero = new Vector4(0, 0, 0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1, 1, 1]
        /// </summary>
        public static readonly Vector4 One = new Vector4(1, 1, 1, 1);

        /// <summary>
        /// X+ Vector.<br>
        /// [1, 0, 0, 0]
        /// </summary>
        public static readonly Vector4 UnitX = new Vector4(1, 0, 0, 0);

        /// <summary>
        /// Y+ Vector.<br>
        /// [0, 1, 0, 0]
        /// </summary>
        public static readonly Vector4 UnitY = new Vector4(0, 1, 0, 0);

        /// <summary>
        /// Z+ Vector.<br>
        /// [0, 0, 1, 0]
        /// </summary>
        public static readonly Vector4 UnitZ = new Vector4(0, 0, 1, 0);

        /// <summary>
        /// W+ Vector.<br>
        /// [0, 0, 0, 1]
        /// </summary>
        public static readonly Vector4 UnitW = new Vector4(0, 0, 0, 1);

        #endregion

        #region 2D

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2 XY
        {
            get
            {
                return new Vector2(X, Y);
            }
        }

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2 XZ
        {
            get
            {
                return new Vector2(X, Z);
            }
        }

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2 YZ
        {
            get
            {
                return new Vector2(Y, Z);
            }
        }

        #endregion

        #region Reordered

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3 XYZ
        {
            get
            {
                return new Vector3(X, Y, Z);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3 XZY
        {
            get
            {
                return new Vector3(X, Z, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3 YZX
        {
            get
            {
                return new Vector3(Y, Z, X);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3 YXZ
        {
            get
            {
                return new Vector3(Y, X, Z);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3 ZXY
        {
            get
            {
                return new Vector3(Z, X, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3 ZYX
        {
            get
            {
                return new Vector3(Z, Y, X);
            }
        }

        #endregion

        #region Eulers

        /// <summary>
        /// Forward direcrtion vector (from euler angles).<br>
        /// Vector length is 100.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                double n_cos_x = -Math.Cos(X);
                return new Vector3((int)(n_cos_x * Math.Sin(Y) * 100), (int)(Math.Sin(X) * 100), (int)(n_cos_x * Math.Cos(Y) * 100));
            }
        }

        #endregion

    }
}