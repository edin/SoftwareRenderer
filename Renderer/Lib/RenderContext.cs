using System;
using System.Collections.Generic;
using System.Linq;

namespace Renderer.Lib
{
    public class RenderContext : RenderBitmap
    {
        private float[] m_zBuffer;
        private Matrix screenSpaceTransform;

        public RenderContext(int width, int height)
            : base(width, height)
        {
            m_zBuffer = new float[width * height];
            screenSpaceTransform = new Matrix().InitScreenSpaceTransform(m_width / 2, m_height / 2);
        }

        public void ClearDepthBuffer()
        {
            int n = m_zBuffer.Length - 1;
            float max = float.MaxValue;

            unsafe
            {
                fixed (float* first = &m_zBuffer[0])
                fixed (float* last = &m_zBuffer[n])
                {
                    for (float* val = first; val <= last; val++)
                    {
                        *val = max;
                    }
                }
            }

            //Parallel.For(0, n, i =>
            //{
            //    m_zBuffer[i] = max;
            //});
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, RenderBitmap texture)
        {
            if (v1.IsInsideViewFrustum() && v2.IsInsideViewFrustum() && v3.IsInsideViewFrustum())
            {
                FillTriangle(v1, v2, v3, texture);
                return;
            }

            List<Vertex> vertices = new List<Vertex>();
            List<Vertex> auxillaryList = new List<Vertex>();

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            if (ClipPolygonAxis(vertices, auxillaryList, 0) &&
                    ClipPolygonAxis(vertices, auxillaryList, 1) &&
                    ClipPolygonAxis(vertices, auxillaryList, 2))
            {
                Vertex initialVertex = vertices[0];

                for (int i = 1; i < vertices.Count - 1; i++)
                {
                    FillTriangle(initialVertex, vertices[i], vertices[i + 1], texture);
                }
            }
        }

        private bool ClipPolygonAxis(List<Vertex> vertices, List<Vertex> auxillaryList,
                int componentIndex)
        {
            ClipPolygonComponent(vertices, componentIndex, 1.0f, auxillaryList);
            vertices.Clear();

            if (auxillaryList.Count() == 0)
            {
                return false;
            }

            ClipPolygonComponent(auxillaryList, componentIndex, -1.0f, vertices);
            auxillaryList.Clear();

            return !(vertices.Count == 0);
        }

        private void ClipPolygonComponent(List<Vertex> vertices, int componentIndex,
                float componentFactor, List<Vertex> result)
        {
            Vertex previousVertex = vertices[vertices.Count - 1];
            float previousComponent = previousVertex.Get(componentIndex) * componentFactor;
            bool previousInside = previousComponent <= previousVertex.GetPosition().w;

            foreach (Vertex currentVertex in vertices)
            {
                float currentComponent = currentVertex.Get(componentIndex) * componentFactor;
                bool currentInside = currentComponent <= currentVertex.GetPosition().w;

                if (currentInside ^ previousInside)
                {
                    float lerpAmt = (previousVertex.GetPosition().w - previousComponent) /
                        ((previousVertex.GetPosition().w - previousComponent) -
                         (currentVertex.GetPosition().w - currentComponent));

                    result.Add(previousVertex.Lerp(currentVertex, lerpAmt));
                }

                if (currentInside)
                {
                    result.Add(currentVertex);
                }

                previousVertex = currentVertex;
                previousComponent = currentComponent;
                previousInside = currentInside;
            }
        }

        private void FillTriangle3(Vertex v1, Vertex v2, Vertex v3, RenderBitmap texture)
        {
            Matrix screenSpaceTransform = new Matrix().InitScreenSpaceTransform(m_width / 2, m_height / 2);

            Vertex a = v1.Transform(screenSpaceTransform, Matrix.Identity).PerspectiveDivide();
            Vertex b = v2.Transform(screenSpaceTransform, Matrix.Identity).PerspectiveDivide();
            Vertex c = v3.Transform(screenSpaceTransform, Matrix.Identity).PerspectiveDivide();

            var triangle = new Triangle(a, b, c);

            var normal = (v1.Position - v2.Position) | (v1.Position - v3.Position);

            if (normal.z > 0)
            {
                return;
            }

            foreach (PixelInfo p in triangle.GetPixels(m_width, m_height))
            {
                int x = (int)p.position.x;
                int y = (int)p.position.y;

                int index = y * m_width + x;
                if (p.position.z < m_zBuffer[index])
                {
                    var tv = (a.TextCoords * p.FactorA) + (b.TextCoords * p.FactorB) + (c.TextCoords * p.FactorC);

                    int u = (int)(tv.x * (texture.m_width - 1) + 0.5);
                    int v = (int)(tv.y * (texture.m_height - 1) + 0.5);

                    if (u >= texture.m_width) u = texture.m_width - 1;
                    if (v >= texture.m_height) v = texture.m_height - 1;
                    if (u < 0) u = 0;
                    if (v < 0) v = 0;

                    CopyPixel(x, y, u, v, texture, 1.0f);
                    m_zBuffer[index] = p.position.z;
                }
            }
        }

