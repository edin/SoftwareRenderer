using System;

namespace Renderer.Lib
{
    public class Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(Vector axis, float angle)
        {
            float sinHalfAngle = (float)Math.Sin(angle / 2);
            float cosHalfAngle = (float)Math.Cos(angle / 2);

            this.x = axis.x * sinHalfAngle;
            this.y = axis.y * sinHalfAngle;
            this.z = axis.z * sinHalfAngle;
            this.w = cosHalfAngle;
        }

        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        public Quaternion Normalized()
        {
            float length = Length();

            return new Quaternion(x / length, y / length, z / length, w / length);
        }

        public Quaternion Conjugate()
        {
            return new Quaternion(-x, -y, -z, w);
        }

        public Quaternion Mul(float r)
        {
            return new Quaternion(x * r, y * r, z * r, w * r);
        }

        public Quaternion Mul(Quaternion r)
        {
            float w_ = w * r.w - x * r.x - y * r.y - z * r.z;
            float x_ = x * r.w + w * r.x + y * r.z - z * r.y;
            float y_ = y * r.w + w * r.y + z * r.x - x * r.z;
            float z_ = z * r.w + w * r.z + x * r.y - y * r.x;

            return new Quaternion(x_, y_, z_, w_);
        }

        public Quaternion Mul(Vector r)
        {
            float w_ = -x * r.x - y * r.y - z * r.z;
            float x_ = w * r.x + y * r.z - z * r.y;
            float y_ = w * r.y + z * r.x - x * r.z;
            float z_ = w * r.z + x * r.y - y * r.x;

            return new Quaternion(x_, y_, z_, w_);
        }

        public Quaternion Sub(Quaternion r)
        {
            return new Quaternion(x - r.x, y - r.y, z - r.z, w - r.w);
        }

        public Quaternion Add(Quaternion r)
        {
            return new Quaternion(x + r.x, y + r.y, z + r.z, w + r.w);
        }

        public Matrix ToRotationMatrix()
        {
            Vector forward = new Vector(2.0f * (x * z - w * y), 2.0f * (y * z + w * x), 1.0f - 2.0f * (x * x + y * y));
            Vector up = new Vector(2.0f * (x * y + w * z), 1.0f - 2.0f * (x * x + z * z), 2.0f * (y * z - w * x));
            Vector right = new Vector(1.0f - 2.0f * (y * y + z * z), 2.0f * (x * y - w * z), 2.0f * (x * z + w * y));

            return new Matrix().InitRotation(forward, up, right);
        }

        public float Dot(Quaternion r)
        {
            return x * r.x + y * r.y + z * r.z + w * r.w;
        }

        public Quaternion NLerp(Quaternion dest, float lerpFactor, bool shortest)
        {
            Quaternion correctedDest = dest;

            if (shortest && this.Dot(dest) < 0)
                correctedDest = new Quaternion(-dest.x, -dest.y, -dest.z, -dest.w);

            return correctedDest.Sub(this).Mul(lerpFactor).Add(this).Normalized();
        }

        public Quaternion SLerp(Quaternion dest, float lerpFactor, bool shortest)
        {
            float EPSILON = 1e3f;

            float cos = this.Dot(dest);
            Quaternion correctedDest = dest;

            if (shortest && cos < 0)
            {
                cos = -cos;
                correctedDest = new Quaternion(-dest.x, -dest.y, -dest.z, -dest.w);
            }

            if (Math.Abs(cos) >= 1 - EPSILON)
                return NLerp(correctedDest, lerpFactor, false);

            float sin = (float)Math.Sqrt(1.0f - cos * cos);
            float angle = (float)Math.Atan2(sin, cos);
            float invSin = 1.0f / sin;

            float srcFactor = (float)Math.Sin((1.0f - lerpFactor) * angle) * invSin;
            float destFactor = (float)Math.Sin((lerpFactor) * angle) * invSin;

            return this.Mul(srcFactor).Add(correctedDest.Mul(destFactor));
        }

        //From Ken Shoemake's "Quaternion Calculus and Fast Animation" article
        public Quaternion(Matrix rot)
        {
            float trace = rot.Get(0, 0) + rot.Get(1, 1) + rot.Get(2, 2);

            if (trace > 0)
            {
                float s = 0.5f / (float)Math.Sqrt(trace + 1.0f);
                w = 0.25f / s;
                x = (rot.Get(1, 2) - rot.Get(2, 1)) * s;
                y = (rot.Get(2, 0) - rot.Get(0, 2)) * s;
                z = (rot.Get(0, 1) - rot.Get(1, 0)) * s;
            }
            else
            {
                if (rot.Get(0, 0) > rot.Get(1, 1) && rot.Get(0, 0) > rot.Get(2, 2))
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + rot.Get(0, 0) - rot.Get(1, 1) - rot.Get(2, 2));
                    w = (rot.Get(1, 2) - rot.Get(2, 1)) / s;
                    x = 0.25f * s;
                    y = (rot.Get(1, 0) + rot.Get(0, 1)) / s;
                    z = (rot.Get(2, 0) + rot.Get(0, 2)) / s;
                }
                else if (rot.Get(1, 1) > rot.Get(2, 2))
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + rot.Get(1, 1) - rot.Get(0, 0) - rot.Get(2, 2));
                    w = (rot.Get(2, 0) - rot.Get(0, 2)) / s;
                    x = (rot.Get(1, 0) + rot.Get(0, 1)) / s;
                    y = 0.25f * s;
                    z = (rot.Get(2, 1) + rot.Get(1, 2)) / s;
                }
                else
                {
                    float s = 2.0f * (float)Math.Sqrt(1.0f + rot.Get(2, 2) - rot.Get(0, 0) - rot.Get(1, 1));
                    w = (rot.Get(0, 1) - rot.Get(1, 0)) / s;
                    x = (rot.Get(2, 0) + rot.Get(0, 2)) / s;
                    y = (rot.Get(1, 2) + rot.Get(2, 1)) / s;
                    z = 0.25f * s;
                }
            }

            float length = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
            x /= length;
            y /= length;
            z /= length;
            w /= length;
        }

        public Vector GetForward()
        {
            return new Vector(0, 0, 1, 1).Rotate(this);
        }

        public Vector GetBack()
        {
            return new Vector(0, 0, -1, 1).Rotate(this);
        }

        public Vector GetUp()
        {
            return new Vector(0, 1, 0, 1).Rotate(this);
        }

        public Vector GetDown()
        {
            return new Vector(0, -1, 0, 1).Rotate(this);
        }

        public Vector GetRight()
        {
            return new Vector(1, 0, 0, 1).Rotate(this);
        }

        public Vector GetLeft()
        {
            return new Vector(-1, 0, 0, 1).Rotate(this);
        }

        public bool equals(Quaternion r)
        {
            return x == r.x && y == r.y && z == r.z && w == r.w;
        }
    }
}