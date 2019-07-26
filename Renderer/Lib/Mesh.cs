using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Renderer.Lib
{
    public class Mesh
    {
        private List<Vertex> m_vertices;
        private List<int> m_indices;

        public Mesh(String fileName)
        {
            IndexedModel model = new OBJModel(fileName).ToIndexedModel();

            m_vertices = new List<Vertex>();
            for (int i = 0; i < model.GetPositions().Count; i++)
            {
                m_vertices.Add(new Vertex(
                            model.GetPositions()[i],
                            model.GetTexCoords()[i],
                            model.GetNormals()[i]));
            }

            m_indices = model.GetIndices();
        }

        public void Draw(RenderContext context, Matrix viewProjection, Matrix transform, RenderBitmap texture)
        {
            Matrix mvp = viewProjection.Mul(transform);
            int n = m_indices.Count;

            Parallel.For(0, (n / 3), i =>
            {
                int k = i * 3;
                var a = m_vertices[m_indices[k]].Transform(mvp, transform);
                var b = m_vertices[m_indices[k + 1]].Transform(mvp, transform);
                var c = m_vertices[m_indices[k + 2]].Transform(mvp, transform);
                context.DrawTriangle(a, b, c, texture);
            });

            //for (int i = 0; i < n ; i = i + 3)
            //{
            //    var a = m_vertices[m_indices[i]].Transform(mvp, transform);
            //    var b = m_vertices[m_indices[i + 1]].Transform(mvp, transform);
            //    var c = m_vertices[m_indices[i + 2]].Transform(mvp, transform);
            //    context.DrawTriangle(a, b, c, texture);
            //}
        }
    }
}