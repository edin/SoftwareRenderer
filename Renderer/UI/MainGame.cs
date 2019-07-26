using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace Renderer.UI
{
    internal class MainGame : Game
    {
        private Buffer<Vector2> _screenTriangle;
        private VertexInputLayout _vertexInputLayout;
        private RasterizerState _noneCullingState;
        private GraphicsDeviceManager deviceManager;

        public MainGame()
        {
            deviceManager = new GraphicsDeviceManager(this);
            deviceManager.PreferredBackBufferWidth = 480;
            deviceManager.PreferredBackBufferHeight = 800;
        }

        protected override void Initialize()
        {
            _screenTriangle = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(GraphicsDevice, new[] {
                new Vector2(-1.0f, -1.0f),
                new Vector2(3.0f, -1.0f),
                new Vector2(-1.0f, 3.0f)
            }, SharpDX.Direct3D11.ResourceUsage.Immutable);

            _vertexInputLayout = VertexInputLayout.New(
                        VertexBufferLayout.New(0, new VertexElement[] { new VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32_Float, 0) }, 0));

            var rasterizerStateDesc = SharpDX.Direct3D11.RasterizerStateDescription.Default();
            rasterizerStateDesc.CullMode = SharpDX.Direct3D11.CullMode.None;
            _noneCullingState = RasterizerState.New(GraphicsDevice, "CullModeNone", rasterizerStateDesc);

            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            var device = this.GraphicsDevice;
            device.Clear(Color.CornflowerBlue);

            device.SetRasterizerState(_noneCullingState);
            device.SetVertexBuffer(_screenTriangle);
            device.SetVertexInputLayout(_vertexInputLayout);
            //device.Draw(PrimitiveType.TriangleList, 3);

            base.Draw(gameTime);
        }
    }
}