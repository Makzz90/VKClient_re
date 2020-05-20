using System;

namespace Microsoft.Phone.Applications.Common
{
  public class Simple3DVector
  {
    public double X { get; private set; }

    public double Y { get; private set; }

    public double Z { get; private set; }

    public double Magnitude
    {
      get
      {
        return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
      }
    }

    public Simple3DVector()
    {
    }

    public Simple3DVector(double x, double y, double z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    public static bool operator ==(Simple3DVector v1, Simple3DVector v2)
    {
      if (v1 == v2)
        return true;
      if (v1 == null || v2 == null || (v1.X != v2.X || v1.Y != v2.Y))
        return false;
      return v1.Z == v2.Z;
    }

    public static bool operator !=(Simple3DVector v1, Simple3DVector v2)
    {
      return !(v1 == v2);
    }

    public static Simple3DVector operator +(Simple3DVector v1, Simple3DVector v2)
    {
      return new Simple3DVector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }

    public static Simple3DVector operator -(Simple3DVector v1, Simple3DVector v2)
    {
      return new Simple3DVector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
    }

    public static Simple3DVector operator *(Simple3DVector v1, Simple3DVector v2)
    {
      return new Simple3DVector(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
    }

    public static Simple3DVector operator *(Simple3DVector v, double d)
    {
      return new Simple3DVector(d * v.X, d * v.Y, d * v.Z);
    }

    public static Simple3DVector operator /(Simple3DVector v, double d)
    {
      return new Simple3DVector(v.X / d, v.Y / d, v.Z / d);
    }

    public override string ToString()
    {
      return string.Format("({0},{1},{2})", this.X, this.Y, this.Z);
    }

    public override bool Equals(object o)
    {
      if ((o as Simple3DVector) != null)
        return this == (Simple3DVector) o;
      return false;
    }

    public override int GetHashCode()
    {
      double num1 = this.X;
      int hashCode1 = num1.GetHashCode();
      num1 = this.Y;
      int hashCode2 = num1.GetHashCode();
      int num2 = hashCode1 ^ hashCode2;
      num1 = this.Z;
      int hashCode3 = num1.GetHashCode();
      return num2 ^ hashCode3;
    }
  }
}
