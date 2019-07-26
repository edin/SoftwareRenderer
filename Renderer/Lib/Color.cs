namespace Renderer.Lib
{
    internal class Color
    {
        private int r;
        private int g;
        private int b;

        public Color(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public static Color operator *(Color c, float f)
        {
            return new Color((int)(c.r * f), (int)(c.g * f), (int)(c.b * f));
        }

        public static Color operator +(Color a, Color b)
        {
            return new Color(a.r + b.r, a.g + b.g, a.b + b.g);
        }

        public System.Drawing.Color ToColor()
        {
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;

            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}