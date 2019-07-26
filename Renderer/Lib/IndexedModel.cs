using System.Collections.Generic;
using System.Linq;

namespace Renderer.Lib
{
    public class IndexedModel
    {
        private List<Vector> m_positions;
        private List<Vector> m_texCoords;
        private List<Vector> m_normals;
        private List<Vector> m_tangents;
        private List<int> m_indices;

        public IndexedModel()
        {
            m_positions = new List<Vector>();
            m_texCoords = new List<Vector>();
            m_normals = new List<Vector>();
            m_tangents = new List<Vector>();
            m_indices = new List<int>();
        }

        public void CalcNormals()
        {
            for (int i = 0; i < m_indices.Count(); i += 3)
            {
                int i0 = m_indices[i];
                int i1 = m_indices[i + 1];
                int i2 = m_indices[i + 2];

                Vector v1 = m_positions[i1].Sub(m_positions[i0]);
                Vector v2 = m_positions[i2].Sub(m_positions[i0]);

                Vector normal = v1.Cross(v2).Normalized;

                m_normals[i0] = m_normals[i0].Add(normal);
                m_normals[i1] = m_normals[i1].Add(normal);
                m_normals[i2] = m_normals[i2].Add(normal);
            }

            for (int i = 0; i < m_normals.Count(); i++)
                m_normals[i] = m_normals[i].Normalized;
        }

        public void CalcTangents()
        {
            for (int i = 0; i < m_indices.Count(); i += 3)
            {
                int i0 = m_indices[i];
                int i1 = m_indices[i + 1];
                int i2 = m_indices[i + 2];

                Vector edge1 = m_positions[i1].Sub(m_positions[i0]);
                Vector edge2 = m_positions[i2].Sub(m_positions[i0]);

                float deltaU1 = m_texCoords[i1].x - m_texCoords[i0].x;
                float deltaV1 = m_texCoords[i1].y - m_texCoords[i0].y;
                float deltaU2 = m_texCoords[i2].x - m_texCoords[i0].x;
                float deltaV2 = m_texCoords[i2].y - m_texCoords[i0].y;

                float dividend = (deltaU1 * deltaV2 - deltaU2 * deltaV1);
                float f = dividend == 0 ? 0.0f : 1.0f / dividend;

                Vector tangent = new Vector(
                        f * (deltaV2 * edge1.x - deltaV1 * edge2.x),
                        f * (deltaV2 * edge1.y - deltaV1 * edge2.y),
                        f * (deltaV2 * edge1.z - deltaV1 * edge2.z),
                        0);

                m_tangents[i0] = m_tangents[i0].Add(tangent);
                m_tangents[i1] = m_tangents[i1].Add(tangent);
                m_tangents[i2] = m_tangents[i2].Add(tangent);
            }

            for (int i = 0; i < m_tangents.Count(); i++)
            {
                m_tangents[i] = m_tangents[i].Normalized;
            }
        }

        public List<Vector> GetPositions()
        {
            return m_positions;
        }

        public List<Vector> GetTexCoords()
        {
            return m_texCoords;
        }

        public List<Vector> GetNormals()
        {
            return m_normals;
        }

        public List<Vector> GetTangents()
        {
            return m_tangents;
        }

        public List<int> GetIndices()
        {
            return m_indices;
        }
    }
}