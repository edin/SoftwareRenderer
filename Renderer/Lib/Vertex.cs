using System;

namespace Renderer.Lib
{
    public class Vertex
    {
        private Vector m_pos;
        private Vector m_texCoords;
        private Vector m_normal;
        private Vertex m_transformed = null;

        public float GetX()
        {
            return m_pos.x;
        }

        public float GetY()
        {
            return m_pos.y;
        }

        public Vector GetPosition()
        {
            return m_pos;
        }

        public Vector GetTexCoords()
        {
            return m_texCoords;
        }

        public Vector GetNormal()
        {
            return m_normal;
        }

        public Vector Position
        {
            get { return m_pos; }
        }

        public Vector Normal
        {
            get { return m_normal; }
        }

        public Vector TextCoords
        {
            get { return m_texCoords; }
        }

        public Vertex(Vector pos, Vector texCoords, Vector normal)
        {
            m_pos = pos;
            m_texCoords = texCoords;
            m_normal = normal;
        }

        public Vertex Transform(Matrix transform, Matrix normalTransform)
        {
            if (m_transformed == null)
            {
                var _pos = new Vector(0, 0, 0);
                var _texCords = new Vector(0, 0, 0);
                var _norm = new Vector(0, 0, 0);
                m_transformed = new Vertex(_pos, _texCords, _norm);
            }
            transform.TransformTo(m_pos, m_transformed.m_pos);
            normalTransform.TransformTo(m_normal, m_transformed.m_normal);

            m_transformed.m_normal.NormalizeSelf();
            m_transformed.m_texCoords = this.m_texCoords;

            return m_transformed;
        }

        public Vertex PerspectiveDivide()
        {
            return new Vertex(new Vector(m_pos.x / m_pos.w, m_pos.y / m_pos.w, m_pos.z / m_pos.w, m_pos.w), m_texCoords, m_normal);
        }

        public float TriangleAreaTimesTwo(Vertex b, Vertex c)
        {
            float x1 = b.GetX() - m_pos.x;
            float y1 = b.GetY() - m_pos.y;

            float x2 = c.GetX() - m_pos.x;
            float y2 = c.GetY() - m_pos.y;

            return (x1 * y2 - x2 * y1);
        }

        public Vertex Lerp(Vertex other, float lerpAmt)
        {
            return new Vertex(
                    m_pos.Lerp(other.GetPosition(), lerpAmt),
                    m_texCoords.Lerp(other.GetTexCoords(), lerpAmt),
                    m_normal.Lerp(other.GetNormal(), lerpAmt)
                    );
        }

        public bool IsInsideViewFrustum()
        {
            return
                Math.Abs(m_pos.x) <= Math.Abs(m_pos.w) &&
                Math.Abs(m_pos.y) <= Math.Abs(m_pos.w) &&
                Math.Abs(m_pos.z) <= Math.Abs(m_pos.w);
        }

        public float Get(int index)
        {
            switch (index)
            {
                case 0:
                    return m_pos.x;

                case 1:
                    return m_pos.y;

                case 2:
                    return m_pos.z;

                case 3:
                    return m_pos.w;

                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}