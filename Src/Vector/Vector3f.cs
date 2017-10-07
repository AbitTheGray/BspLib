using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 3D Vector made of <see cref="System.Single"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector3f
    {
        public const int MemorySize = 3 * sizeof(float);

        private readonly float _x, _y, _z;

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
        /// Create new instance of 3D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        /// <param name="z">Z Coordinate.</param>
        public Vector3f(float x, float y, float z)
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
        public Vector3f(Vector2f xy, float z)
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
        public Vector3f(float x, Vector2f yz)
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
            if (obj is Vector3f)
                return Vector3f.Equals(this, (Vector3f)obj);
            else if (obj is Vector3)
                return Vector3f.Equals(this, (Vector3)obj);
            else
                return false;
        }

        public static bool Equals(Vector3f v1, Vector3f v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X && v1.Z == v2.Z;
        }

        #endregion

        #region Functions

        public static float Dot(Vector3f v1, Vector3f v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector3 v)
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
                return (float)Length;
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

        public static Vector3f operator -(Vector3f v)
        {
            return new Vector3f(-v.X, -v.Y, -v.Z);
        }

        public static Vector3f operator +(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3f operator -(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3f operator *(Vector3f v, float m)
        {
            return new Vector3f(v.X * m, v.Y * m, v.Z * m);
        }

        public static Vector3f operator /(Vector3f v, float d)
        {
            return new Vector3f(v.X / d, v.Y / d, v.Z / d);
        }

        public static bool operator ==(Vector3f v1, Vector3f v2)
        {
            return Vector3f.Equals(v1, v2);
        }

        public static bool operator !=(Vector3f v1, Vector3f v2)
        {
            return !Vector3f.Equals(v1, v2);
        }

        #endregion

        #region Vector3f = Vector3

        public static implicit operator Vector3f(Vector3 v)
        {
            return new Vector3f(v.X, v.Y, v.Z);
        }

        public static explicit operator Vector3(Vector3f v)
        {
            return new Vector3((int)v.X, (int)v.Y, (int)v.Z);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0, 0]
        /// </summary>
        public static readonly Vector3f Zero = new Vector3f(0, 0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1, 1]
        /// </summary>
        public static readonly Vector3f One = new Vector3f(1, 1, 1);

        /// <summary>
        /// Half Vector.<br>
        /// [0.5, 0.5, 0.5]
        /// </summary>
        public static readonly Vector3f Half = new Vector3f(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// X+ Vector.<br>
        /// [1, 0, 0]
        /// </summary>
        public static readonly Vector3f UnitX = new Vector3f(1, 0, 0);

        /// <summary>
        /// Y+ Vector.<br>
        /// [0, 1, 0]
        /// </summary>
        public static readonly Vector3f UnitY = new Vector3f(0, 1, 0);

        /// <summary>
        /// Z+ Vector.<br>
        /// [0, 0, 1]
        /// </summary>
        public static readonly Vector3f UnitZ = new Vector3f(0, 0, 1);

        #endregion

        #region 2D

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2f XY
        {
            get
            {
                return new Vector2f(X, Y);
            }
        }

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2f XZ
        {
            get
            {
                return new Vector2f(X, Z);
            }
        }

        /// <summary>
        /// 2D version of this 3D vector skipping one axis.
        /// </summary>
        public Vector2f YZ
        {
            get
            {
                return new Vector2f(Y, Z);
            }
        }

        #endregion

        #region Reordered

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3f XYZ
        {
            get
            {
                return new Vector3f(X, Y, Z);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3f XZY
        {
            get
            {
                return new Vector3f(X, Z, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3f YZX
        {
            get
            {
                return new Vector3f(Y, Z, X);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3f YXZ
        {
            get
            {
                return new Vector3f(Y, X, Z);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3f ZXY
        {
            get
            {
                return new Vector3f(Z, X, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector3f ZYX
        {
            get
            {
                return new Vector3f(Z, Y, X);
            }
        }

        #endregion

        #region Eulers

        /// <summary>
        /// Forward direcrtion vector (from euler angles).
        /// </summary>
        public Vector3f Forward
        {
            get
            {
                double n_cos_x = -Math.Cos(X);
                return new Vector3f((float)(n_cos_x * Math.Sin(Y)), (float)(Math.Sin(X)), (float)(n_cos_x * Math.Cos(Y)));
            }
        }

        #endregion

    }
}