        private void FillTriangle(Vertex v1, Vertex v2, Vertex v3, RenderBitmap texture)
        {
            Vertex minYVert = v1.Transform(screenSpaceTransform, Matrix.Identity).PerspectiveDivide();
            Vertex midYVert = v2.Transform(screenSpaceTransform, Matrix.Identity).PerspectiveDivide();
            Vertex maxYVert = v3.Transform(screenSpaceTransform, Matrix.Identity).PerspectiveDivide();

            if (minYVert.TriangleAreaTimesTwo(maxYVert, midYVert) >= 0)
            {
                return;
            }

            if (maxYVert.GetY() < midYVert.GetY())
            {
                Vertex temp = maxYVert;
                maxYVert = midYVert;
                midYVert = temp;
            }

            if (midYVert.GetY() < minYVert.GetY())
            {
                Vertex temp = midYVert;
                midYVert = minYVert;
                minYVert = temp;
            }

            if (maxYVert.GetY() < midYVert.GetY())
            {
                Vertex temp = maxYVert;
                maxYVert = midYVert;
                midYVert = temp;
            }

            ScanTriangle(minYVert, midYVert, maxYVert, minYVert.TriangleAreaTimesTwo(maxYVert, midYVert) >= 0, texture);
        }

        private void ScanTriangle(Vertex minYVert, Vertex midYVert, Vertex maxYVert, bool handedness, RenderBitmap texture)
        {
            Gradients gradients = new Gradients(minYVert, midYVert, maxYVert);
            Edge topToBottom = new Edge(gradients, minYVert, maxYVert, 0);
            Edge topToMiddle = new Edge(gradients, minYVert, midYVert, 0);
            Edge middleToBottom = new Edge(gradients, midYVert, maxYVert, 1);

            ScanEdges(gradients, topToBottom, topToMiddle, handedness, texture);
            ScanEdges(gradients, topToBottom, middleToBottom, handedness, texture);
        }

        private void ScanEdges(Gradients gradients, Edge a, Edge b, bool handedness, RenderBitmap texture)
        {
            Edge left = a;
            Edge right = b;

            if (handedness)
            {
                Edge temp = left;
                left = right;
                right = temp;
            }

            int yStart = b.GetYStart();
            int yEnd = b.GetYEnd();

            for (int j = yStart; j < yEnd; j++)
            {
                DrawScanLine(gradients, left, right, j, texture);
                left.Step();
                right.Step();
            }
        }

        private void DrawScanLine(Gradients gradients, Edge left, Edge right, int j, RenderBitmap texture)
        {
            int xMin = (int)Math.Ceiling(left.GetX());
            int xMax = (int)Math.Ceiling(right.GetX());
            float xPrestep = xMin - left.GetX();

            float texCoordXXStep = gradients.GetTexCoordXXStep();
            float texCoordYXStep = gradients.GetTexCoordYXStep();
            float oneOverZXStep = gradients.GetOneOverZXStep();
            float depthXStep = gradients.GetDepthXStep();
            float lightAmtXStep = gradients.GetLightAmtXStep();

            float texCoordX = left.GetTexCoordX() + texCoordXXStep * xPrestep;
            float texCoordY = left.GetTexCoordY() + texCoordYXStep * xPrestep;
            float oneOverZ = left.GetOneOverZ() + oneOverZXStep * xPrestep;
            float depth = left.GetDepth() + depthXStep * xPrestep;
            float lightAmt = left.GetLightAmt() + lightAmtXStep * xPrestep;

            float text_w = (float)(texture.m_width - 1) + 0.5f;
            float text_h = (float)(texture.m_height - 1) + 0.5f;

            int rowStart = j * m_width;

            for (int i = xMin; i < xMax; ++i)
            {
                int index = rowStart + i;

                if (depth < m_zBuffer[index])
                {
                    m_zBuffer[index] = depth;
                    float z = 1.0f / oneOverZ;
                    int srcX = (int)((texCoordX * z) * text_w);
                    int srcY = (int)((texCoordY * z) * text_h);

                    CopyPixel(i, j, srcX, srcY, texture, lightAmt);
                }

                oneOverZ += oneOverZXStep;
                texCoordX += texCoordXXStep;
                texCoordY += texCoordYXStep;
                depth += depthXStep;
                lightAmt += lightAmtXStep;
            }
        }
    }
}