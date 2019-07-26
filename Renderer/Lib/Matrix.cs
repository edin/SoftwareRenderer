using System;

namespace Renderer.Lib
{
    public class Matrix
    {
        private float[][] m;

        public static Matrix Identity = new Matrix().InitIdentity();

        public Matrix()
        {
            m = new float[4][] {
                new float[4],
                new float[4],
                new float[4],
                new float[4],
            };
        }

        public Matrix InitIdentity()
        {
            m[0][0] = 1;
            m[0][1] = 0;
            m[0][2] = 0;
            m[0][3] = 0;

            m[1][0] = 0;
            m[1][1] = 1;
            m[1][2] = 0;
            m[1][3] = 0;

            m[2][0] = 0;
            m[2][1] = 0;
            m[2][2] = 1;
            m[2][3] = 0;

            m[3][0] = 0;
            m[3][1] = 0;
            m[3][2] = 0;
            m[3][3] = 1;

            return this;
        }

        public Matrix InitScreenSpaceTransform(float halfWidth, float halfHeight)
        {
            m[0][0] = halfWidth;
            m[0][1] = 0;
            m[0][2] = 0;
            m[0][3] = halfWidth - 0.5f;

            m[1][0] = 0;
            m[1][1] = -halfHeight;
            m[1][2] = 0;
            m[1][3] = halfHeight - 0.5f;

            m[2][0] = 0;
            m[2][1] = 0;
            m[2][2] = 1;
            m[2][3] = 0;

            m[3][0] = 0;
            m[3][1] = 0;
            m[3][2] = 0;
            m[3][3] = 1;
            return this;
        }

        public Matrix InitTranslation(float x, float y, float z)
        {
            m[0][0] = 1;
            m[0][1] = 0;
            m[0][2] = 0;
            m[0][3] = x;

            m[1][0] = 0;
            m[1][1] = 1;
            m[1][2] = 0;
            m[1][3] = y;

            m[2][0] = 0;
            m[2][1] = 0;
            m[2][2] = 1;
            m[2][3] = z;

            m[3][0] = 0;
            m[3][1] = 0;
            m[3][2] = 0;
            m[3][3] = 1;
            return this;
        }

        public Matrix InitRotation(float x, float y, float z, float angle)
        {
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            m[0][0] = cos + x * x * (1 - cos);
            m[0][1] = x * y * (1 - cos) - z * sin;
            m[0][2] = x * z * (1 - cos) + y * sin;
            m[0][3] = 0;

            m[1][0] = y * x * (1 - cos) + z * sin;
            m[1][1] = cos + y * y * (1 - cos);
            m[1][2] = y * z * (1 - cos) - x * sin;
            m[1][3] = 0;

            m[2][0] = z * x * (1 - cos) - y * sin;
            m[2][1] = z * y * (1 - cos) + x * sin;
            m[2][2] = cos + z * z * (1 - cos);
            m[2][3] = 0;

            m[3][0] = 0;
            m[3][1] = 0;
            m[3][2] = 0;
            m[3][3] = 1;

            return this;
        }

        public Matrix InitRotation(float x, float y, float z)
        {
            Matrix rx = new Matrix();
            Matrix ry = new Matrix();
            Matrix rz = new Matrix();

            rz.m[0][0] = (float)Math.Cos(z);
            rz.m[0][1] = -(float)Math.Sin(z);
            rz.m[0][2] = 0;
            rz.m[0][3] = 0;

            rz.m[1][0] = (float)Math.Sin(z);
            rz.m[1][1] = (float)Math.Cos(z);
            rz.m[1][2] = 0;
            rz.m[1][3] = 0;

            rz.m[2][0] = 0;
            rz.m[2][1] = 0;
            rz.m[2][2] = 1;
            rz.m[2][3] = 0;

            rz.m[3][0] = 0;
            rz.m[3][1] = 0;
            rz.m[3][2] = 0;
            rz.m[3][3] = 1;

            rx.m[0][0] = 1;
            rx.m[0][1] = 0;
            rx.m[0][2] = 0;
            rx.m[0][3] = 0;

            rx.m[1][0] = 0;
            rx.m[1][1] = (float)Math.Cos(x);
            rx.m[1][2] = -(float)Math.Sin(x);
            rx.m[1][3] = 0;

            rx.m[2][0] = 0;
            rx.m[2][1] = (float)Math.Sin(x);
            rx.m[2][2] = (float)Math.Cos(x);
            rx.m[2][3] = 0;

            rx.m[3][0] = 0;
            rx.m[3][1] = 0;
            rx.m[3][2] = 0;
            rx.m[3][3] = 1;

            ry.m[0][0] = (float)Math.Cos(y);
            ry.m[0][1] = 0;
            ry.m[0][2] = -(float)Math.Sin(y);
            ry.m[0][3] = 0;

            ry.m[1][0] = 0;
            ry.m[1][1] = 1;
            ry.m[1][2] = 0;
            ry.m[1][3] = 0;

            ry.m[2][0] = (float)Math.Sin(y);
            ry.m[2][1] = 0;
            ry.m[2][2] = (float)Math.Cos(y);
            ry.m[2][3] = 0;

            ry.m[3][0] = 0;
            ry.m[3][1] = 0;
            ry.m[3][2] = 0;
            ry.m[3][3] = 1;

            m = rz.Mul(ry.Mul(rx)).GetM();
            return this;
        }

