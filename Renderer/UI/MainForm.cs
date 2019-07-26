using Renderer.Lib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Renderer
{
    public partial class MainForm : Form
    {
        private RenderContext target;
        private Input input;
        private Stopwatch stopWatch = new Stopwatch();
        private Camera camera;

        private Lib.RenderBitmap texture;
        private Lib.RenderBitmap texture2;
        private Mesh terrainMesh;
        private Transform terrainTransform;
        private long previousTime;
        private System.Drawing.Bitmap bmp;

        //private System.Timers.Timer m_Timer;
        private Mesh monkeyMesh;

        private Transform monkeyTransform;

        private bool m_Drawing = false;
        private int m_SkipCount = 0;

        private System.Threading.Thread m_Thread;
        private bool m_IsRunning;
        private int m_Counter;
        private bool m_CheckBitmap;

        private object m_lock = new object();

        public MainForm()
        {
            InitializeComponent();
            this.bmp = new System.Drawing.Bitmap(800, 600, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            //this.DoubleBuffered = true;
        }

        private void MainForLoad(object sender, EventArgs e)
        {
            //m_Timer = new System.Timers.Timer(15);
            //m_Timer.Elapsed += OnTimer_Elapsed;
            //m_Timer.Start();

            m_IsRunning = true;

            stopWatch.Start();

            this.WindowState = FormWindowState.Maximized;

            this.target = new RenderContext(this.Width, this.Height);

            this.texture = new Lib.RenderBitmap("Resource/bricks.jpg");
            this.texture2 = new Lib.RenderBitmap("Resource/bricks2.jpg");
            this.monkeyMesh = new Mesh("Resource/monkey.obj");
            this.monkeyTransform = new Transform(new Vector(0, 0.0f, 3.0f));

            this.terrainMesh = new Mesh("Resource/terrain.obj");
            this.terrainTransform = new Transform(new Vector(0, -1.0f, 0.0f));

            this.input = new Input();
            this.camera = new Camera(new Matrix().InitPerspective(Utils.ToRadians(70.0f), ((float)target.m_width / target.m_height), 0.1f, 1000.0f));

            UpdateBitmapSize();

            m_Thread = new System.Threading.Thread(() =>
            {
                var sw = new Stopwatch();
                long time = 0;
                long endTime = 0;
                long nextTime = 0;
                float fps = 0.0f;

                using (var gx = this.CreateGraphics())
                {
                    sw.Start();

                    while (m_IsRunning)
                    {
                        time = sw.ElapsedMilliseconds;

                        if (time > nextTime)
                        {
                            nextTime = nextTime + 10;
                            Draw(gx);
                        }

                        //m_Counter++;

                        //if (m_Counter >= 15)
                        //{
                        //    endTime = sw.ElapsedMilliseconds;
                        //    this.BeginInvoke((Action)(() =>
                        //    {
                        //        this.Text = fps.ToString();
                        //    }));
                        //}
                    }
                }
            });
            m_Thread.Start();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        //const int WM_PAINT = 0x0F;

        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == WM_PAINT)
        //    {
        //        return;
        //    }
        //    base.WndProc(ref m);
        //}

        private void UpdateBitmapSize()
        {
            if (bmp == null) return;
            if (bmp.Width != this.Width || bmp.Height != this.Height)
            {
                bmp.Dispose();
                bmp = new System.Drawing.Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                this.target = new RenderContext(this.Width, this.Height);
                m_CheckBitmap = false;
            }
        }

        private void Draw(Graphics g)
        {
            //lock (m_lock)
            //{
            //    UpdateBitmapSize();
            //}

            lock (m_lock)
            {
                //if (m_CheckBitmap)
                {
                    //return;
                }

                long currentTime = stopWatch.ElapsedMilliseconds;
                float delta = (float)((currentTime - previousTime) / 500.0);

                previousTime = currentTime;
                camera.Update(this.input, delta);
                Matrix vp = camera.GetViewProjection();

                monkeyTransform = monkeyTransform.Rotate(new Quaternion(new Vector(0, 1, 0), delta));

                target.m_bitmap = bmp;

                target.Begin();

                target.Clear(180);
                target.ClearDepthBuffer();
                monkeyMesh.Draw(target, vp, monkeyTransform.GetTransformation(), texture2);
                terrainMesh.Draw(target, vp, terrainTransform.GetTransformation(), texture);

                target.End();

                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            m_CheckBitmap = true;
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_IsRunning = false;
            m_Thread.Join();
        }
    }
}