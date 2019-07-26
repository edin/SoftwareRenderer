using System;
using System.Collections.Generic;
using System.Linq;

namespace Renderer.Lib
{
    public class OBJModel
    {
        public class Utils
        {
            public static float FloatValueOf(object o)
            {
                return Convert.ToSingle(o);
            }

            public static int ParseInt(object o)
            {
                return Convert.ToInt32(o);
            }
        }

        private class OBJIndex
        {
            private int m_vertexIndex;
            private int m_texCoordIndex;
            private int m_normalIndex;

            public int GetVertexIndex()
            {
                return m_vertexIndex;
            }

            public int GetTexCoordIndex()
            {
                return m_texCoordIndex;
            }

            public int GetNormalIndex()
            {
                return m_normalIndex;
            }

            public void SetVertexIndex(int val)
            {
                m_vertexIndex = val;
            }

            public void SetTexCoordIndex(int val)
            {
                m_texCoordIndex = val;
            }

            public void SetNormalIndex(int val)
            {
                m_normalIndex = val;
            }

            override public bool Equals(Object obj)
            {
                OBJIndex index = (OBJIndex)obj;

                return m_vertexIndex == index.m_vertexIndex
                        && m_texCoordIndex == index.m_texCoordIndex
                        && m_normalIndex == index.m_normalIndex;
            }

            public override int GetHashCode()
            {
                int BASE = 17;
                int MULTIPLIER = 31;

                int result = BASE;

                result = MULTIPLIER * result + m_vertexIndex;
                result = MULTIPLIER * result + m_texCoordIndex;
                result = MULTIPLIER * result + m_normalIndex;

                return result;
            }
        }

        private List<Vector> m_positions;
        private List<Vector> m_texCoords;
        private List<Vector> m_normals;
        private List<OBJIndex> m_indices;
        private bool m_hasTexCoords;
        private bool m_hasNormals;

        private static String[] RemoveEmptyStrings(String[] data)
        {
            List<String> result = new List<String>();

            for (int i = 0; i < data.Length; i++)
                if (!String.IsNullOrEmpty(data[i]))
                {
                    result.Add(data[i]);
                }

            return result.ToArray();
        }

        public OBJModel(String fileName)
        {
            m_positions = new List<Vector>();
            m_texCoords = new List<Vector>();
            m_normals = new List<Vector>();
            m_indices = new List<OBJIndex>();
            m_hasTexCoords = false;
            m_hasNormals = false;

            var meshReader = new System.IO.StreamReader(fileName);
            String line;

            while ((line = meshReader.ReadLine()) != null)
            {
                String[] tokens = line.Split(' ');
                tokens = RemoveEmptyStrings(tokens);

                if (tokens.Length == 0 || tokens[0] == "#")
                    continue;
                else if (tokens[0] == "v")
                {
                    m_positions.Add(new Vector(Utils.FloatValueOf(tokens[1]),
                            Utils.FloatValueOf(tokens[2]),
                            Utils.FloatValueOf(tokens[3]), 1));
                }
                else if (tokens[0] == "vt")
                {
                    m_texCoords.Add(new Vector(Utils.FloatValueOf(tokens[1]),
                            1.0f - Utils.FloatValueOf(tokens[2]), 0, 0));
                }
                else if (tokens[0] == "vn")
                {
                    m_normals.Add(new Vector(Utils.FloatValueOf(tokens[1]),
                            Utils.FloatValueOf(tokens[2]),
                            Utils.FloatValueOf(tokens[3]), 0));
                }
                else if (tokens[0] == "f")
                {
                    for (int i = 0; i < tokens.Length - 3; i++)
                    {
                        m_indices.Add(ParseOBJIndex(tokens[1]));
                        m_indices.Add(ParseOBJIndex(tokens[2 + i]));
                        m_indices.Add(ParseOBJIndex(tokens[3 + i]));
                    }
                }
            }

            meshReader.Dispose();
        }

        public IndexedModel ToIndexedModel()
        {
            IndexedModel result = new IndexedModel();
            IndexedModel normalModel = new IndexedModel();
            Dictionary<OBJIndex, int> resultIndexMap = new Dictionary<OBJIndex, int>();
            Dictionary<int, int> normalIndexMap = new Dictionary<int, int>();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();

            for (int i = 0; i < m_indices.Count(); i++)
            {
                OBJIndex currentIndex = m_indices[i];

                Vector currentPosition = m_positions[currentIndex.GetVertexIndex()];
                Vector currentTexCoord;
                Vector currentNormal;

                if (m_hasTexCoords)
                    currentTexCoord = m_texCoords[currentIndex.GetTexCoordIndex()];
                else
                    currentTexCoord = new Vector(0, 0, 0, 0);

                if (m_hasNormals)
                    currentNormal = m_normals[currentIndex.GetNormalIndex()];
                else
                    currentNormal = new Vector(0, 0, 0, 0);

                int modelVertexIndex = 0;

                if (!resultIndexMap.ContainsKey(currentIndex))
                {
                    modelVertexIndex = result.GetPositions().Count();
                    resultIndexMap.Add(currentIndex, modelVertexIndex);

                    result.GetPositions().Add(currentPosition);
                    result.GetTexCoords().Add(currentTexCoord);
                    if (m_hasNormals)
                        result.GetNormals().Add(currentNormal);
                }
                else
                {
                    modelVertexIndex = resultIndexMap[currentIndex];
                }

                int normalModelIndex = 0;

                if (!normalIndexMap.ContainsKey(currentIndex.GetVertexIndex()))
                {
                    normalModelIndex = normalModel.GetPositions().Count();
                    normalIndexMap.Add(currentIndex.GetVertexIndex(), normalModelIndex);

                    normalModel.GetPositions().Add(currentPosition);
                    normalModel.GetTexCoords().Add(currentTexCoord);
                    normalModel.GetNormals().Add(currentNormal);
                    normalModel.GetTangents().Add(new Vector(0, 0, 0, 0));
                }
                else
                {
                    normalModelIndex = normalIndexMap[currentIndex.GetVertexIndex()];
                }

                result.GetIndices().Add(modelVertexIndex);
                normalModel.GetIndices().Add(normalModelIndex);

                if (!indexMap.ContainsKey(modelVertexIndex))
                {
                    indexMap.Add(modelVertexIndex, normalModelIndex);
                }
            }

            if (!m_hasNormals)
            {
                normalModel.CalcNormals();

                for (int i = 0; i < result.GetPositions().Count; i++)
                    result.GetNormals().Add(normalModel.GetNormals()[indexMap[i]]);
            }

            normalModel.CalcTangents();

            for (int i = 0; i < result.GetPositions().Count; i++)
                result.GetTangents().Add(normalModel.GetTangents()[indexMap[i]]);

            return result;
        }

        private OBJIndex ParseOBJIndex(String token)
        {
            String[] values = token.Split('/');

            OBJIndex result = new OBJIndex();
            result.SetVertexIndex(Utils.ParseInt(values[0]) - 1);

            if (values.Length > 1)
            {
                if (!String.IsNullOrEmpty(values[1]))
                {
                    m_hasTexCoords = true;
                    result.SetTexCoordIndex(Utils.ParseInt(values[1]) - 1);
                }

                if (values.Length > 2)
                {
                    m_hasNormals = true;
                    result.SetNormalIndex(Utils.ParseInt(values[2]) - 1);
                }
            }

            return result;
        }
    }
}