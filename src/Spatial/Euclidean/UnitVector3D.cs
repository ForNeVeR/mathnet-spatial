using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Units;

namespace MathNet.Spatial.Euclidean
{
    /// <summary>
    /// A unit vector, this is used to describe a direction in 3D
    /// </summary>
    [Serializable]
    public struct UnitVector3D : IXmlSerializable, IEquatable<UnitVector3D>, IEquatable<Vector3D>, IFormattable
    {
        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double X;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Z;

        public UnitVector3D(double x, double y, double z)
        {
            var l = Math.Sqrt((x*x) + (y*y) + (z*z));
            if (l < float.Epsilon)
            {
                throw new ArgumentException("l < float.Epsilon");
            }

            this.X = x/l;
            this.Y = y/l;
            this.Z = z/l;
        }

        public UnitVector3D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        public UnitVector3D(double[] data)
            : this(data[0], data[1], data[2])
        {
            if (data.Length != 3)
            {
                throw new ArgumentException("Size must be 3");
            }
        }

        /// <summary>
        /// A vector orthogonbal to this
        /// </summary>
        public UnitVector3D Orthogonal
        {
            get
            {
                if (-this.X - this.Y > 0.1)
                {
                    return new UnitVector3D(this.Z, this.Z, -this.X - this.Y);
                }

                return new UnitVector3D(-this.Y - this.Z, this.X, this.X);
            }
        }

        /// <summary>
        /// Creates a UnitVector3D from its string representation
        /// </summary>
        /// <param name="s">The string representation of the UnitVector3D</param>
        /// <returns></returns>
        public static UnitVector3D Parse(string s)
        {
            var doubles = Parser.ParseItem3D(s);
            return new UnitVector3D(doubles);
        }

        public static bool operator ==(UnitVector3D left, UnitVector3D right)
        {
            return left.Equals(right);
        }

        public static bool operator ==(Vector3D left, UnitVector3D right)
        {
            return left.Equals(right);
        }

        public static bool operator ==(UnitVector3D left, Vector3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnitVector3D left, UnitVector3D right)
        {
            return !left.Equals(right);
        }

        public static bool operator !=(Vector3D left, UnitVector3D right)
        {
            return !left.Equals(right);
        }

        public static bool operator !=(UnitVector3D left, Vector3D right)
        {
            return !left.Equals(right);
        }

        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Matrix<double> left, UnitVector3D right)
        {
            return left*right.ToVector();
        }

        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(UnitVector3D left, Matrix<double> right)
        {
            return left.ToVector()*right;
        }

        public static double operator *(UnitVector3D left, UnitVector3D right)
        {
            return left.DotProduct(right);
        }

        public override string ToString()
        {
            return this.ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            return this.ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            string separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format("({0}{1} {2}{1} {3})", this.X.ToString(format, numberFormatInfo), separator, this.Y.ToString(format, numberFormatInfo), this.Z.ToString(format, numberFormatInfo));
        }

