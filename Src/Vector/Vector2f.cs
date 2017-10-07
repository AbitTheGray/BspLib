using System;

namespace BspLib.Vector
{
    /// <summary>
    /// 2D Vector made of <see cref="System.Single"/>.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Vector2f
    {
        public const int MemorySize = 2 * sizeof(float);

        private readonly float _x, _y;

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
        /// Create new instance of 2D Vector
        /// </summary>
        /// <param name="x">X Coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        public Vector2f(float x, float y)
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
            return (int)X + (int)Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2f)
                return Vector2f.Equals(this, (Vector2f)obj);
            else if (obj is Vector2)
                return Vector2f.Equals(this, (Vector2)obj);
            else
                return false;
        }

        public static bool Equals(Vector2f v1, Vector2f v2)
        {
            return v1.Y == v2.Y && v1.X == v2.X;
        }

        #endregion

        #region Functions

        public static float Dot(Vector2f v1, Vector2f v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        /// <summary>
        /// Distance to specified port.
        /// </summary>
        /// <param name="v">Second vector.</param>
        public double DistanceTo(Vector2 v)
        {
            float x = v.X - this.X;
            float y = v.Y - this.Y;
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
            float x = v.X - this.X;
            float y = v.Y - this.Y;
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

        public static Vector2f operator -(Vector2f v)
        {
            return new Vector2f(-v.X, -v.Y);
        }

        public static Vector2f operator +(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2f operator -(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2f operator *(Vector2f v, float m)
        {
            return new Vector2f(v.X * m, v.Y * m);
        }

        public static Vector2f operator /(Vector2f v, float d)
        {
            return new Vector2f(v.X / d, v.Y / d);
        }

        public static bool operator ==(Vector2f v1, Vector2f v2)
        {
            return Vector2f.Equals(v1, v2);
        }

        public static bool operator !=(Vector2f v1, Vector2f v2)
        {
            return !Vector2f.Equals(v1, v2);
        }

        #endregion

        #region Vector2f = Vector2

        public static implicit operator Vector2f(Vector2 v)
        {
            return new Vector2f(v.X, v.Y);
        }

        public static explicit operator Vector2(Vector2f v)
        {
            return new Vector2((int)v.X, (int)v.Y);
        }

        #endregion

        #region Default Values

        /// <summary>
        /// Zero Vector.<br>
        /// [0, 0]
        /// </summary>
        public static readonly Vector2f Zero = new Vector2f(0, 0);

        /// <summary>
        /// One Vector.<br>
        /// [1, 1]
        /// </summary>
        public static readonly Vector2f One = new Vector2f(1, 1);

        /// <summary>
        /// Half Vector.<br>
        /// [0.5, 0.5]
        /// </summary>
        public static readonly Vector2f Half = new Vector2f(0.5f, 0.5f);

        /// <summary>
        /// Unit Y Vector.<br>
        /// [0, 1]
        /// </summary>
        public static readonly Vector2f UnitY = new Vector2f(0, 1);

        /// <summary>
        /// Unit X Vector.<br>
        /// [1, 0]
        /// </summary>
        public static readonly Vector2f UnitX = new Vector2f(1, 0);

        /// <summary>
        /// Forward Vector.<br>
        /// [0, 1]
        /// </summary>
        public static readonly Vector2f North = new Vector2f(0, 1);

        /// <summary>
        /// Back Vector.<br>
        /// [0, -1]
        /// </summary>
        public static readonly Vector2f South = new Vector2f(0, -1);

        /// <summary>
        /// Right Vector.<br>
        /// [1, 0]
        /// </summary>
        public static readonly Vector2f East = new Vector2f(1, 0);

        /// <summary>
        /// Left Vector.<br>
        /// [-1, 0]
        /// </summary>
        public static readonly Vector2f West = new Vector2f(-1, 0);

        #endregion

        #region Reordered

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector2f XY
        {
            get
            {
                return new Vector2f(X, Y);
            }
        }

        /// <summary>
        /// Re-ordered vector
        /// </summary>
        public Vector2f YX
        {
            get
            {
                return new Vector2f(Y, X);
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
        /// Create vector from a rotation in radian.
        /// </summary>
        /// <param name="radian">Rotation.</param>
        public static Vector2f FromRotationRadian(double radian)
        {
            return new Vector2f((float)Math.Sin(radian), (float)Math.Cos(radian));
        }

        /// <summary>
        /// Create vector from a rotation in degree.
        /// </summary>
        /// <param name="radian">Rotation.</param>
        public static Vector2f FromRotationDegree(double degree)
        {
            return FromRotationRadian(degree / 180 * Math.PI);
        }

        #endregion

    }
}