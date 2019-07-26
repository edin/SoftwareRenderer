using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Renderer.Lib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBColor
    {
        public byte b;
        public byte g;
        public byte r;
        public byte a;
    }

    public class RenderBitmap
    {
        public int m_width;
        public int m_height;
        public RGBColor[] m_components;  //Used for texture

        public Bitmap m_bitmap;
        private BitmapData m_data;

        public RenderBitmap(int width, int height)
        {
            m_width = width;
            m_height = height;
            m_components = new RGBColor[m_width * m_height];
        }

        public RenderBitmap(String fileName)
        {
            var image = new System.Drawing.Bitmap(fileName);

            int width = image.Width;
            int height = image.Height;

            m_components = new RGBColor[width * height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    System.Drawing.Color pixel = image.GetPixel(i, j);

                    int k = (j * image.Width + i);

                    m_components[k].a = pixel.A;
                    m_components[k].b = pixel.B;
                    m_components[k].g = pixel.G;
                    m_components[k].r = pixel.R;
                }

            m_width = width;
            m_height = height;
        }

        public void Begin()
        {
            m_data = m_bitmap.LockBits(new Rectangle(0, 0, m_width, m_height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        }

        public void End()
        {
            m_bitmap.UnlockBits(m_data);
        }

        public void Clear(byte shade)
        {
            unsafe
            {
                byte* firstPixel = (byte*)m_data.Scan0;
                var width = m_bitmap.Width;
                var height = m_bitmap.Height;

                int iColor = unchecked((shade << 24) | (shade << 16) | (shade << 8) | shade);
                int stride = m_data.Stride;

                for (var y = 0; y < height; ++y)
                {
                    RGBColor* first = (RGBColor*)(firstPixel + y * stride);
                    RGBColor* last = (RGBColor*)(first + width);
                    for (RGBColor* color = first; color <= last; ++color)
                    {
                        *color = *(RGBColor*)&iColor;
                    }
                }
            }
        }

        public void CopyPixel(int destX, int destY, int srcX, int srcY, RenderBitmap src, float lightAmt)
        {
            int srcIndex = (srcY * src.m_width + srcX);

            unsafe
            {
                RGBColor* rgbColor = (RGBColor*)((byte*)(m_data.Scan0 + m_data.Stride * destY + destX * 4));
                fixed (RGBColor* color = &src.m_components[srcIndex])
                {
                    rgbColor->r = (byte)(color->r * lightAmt);
                    rgbColor->g = (byte)(color->g * lightAmt);
                    rgbColor->b = (byte)(color->b * lightAmt);
                }
            }
        }

        public void ToBitmap(System.Drawing.Bitmap bmp)
        {
            var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* firstPixel = (byte*)data.Scan0;
                var width = bmp.Width;
                var height = bmp.Height;

                for (var y = 0; y < height; ++y)
                {
                    RGBColor* pixel = (RGBColor*)(firstPixel + y * data.Stride);

                    int pos = y * width;

                    fixed (RGBColor* first = &m_components[pos])
                    fixed (RGBColor* last = &m_components[pos + width - 1])
                    {
                        for (RGBColor* color = first; color <= last; color++)
                        {
                            *pixel = *color;
                            pixel->a = 255;
                            pixel++;
                        }
                    }
                }
            }

            bmp.UnlockBits(data);
        }
    }
}