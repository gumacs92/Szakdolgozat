using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Szakdolgozat.Kinect.ToolBox
{
    [Serializable]
    public struct Vector
    {
        public float X;
        public float Y;
        public float Z;
        public static Vector Zero
        {
            get
            {
                return new Vector(0, 0, 0);
            }
        }
        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }
        public static Vector operator -(Vector left, Vector right)
        {
            return new Vector(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector operator +(Vector left, Vector right)
        {
            return new Vector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector operator *(Vector left, float value)
        {
            return new Vector(left.X * value, left.Y * value, left.Z * value);
        }
        public static Vector operator *(float value, Vector left)
        {
            return left * value;
        }
        public static Vector operator /(Vector left, float value)
        {
            return new Vector(left.X / value, left.Y / value, left.Z / value);
        }
        public static Vector ToVector(SkeletonPoint vector)
        {
            return new Vector(vector.X, vector.Y, vector.Z);
        }
    }
}
