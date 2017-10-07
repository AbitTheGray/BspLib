using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 3D Vector made of <see cref="System.Double"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector3d
    {
        public const int MemorySize = 3 * sizeof(double);

        private readonly double _x, _y, _z;

        /// <summary>
        /// X Coordinate
        /// </summary>
        public double X
        {
            get
            {
                return _x;
            }
        }

        /// <summary>
        /// Y Coordinate
        /// </summary>
        public double Y
        {
            get
            {
                return _y;
            }
        }

        /// <summary>
        /// Z Coordinate
        /// </summary>
        public double Z
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
        public Vector3d(double x, double y, double z)
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
        public Vector3d(Vector2d xy, double z)
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
        public Vector3d(double x, Vector2d yz)
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
            return (int)X + (int)Y + (int)Z;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3d)
                return Vector3d.Equals(this, (Vector3d)obj);
            else if (obj is Vector3f)
                return Vector3d.Equals(this, (Vector3f)obj);
            else if (obj is Vector3)
                return Vector3d.Equals(this, (Vector3)obj);
            else
                return false;
        }

        public static bool Equals(Vector3d v1, Vector3d v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X && v1.Z == v2.Z;
        }

        #endregion

        #region Functions

        public static double Dot(Vector3d v1, Vector3d v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector3 v)
        {
            double x = v.X - this.X;
            double y = v.Y - this.Y;
            double z = v.Z - this.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector3f v)
        {
            double x = v.X - this.X;
            double y = v.Y - this.Y;
            double z = v.Z - this.Z;
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

        public static Vector3d operator -(Vector3d v)
        {
            return new Vector3d(-v.X, -v.Y, -v.Z);
        }

        public static Vector3d operator +(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3d operator -(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3d operator *(Vector3d v, double m)
        {
            return new Vector3d(v.X * m, v.Y * m, v.Z * m);
        }

        public static Vector3d operator /(Vector3d v, double d)
        {
            return new Vector3d(v.X / d, v.Y / d, v.Z / d);
        }

        public static bool operator ==(Vector3d v1, Vector3d v2)
        {
            return Vector3d.Equals(v1, v2);
        }

        public static bool operator !=(Vector3d v1, Vector3d v2)
        {
            return !Vector3d.Equals(v1, v2);
        }

        #endregion

        #region Vector3d = Vector3

        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static explicit operator Vector3(Vector3d v)
        {
            return new Vector3((int)v.X, (int)v.Y, (int)v.Z);
        }

        #endregion

        #region Vector3d = Vector3f

        public static implicit operator Vector3d(Vector3f v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static explicit operator Vector3f(Vector3d v)
        {
            return new Vector3f((float)v.X, (float)v.Y, (float)v.Z);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0, 0]
        /// </summary>
        public static readonly Vector3d Zero = new Vector3d(0, 0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1, 1]
        /// </summary>
        public static readonly Vector3d One = new Vector3d(1, 1, 1);

        /// <summary>
        /// Half Vector.<br>
        /// [0.5, 0.5, 0.5]
        /// </summary>
        public static readonly Vector3d Half = new Vector3d(0.5, 0.5, 0.5);

        /// <summary>
        /// X+ Vector.<br>
        /// [1, 0, 0]
        /// </summary>
        public static readonly Vector3d UnitX = new Vector3d(1, 0, 0);

        /// <summary>
        /// Y+ Vector.<br>
        /// [0, 1, 0]
        /// </summary>
        public static readonly Vector3d UnitY = new Vector3d(0, 1, 0);

        /// <summary>
        /// Z+ Vector.<br>
        /// [0, 0, 1]
        /// </summary>
        public static readonly Vector3d UnitZ = new Vector3d(0, 0, 1);

        #endregion

        #region 2D

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2d XY
        {
            get
            {
                return new Vector2d(X, Y);
            }
        }

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2d XZ
        {
            get
            {
                return new Vector2d(X, Z);
            }
        }

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2d YZ
        {
            get
            {
                return new Vector2d(Y, Z);
            }
        }

        #endregion

        #region Reordered

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3d XYZ
        {
            get
            {
                return new Vector3d(X, Y, Z);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3d XZY
        {
            get
            {
                return new Vector3d(X, Z, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3d YZX
        {
            get
            {
                return new Vector3d(Y, Z, X);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3d YXZ
        {
            get
            {
                return new Vector3d(Y, X, Z);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3d ZXY
        {
            get
            {
                return new Vector3d(Z, X, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3d ZYX
        {
            get
            {
                return new Vector3d(Z, Y, X);
            }
        }

        #endregion

        #region Eulers

        /// <summary>
        /// Forward direcrtion vector (from euler angles).
        /// </summary>
        public Vector3d Forward
        {
            get
            {
                double n_cos_x = -Math.Cos(X);
                return new Vector3d(n_cos_x * Math.Sin(Y), Math.Sin(X), n_cos_x * Math.Cos(Y));
            }
        }

        #endregion

    }
}