        public Matrix InitScale(float x, float y, float z)
        {
            m[0][0] = x; m[0][1] = 0; m[0][2] = 0; m[0][3] = 0;
            m[1][0] = 0; m[1][1] = y; m[1][2] = 0; m[1][3] = 0;
            m[2][0] = 0; m[2][1] = 0; m[2][2] = z; m[2][3] = 0;
            m[3][0] = 0; m[3][1] = 0; m[3][2] = 0; m[3][3] = 1;

            return this;
        }

        public Matrix InitPerspective(float fov, float aspectRatio, float zNear, float zFar)
        {
            float tanHalfFOV = (float)Math.Tan(fov / 2);
            float zRange = zNear - zFar;

            m[0][0] = 1.0f / (tanHalfFOV * aspectRatio); m[0][1] = 0; m[0][2] = 0; m[0][3] = 0;
            m[1][0] = 0; m[1][1] = 1.0f / tanHalfFOV; m[1][2] = 0; m[1][3] = 0;
            m[2][0] = 0; m[2][1] = 0; m[2][2] = (-zNear - zFar) / zRange; m[2][3] = 2 * zFar * zNear / zRange;
            m[3][0] = 0; m[3][1] = 0; m[3][2] = 1; m[3][3] = 0;

            return this;
        }

        public Matrix InitOrthographic(float left, float right, float bottom, float top, float near, float far)
        {
            float width = right - left;
            float height = top - bottom;
            float depth = far - near;

            m[0][0] = 2 / width; m[0][1] = 0; m[0][2] = 0; m[0][3] = -(right + left) / width;
            m[1][0] = 0; m[1][1] = 2 / height; m[1][2] = 0; m[1][3] = -(top + bottom) / height;
            m[2][0] = 0; m[2][1] = 0; m[2][2] = -2 / depth; m[2][3] = -(far + near) / depth;
            m[3][0] = 0; m[3][1] = 0; m[3][2] = 0; m[3][3] = 1;

            return this;
        }

        public Matrix InitRotation(Vector forward, Vector up)
        {
            Vector f = forward.Normalized;
            Vector r = up.Normalized;

            r = r | f;
            Vector u = f | r;
            return InitRotation(f, u, r);
        }

        public Matrix InitRotation(Vector forward, Vector up, Vector right)
        {
            Vector f = forward;
            Vector r = right;
            Vector u = up;

            m[0][0] = r.x; m[0][1] = r.y; m[0][2] = r.z; m[0][3] = 0;
            m[1][0] = u.x; m[1][1] = u.y; m[1][2] = u.z; m[1][3] = 0;
            m[2][0] = f.x; m[2][1] = f.y; m[2][2] = f.z; m[2][3] = 0;
            m[3][0] = 0; m[3][1] = 0; m[3][2] = 0; m[3][3] = 1;

            return this;
        }

        public Vector Transform(Vector r)
        {
            return new Vector(m[0][0] * r.x + m[0][1] * r.y + m[0][2] * r.z + m[0][3] * r.w,
                              m[1][0] * r.x + m[1][1] * r.y + m[1][2] * r.z + m[1][3] * r.w,
                              m[2][0] * r.x + m[2][1] * r.y + m[2][2] * r.z + m[2][3] * r.w,
                              m[3][0] * r.x + m[3][1] * r.y + m[3][2] * r.z + m[3][3] * r.w);
        }

        public void TransformTo(Vector r, Vector result)
        {
            var a = m[0];
            var b = m[1];
            var c = m[2];
            var d = m[3];

            result.x = a[0] * r.x + a[1] * r.y + a[2] * r.z + a[3] * r.w;
            result.y = b[0] * r.x + b[1] * r.y + b[2] * r.z + b[3] * r.w;
            result.z = c[0] * r.x + c[1] * r.y + c[2] * r.z + c[3] * r.w;
            result.w = d[0] * r.x + d[1] * r.y + d[2] * r.z + d[3] * r.w;
        }

        public Matrix Mul(Matrix r)
        {
            Matrix res = new Matrix();

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    res.Set(i, j, m[i][0] * r.Get(0, j) +
                            m[i][1] * r.Get(1, j) +
                            m[i][2] * r.Get(2, j) +
                            m[i][3] * r.Get(3, j));
                }

            return res;
        }

        public float[][] GetM()
        {
            var res = new float[4][]{
                new float[4],
                new float[4],
                new float[4],
                new float[4]
            };

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    res[i][j] = m[i][j];

            return res;
        }

        public float Get(int x, int y)
        {
            return m[x][y];
        }

        public void SetM(float[][] m)
        {
            this.m = m;
        }

        public void Set(int x, int y, float value)
        {
            m[x][y] = value;
        }
    }
}