using System;

namespace Renderer.Lib
{
    public class Vector
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector(float x, float y, float z, float w = 1)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
            }
        }

        public void NormalizeSelf()
        {
            float f = (float)(1.0 / Length);
            x = x * f;
            y = y * f;
            z = z * f;
            w = w * f;
        }

        public Vector Normalized
        {
            get
            {
                float d = Length;
                return this * (float)(1.0 / d);
            }
        }

        public Vector Rotate(Vector axis, float angle)
        {
            float sinAngle = (float)Math.Sin(-angle);
            float cosAngle = (float)Math.Cos(-angle);

            var v = this * (axis * (1.0f - cosAngle));

            return (this | (axis * sinAngle)) + ((this * cosAngle) + (axis * v));
        }

        public Vector Rotate(Quaternion rotation)
        {
            Quaternion conjugate = rotation.Conjugate();

            Quaternion w = rotation.Mul(this).Mul(conjugate);

            return new Vector(w.x, w.y, w.z, 1.0f);
        }

        public Vector Sub(Vector v)
        {
            return this - v;
        }

        public Vector Add(Vector v)
        {
            return this + v;
        }

        public Vector Mul(float f)
        {
            return this * f;
        }

        public Vector Mul(Vector v)
        {
            return this | v;
        }

        public float Dot(Vector v)
        {
            return this * v;
        }

        public static Vector operator *(Vector v, float f)
        {
            return new Vector(v.x * f, v.y * f, v.z * f, v.w * f);
        }

        public static float operator *(Vector a, Vector b)
        {
            return (a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w);
        }

        public static Vector operator /(Vector a, float f)
        {
            return new Vector(a.x / f, a.y / f, a.z / f, a.w / f);
        }

        public static Vector operator /(Vector a, Vector b)
        {
            return new Vector(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        }

        public Vector Cross(Vector v)
        {
            return this | v;
        }

        public static Vector operator |(Vector a, Vector b)
        {
            float vx = a.y * b.z - a.z * b.y;
            float vy = a.z * b.x - a.x * b.z;
            float vz = a.x * b.y - a.y * b.x;
            return new Vector(vx, vy, vz, 0);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public Vector Lerp(Vector dest, float lerpFactor)
        {
            return dest.Sub(this).Mul(lerpFactor).Add(this);
        }
    }
}