using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 3D Vector made of <see cref="System.Int32"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector3
    {
        public const int MemorySize = 3 * sizeof(int);

        private readonly int _x, _y, _z;

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
        /// Create new instance of 3D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        /// <param name="z">Z Coordinate.</param>
        public Vector3(int x, int y, int z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        /// <summary>
        /// Create new instance of 3D Vector
        /// </summary>
        /// <param name="xy">X and Y Coordinates.</param>
        /// <param name="z">Z Coordinate.</param>
        public Vector3(Vector2 xy, int z)
        {
            this._x = xy.X;
            this._y = xy.Y;
            this._z = z;
        }

        /// <summary>
        /// Create new instance of 3D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="yz">Y and Z Coordinates.</param>
        public Vector3(int x, Vector2 yz)
        {
            this._x = x;
            this._y = yz.X;
            this._z = yz.Y;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", X, Y, Z);
        }

        #region Compare

        public override int GetHashCode()
        {
            return X + Y + Z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3)
                return Vector3.Equals(this, (Vector3)obj);
            else
                return false;
        }

        public static bool Equals(Vector3 v1, Vector3 v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X && v1.Z == v2.Z;
        }

        #endregion

        #region Functions

        public static int Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector3 v)
        {
            int x = v.X - this.X;
            int y = v.Y - this.Y;
            int z = v.Z - this.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector3f v)
        {
            float x = v.X - this.X;
            float y = v.Y - this.Y;
            float z = v.Z - this.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector3d v)
        {
            double x = v.X - this.X;
            double y = v.Y - this.Y;
            double z = v.Z - this.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }


        /// <summary>
        /// Length of this vector
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        /// <summary>
        /// Length of this vector casted to <see cref="System.Single"/>
        /// </summary>
        public float LengthF
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        /// <summary>
        /// Get Normalized version of this vector.<br>
        /// Normalized vector has Length equal to 1.
        /// </summary>
        public Vector3d Normalized
        {
            get
            {
                double length = Length;
                return new Vector3d(X / length, Y / length, Z / length);
            }
        }

        /// <summary>
        /// Get Normalized version of this vector.<br>
        /// Normalized vector has Length equal to 1.
        /// </summary>
        public Vector3f NormalizedF
        {
            get
            {
                double length = Length;
                return new Vector3f((float)(X / length), (float)(Y / length), (float)(Z / length));
            }
        }

        #endregion

        #region Operators

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.X, -v.Y, -v.Z);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator *(Vector3 v, int m)
        {
            return new Vector3(v.X * m, v.Y * m, v.Z * m);
        }

        public static Vector3 operator /(Vector3 v, int d)
        {
            return new Vector3(v.X / d, v.Y / d, v.Z / d);
        }

        public static Vector3f operator *(Vector3 v, float m)
        {
            return new Vector3f(v.X * m, v.Y * m, v.Z * m);
        }

        public static Vector3f operator /(Vector3 v, float d)
        {
            return new Vector3f(v.X / d, v.Y / d, v.Z / d);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return Vector3.Equals(v1, v2);
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !Vector3.Equals(v1, v2);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0, 0]
        /// </summary>
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1, 1]
        /// </summary>
        public static readonly Vector3 One = new Vector3(1, 1, 1);

        /// <summary>
        /// X+ Vector.<br>
        /// [1, 0, 0]
        /// </summary>
        public static readonly Vector3 UnitX = new Vector3(1, 0, 0);

        /// <summary>
        /// Y+ Vector.<br>
        /// [0, 1, 0]
        /// </summary>
        public static readonly Vector3 UnitY = new Vector3(0, 1, 0);

        /// <summary>
        /// Z+ Vector.<br>
        /// [0, 0, 1]
        /// </summary>
        public static readonly Vector3 UnitZ = new Vector3(0, 0, 1);

        /// <summary>
        /// East Vector.<br>
        /// [1, 0, 0]
        /// </summary>
        public static readonly Vector3 East = new Vector3(1, 0, 0);

        /// <summary>
        /// West Vector.<br>
        /// [-1, 0, 0]
        /// </summary>
        public static readonly Vector3 West = new Vector3(-1, 0, 0);

        /// <summary>
        /// Up Vector.<br>
        /// [0, 1, 0]
        /// </summary>
        public static readonly Vector3 Up = new Vector3(0, 1, 0);

        /// <summary>
        /// Down Vector.<br>
        /// [0, -1, 0]
        /// </summary>
        public static readonly Vector3 Down = new Vector3(0, -1, 0);

        /// <summary>
        /// North Vector.<br>
        /// [0, 0, 1]
        /// </summary>
        public static readonly Vector3 North = new Vector3(0, 0, 1);

        /// <summary>
        /// South Vector.<br>
        /// [0, 0, -1]
        /// </summary>
        public static readonly Vector3 South = new Vector3(0, 0, -1);

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