using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 2D Vector made of <see cref="System.Int32"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector2
    {
        public const int MemorySize = 2 * sizeof(int);

        private readonly int _x, _y;

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
        /// Create new instance of 2D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        public Vector2(int x, int y)
        {
            this._x = x;
            this._y = y;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", X, Y);
        }

        #region Compare

        public override int GetHashCode()
        {
            return X + Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
                return Vector2.Equals(this, (Vector2)obj);
            else
                return false;
        }

        public static bool Equals(Vector2 v1, Vector2 v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X;
        }

        #endregion

        #region Functions

        public static int Dot(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector2 v)
        {
            int x = v.X - this.X;
            int y = v.Y - this.Y;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector2f v)
        {
            float x = v.X - this.X;
            float y = v.Y - this.Y;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector2d v)
        {
            double x = v.X - this.X;
            double y = v.Y - this.Y;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Angle to vector in radians.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double AngleToRadian(Vector2 v)
        {
            int x = v.X - this.X;
            int y = v.Y - this.Y;
            return Math.Atan2(y, x);
        }

        /// <summary>
        /// Angle to vector in radians.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double AngleToRadian(Vector2f v)
        {
            float x = v.X - this.X;
            float y = v.Y - this.Y;
            return Math.Atan2(y, x);
        }

        /// <summary>
        /// Angle to vector in radians.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double AngleToRadian(Vector2d v)
        {
            double x = v.X - this.X;
            double y = v.Y - this.Y;
            return Math.Atan2(y, x);
        }

        /// <summary>
        /// Angle to vector in degree.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double AngleToDegree(Vector2 v)
        {
            return AngleToRadian(v) / Math.PI * 180;
        }

        /// <summary>
        /// Angle to vector in degree.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double AngleToDegree(Vector2f v)
        {
            return AngleToRadian(v) / Math.PI * 180;
        }

        /// <summary>
        /// Angle to vector in degree.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double AngleToDegree(Vector2d v)
        {
            return AngleToRadian(v) / Math.PI * 180;
        }

        /// <summary>
        /// Length of this vector
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y);
            }
        }

        /// <summary>
        /// Length of this vector casted to <see cref="System.Single"/>
        /// </summary>
        public float LengthF
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
        }

        /// <summary>
        /// Get Normalized version of this vector.<br>
        /// Normalized vector has Length equal to 1.
        /// </summary>
        public Vector2d Normalized
        {
            get
            {
                double length = Length;
                return new Vector2d(X / length, Y / length);
            }
        }

        /// <summary>
        /// Get Normalized version of this vector.<br>
        /// Normalized vector has Length equal to 1.
        /// </summary>
        public Vector2f NormalizedF
        {
            get
            {
                double length = Length;
                return new Vector2f((float)(X / length), (float)(Y / length));
            }
        }

        #endregion

        #region Operators

        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.X, -v.Y);
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 operator *(Vector2 v, int m)
        {
            return new Vector2(v.X * m, v.Y * m);
        }

        public static Vector2 operator /(Vector2 v, int d)
        {
            return new Vector2(v.X / d, v.Y / d);
        }

        public static Vector2f operator *(Vector2 v, float m)
        {
            return new Vector2f(v.X * m, v.Y * m);
        }

        public static Vector2f operator /(Vector2 v, float d)
        {
            return new Vector2f(v.X / d, v.Y / d);
        }

        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            return Vector2.Equals(v1, v2);
        }

        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return !Vector2.Equals(v1, v2);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0]
        /// </summary>
        public static readonly Vector2 Zero = new Vector2(0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1]
        /// </summary>
        public static readonly Vector2 One = new Vector2(1, 1);

        /// <summary>
        /// Unit Y Vector.<br>
        /// [0, 1]
        /// </summary>
        public static readonly Vector2 UnitY = new Vector2(0, 1);

        /// <summary>
        /// Unit X Vector.<br>
        /// [1, 0]
        /// </summary>
        public static readonly Vector2 UnitX = new Vector2(1, 0);

        /// <summary>
        /// Forward Vector.<br>
        /// [0, 1]
        /// </summary>
        public static readonly Vector2 North = new Vector2(0, 1);

        /// <summary>
        /// Back Vector.<br>
        /// [0, -1]
        /// </summary>
        public static readonly Vector2 South = new Vector2(0, -1);

        /// <summary>
        /// Right Vector.<br>
        /// [1, 0]
        /// </summary>
        public static readonly Vector2 East = new Vector2(1, 0);

        /// <summary>
        /// Left Vector.<br>
        /// [-1, 0]
        /// </summary>
        public static readonly Vector2 West = new Vector2(-1, 0);

        #endregion

        #region Reordered

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector2 XY
        {
            get
            {
                return new Vector2(X, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector2 YX
        {
            get
            {
                return new Vector2(Y, X);
            }
        }

        #endregion

        #region Eulers

        /// <summary>
        /// Returns rotation around 3rd axis in radian.
        /// </summary>
        public double ToRotationRadian()
        {
            return Math.Atan2(Y, X);
        }

        /// <summary>
        /// Returns rotation around 3rd axis in degree.
        /// </summary>
        public double ToRotationDegree()
        {
            return ToRotationRadian() / Math.PI * 180;
        }

        /// <summary>
        /// Create vector from a rotation in radian.<br>
        /// Vector length is multipled by 100.
        /// </summary>
        /// <param name="radian">Rotation in radian.</param>
        public static Vector2 FromRotationRadian(double radian)
        {
            return new Vector2((int)(Math.Sin(radian) * 100), (int)(Math.Cos(radian) * 100));
        }

        /// <summary>
        /// Create vector from a rotation in degree.<br>
        /// Vector length is multipled by 100.
        /// </summary>
        /// <param name="degree">Rotation in degree.</param>
        public static Vector2 FromRotationDegree(double degree)
        {
            return FromRotationRadian(degree / 180 * Math.PI);
        }

        #endregion

    }
}