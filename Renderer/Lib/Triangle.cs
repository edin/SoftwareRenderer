using System;

namespace Renderer.Lib
{
    public class Triangle
    {
        private Vertex a;
        private Vertex b;
        private Vertex c;

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        private bool Side(Vector p1, Vector p2, Vector p3)
        {
            return ((p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y)) < 0.0f;
        }

        private bool IsPointInside(Vector pt)
        {
            var b1 = Side(pt, a.Position, b.Position);
            var b2 = Side(pt, b.Position, c.Position);
            if (b1 != b2) return false;

            var b3 = Side(pt, c.Position, a.Position);
            return (b2 == b3);
        }

        private int Max(float a, float b, float c)
        {
            return (int)Math.Ceiling(Math.Max(Math.Max(a, b), c));
        }

        private int Min(float a, float b, float c)
        {
            return (int)Math.Min(Math.Min(a, b), c);
        }

        public BoundingBox GetBoundingBox()
        {
            var box = new BoundingBox();
            box.Xmin = Min(a.Position.x, b.Position.x, c.Position.x);
            box.Xmax = Max(a.Position.x, b.Position.x, c.Position.x);

            box.Ymin = Min(a.Position.y, b.Position.y, c.Position.y);
            box.Ymax = Max(a.Position.y, b.Position.y, c.Position.y);
            return box;
        }

        public System.Collections.Generic.IEnumerable<PixelInfo> GetPixels(int width, int height)
        {
            const float Epsilon = 0.001f;

            /*
             *   m11 m12 m13 | m11 m12 |
             *   m21 m22 m23 | m21 m22 |
             *   m31 m32 m33 | m31 m32 |
             *
             *   m11*m22*m33 + m12 * m23 * m31 + m13 * m12 * m32 - m31*m22*m13 - m32 * m23 * m11 - m33 * m21 * m12  = 0
             *
             *
             *   (m13*m12*m32) - (m31*m22*m13) =  (m32 * m23 * m11) + (m33 * m21 * m12) -(m11*m22*m33) - (m12 * m23 * m31)
             *
             *   m13 =  ((m32 * m23 * m11) + (m33 * m21 * m12) -(m11*m22*m33) - (m12 * m23 * m31)) / (m12*m32 - m31*m22)
             */

            float m21 = b.Position.x - a.Position.x;
            float m22 = b.Position.y - a.Position.y;
            float m23 = b.Position.z - a.Position.z;

            float m31 = c.Position.x - a.Position.x;
            float m32 = c.Position.y - a.Position.y;
            float m33 = c.Position.z - a.Position.z;

            Vector P = new Vector(0, 0, 0);
            float totalArea = ((a.Position - b.Position) | (a.Position - c.Position)).Length;

            if (totalArea < Epsilon)
            {
                yield break;
            }

            float m22_33 = m22 * m33;
            float m22_31 = m22 * m31;
            float m23_32 = m23 * m32;
            float m21_33 = m21 * m33;

            var box = this.GetBoundingBox();
            if (box.Xmax >= width) box.Xmax = width - 1;
            if (box.Ymax >= height) box.Ymax = height - 1;
            if (box.Xmin < 0) box.Xmin = 0;
            if (box.Ymin < 0) box.Ymin = 0;

            var pixelInfo = new PixelInfo();
            var testPoint = new Vector(0, 0, 0);

            for (int y = box.Ymin; y < box.Ymax; ++y)
            {
                for (int x = box.Xmin; x < box.Xmax; ++x)
                {
                    testPoint.x = x + 0.5f;
                    testPoint.y = y + 0.5f;
                    P.x = x;
                    P.y = y;

                    pixelInfo.inside = IsPointInside(testPoint);
                    if (!pixelInfo.inside) continue;

                    float m11 = (x - a.Position.x);
                    float m12 = (y - a.Position.y);
                    float d = (m12 * m32 - m31 * m22);

                    if (d != 0)
                    {
                        float m13 = ((m32 * m23 * m11) + (m33 * m21 * m12) - (m11 * m22 * m33) - (m12 * m23 * m31)) / d;
                        P.z = m13 + a.Position.z;
                    }
                    else
                    {
                        continue;
                    }

                    pixelInfo.position = P;

                    pixelInfo.FactorC = ((P - a.Position) | (P - b.Position)).Length / totalArea;
                    pixelInfo.FactorA = ((P - b.Position) | (P - c.Position)).Length / totalArea;
                    pixelInfo.FactorB = ((P - a.Position) | (P - c.Position)).Length / totalArea;
                    pixelInfo.area = totalArea / 2;
                    yield return pixelInfo;
                }
            }
        }
    }

    public class PixelInfo
    {
        public Vector position;
        public bool inside;
        public float area;
        public float FactorA;
        public float FactorB;
        public float FactorC;
    }
}