        public bool Equals(Vector3D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(UnitVector3D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(UnitVector3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - this.X) < tolerance &&
                   Math.Abs(other.Y - this.Y) < tolerance &&
                   Math.Abs(other.Z - this.Z) < tolerance;
        }

        public bool Equals(Vector3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - this.X) < tolerance &&
                   Math.Abs(other.Y - this.Y) < tolerance &&
                   Math.Abs(other.Z - this.Z) < tolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return (obj is UnitVector3D && this.Equals((UnitVector3D)obj)) ||
                   (obj is Vector3D && this.Equals((Vector3D)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.X.GetHashCode();
                hashCode = (hashCode*397) ^ this.Y.GetHashCode();
                hashCode = (hashCode*397) ^ this.Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);

            // Hacking set readonly fields here, can't think of a cleaner workaround
            XmlExt.SetReadonlyField(ref this, x => x.X, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("X")));
            XmlExt.SetReadonlyField(ref this, x => x.Y, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Y")));
            XmlExt.SetReadonlyField(ref this, x => x.Z, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Z")));
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute("X", this.X);
            writer.WriteAttribute("Y", this.Y);
            writer.WriteAttribute("Z", this.Z);
        }

        public static UnitVector3D ReadFrom(XmlReader reader)
        {
            var v = new UnitVector3D();
            v.ReadXml(reader);
            return v;
        }

        public static UnitVector3D XAxis
        {
            get { return new UnitVector3D(1, 0, 0); }
        }

        public static UnitVector3D YAxis
        {
            get { return new UnitVector3D(0, 1, 0); }
        }

        public static UnitVector3D ZAxis
        {
            get { return new UnitVector3D(0, 0, 1); }
        }

        internal Matrix<double> CrossProductMatrix
        {
            get { return Matrix<double>.Build.Dense(3, 3, new[] { 0d, Z, -Y, -Z, 0d, X, Y, -X, 0d }); }
        }

        /// <summary>
        /// The length of the vector not the count of elements
        /// </summary>
        public double Length
        {
            get { return 1; }
        }

        public static Vector3D operator +(UnitVector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3D operator +(Vector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3D operator +(UnitVector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3D operator -(UnitVector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3D operator -(Vector3D v1, UnitVector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3D operator -(UnitVector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3D operator -(UnitVector3D v)
        {
            return new Vector3D(-1*v.X, -1*v.Y, -1*v.Z);
        }

        public static Vector3D operator *(double d, UnitVector3D v)
        {
            return new Vector3D(d*v.X, d*v.Y, d*v.Z);
        }

        // Commented out because the d * v reads nicer than v *d
        ////public static Vector3D operator *(Vector3D v,double d)
        ////{
        ////    return d*v;
        ////}

        public static Vector3D operator /(UnitVector3D v, double d)
        {
            return new Vector3D(v.X/d, v.Y/d, v.Z/d);
        }

        ////public static explicit operator UnitVector3D(System.Windows.Media.Media3D.Vector3D v)
        ////{
        ////    return new UnitVector3D(v.X, v.Y, v.Z);
        ////}

        ////public static explicit operator System.Windows.Media.Media3D.Vector3D(UnitVector3D p)
        ////{
        ////    return new System.Windows.Media.Media3D.Vector3D(p.X, p.Y, p.Z);
        ////}

        public Vector3D ScaleBy(double scaleFactor)
        {
            return scaleFactor*this;
        }

        [Pure]
        public Ray3D ProjectOn(Plane planeToProjectOn)
        {
            return planeToProjectOn.Project(this.ToVector3D());
        }

        public Vector3D ProjectOn(UnitVector3D uv)
        {
            double pd = DotProduct(uv);
            return pd*this;
        }

        [Pure]
        public bool IsParallelTo(Vector3D othervector, double tolerance = 1e-6)
        {
            var other = othervector.Normalize();
            var dp = Math.Abs(this.DotProduct(other));
            return Math.Abs(1 - dp) < tolerance;
        }

        [Pure]
        public bool IsParallelTo(UnitVector3D othervector, double tolerance = 1e-6)
        {
            var dp = Math.Abs(this.DotProduct(othervector));
            return Math.Abs(1 - dp) < tolerance;
        }

        [Pure]
        public bool IsPerpendicularTo(Vector3D othervector, double tolerance = 1e-6)
        {
            var other = othervector.Normalize();
            return Math.Abs(this.DotProduct(other)) < tolerance;
        }

        [Pure]
        public bool IsPerpendicularTo(UnitVector3D othervector, double tolerance = 1e-6)
        {
            return Math.Abs(this.DotProduct(othervector)) < tolerance;
        }

        [Pure]
        public UnitVector3D Negate()
        {
            return new UnitVector3D(-1*this.X, -1*this.Y, -1*this.Z);
        }

        [Pure]
        public double DotProduct(Vector3D v)
        {
            return (this.X*v.X) + (this.Y*v.Y) + (this.Z*v.Z);
        }

        [Pure]
        public double DotProduct(UnitVector3D v)
        {
            var dp = (this.X*v.X) + (this.Y*v.Y) + (this.Z*v.Z);
            return Math.Max(-1, Math.Min(dp, 1));
        }

        [Obsolete("Use - instead")]
        public Vector3D Subtract(UnitVector3D v)
        {
            return new Vector3D(this.X - v.X, this.Y - v.Y, this.Z - v.Z);
        }

        [Obsolete("Use + instead")]
        public Vector3D Add(UnitVector3D v)
        {
            return new Vector3D(this.X + v.X, this.Y + v.Y, this.Z + v.Z);
        }

        public UnitVector3D CrossProduct(UnitVector3D inVector3D)
        {
            var x = (this.Y*inVector3D.Z) - (this.Z*inVector3D.Y);
            var y = (this.Z*inVector3D.X) - (this.X*inVector3D.Z);
            var z = (this.X*inVector3D.Y) - (this.Y*inVector3D.X);
            var v = new UnitVector3D(x, y, z);
            return v;
        }

        public Vector3D CrossProduct(Vector3D inVector3D)
        {
            var x = (this.Y*inVector3D.Z) - (this.Z*inVector3D.Y);
            var y = (this.Z*inVector3D.X) - (this.X*inVector3D.Z);
            var z = (this.X*inVector3D.Y) - (this.Y*inVector3D.X);
            var v = new Vector3D(x, y, z);
            return v;
        }

        public Matrix<double> GetUnitTensorProduct()
        {
            // unitTensorProduct:matrix([ux^2,ux*uy,ux*uz],[ux*uy,uy^2,uy*uz],[ux*uz,uy*uz,uz^2]),
            double xy = X*Y;
            double xz = X*Z;
            double yz = Y*Z;
            return Matrix<double>.Build.Dense(3, 3, new[] { X*X, xy, xz, xy, Y*Y, yz, xz, yz, Z*Z });
        }

        /// <summary>
        /// Returns signed angle
        /// </summary>
        /// <param name="v">The fromVector3D to calculate the signed angle to </param>
        /// <param name="about">The vector around which to rotate to get the correct sign</param>
        public Angle SignedAngleTo(Vector3D v, UnitVector3D about)
        {
            return SignedAngleTo(v.Normalize(), about);
        }

        /// <summary>
        /// Returns signed angle
        /// </summary>
        /// <param name="v">The fromVector3D to calculate the signed angle to </param>
        /// <param name="about">The vector around which to rotate to get the correct sign</param>
        public Angle SignedAngleTo(UnitVector3D v, UnitVector3D about)
        {
            if (IsParallelTo(about))
            {
                throw new ArgumentException("FromVector paralell to aboutVector");
            }

            if (v.IsParallelTo(about))
            {
                throw new ArgumentException("FromVector paralell to aboutVector");
            }

            var rp = new Plane(new Point3D(0, 0, 0), about);
            var pfv = ProjectOn(rp).Direction;
            var ptv = v.ProjectOn(rp).Direction;
            var dp = pfv.DotProduct(ptv);
            if (Math.Abs(dp - 1) < 1E-15)
            {
                return new Angle(0, AngleUnit.Radians);
            }

            if (Math.Abs(dp + 1) < 1E-15)
            {
                return new Angle(Math.PI, AngleUnit.Radians);
            }

            var angle = Math.Acos(dp);
            var cpv = pfv.CrossProduct(ptv);
            var sign = cpv.DotProduct(rp.Normal);
            var signedAngle = sign*angle;
            return new Angle(signedAngle, AngleUnit.Radians);
        }

        /// <summary>
        /// The nearest angle between the vectors
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns>The angle</returns>
        public Angle AngleTo(Vector3D v)
        {
            return AngleTo(v.Normalize());
        }

        /// <summary>
        /// The nearest angle between the vectors
        /// </summary>
        /// <param name="v">The other vector</param>
        /// <returns>The angle</returns>
        public Angle AngleTo(UnitVector3D v)
        {
            var dp = this.DotProduct(v);
            var angle = Math.Acos(dp);
            return new Angle(angle, AngleUnit.Radians);
        }

        /// <summary>
        /// Returns a vector that is this vector rotated the signed angle around the about vector
        /// </summary>
        /// <param name="about"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public UnitVector3D Rotate<T>(UnitVector3D about, double angle, T angleUnit) where T : IAngleUnit
        {
            return this.Rotate(about, Angle.From(angle, angleUnit));
        }

        /// <summary>
        /// Returns a vector that is this vector rotated the signed angle around the about vector
        /// </summary>
        /// <param name="about"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public UnitVector3D Rotate(UnitVector3D about, Angle angle)
        {
            var cs = CoordinateSystem.Rotation(angle, about);
            return cs.Transform(this).Normalize();
        }

        public Point3D ToPoint3D()
        {
            return new Point3D(this.X, this.Y, this.Z);
        }

        [Pure]
        public Vector3D ToVector3D()
        {
            return new Vector3D(this.X, this.Y, this.Z);
        }

        public Vector3D TransformBy(CoordinateSystem coordinateSystem)
        {
            return coordinateSystem.Transform(this.ToVector3D());
        }

        public Vector3D TransformBy(Matrix<double> m)
        {
            return new Vector3D(m.Multiply(this.ToVector()));
        }

        /// <summary>
        /// Convert to a Math.NET Numerics dense vector of length 3.
        /// </summary>
        public Vector<double> ToVector()
        {
            return Vector<double>.Build.Dense(new[] { X, Y, Z });
        }
    }
}
