using System.Windows.Forms;

namespace Renderer.Lib
{
    public class Camera
    {
        private static Vector Y_AXIS = new Vector(0, 1, 0);

        private Transform m_transform;
        private Matrix m_projection;

        private Transform GetTransform()
        {
            return m_transform;
        }

        public Camera(Matrix projection)
        {
            this.m_projection = projection;
            this.m_transform = new Transform();
        }

        public Matrix GetViewProjection()
        {
            Matrix cameraRotation = GetTransform().GetTransformedRot().Conjugate().ToRotationMatrix();
            Vector cameraPos = GetTransform().GetTransformedPos().Mul(-1);

            Matrix cameraTranslation = new Matrix().InitTranslation(cameraPos.x, cameraPos.y, cameraPos.z);

            return m_projection.Mul(cameraRotation.Mul(cameraTranslation));
        }

        public void Update(Input input, float delta)
        {
            float sensitivityX = 2.66f * delta;
            float sensitivityY = 2.0f * delta;
            float movAmt = 5.0f * delta;

            if (input.GetKey(Keys.W)) Move(GetTransform().GetRot().GetForward(), movAmt);
            if (input.GetKey(Keys.S)) Move(GetTransform().GetRot().GetForward(), -movAmt);
            if (input.GetKey(Keys.A)) Move(GetTransform().GetRot().GetLeft(), movAmt);
            if (input.GetKey(Keys.D)) Move(GetTransform().GetRot().GetRight(), movAmt);

            if (input.GetKey(Keys.Right)) Rotate(Y_AXIS, sensitivityX);
            if (input.GetKey(Keys.Left)) Rotate(Y_AXIS, -sensitivityX);
            if (input.GetKey(Keys.Down)) Rotate(GetTransform().GetRot().GetRight(), sensitivityY);
            if (input.GetKey(Keys.Up)) Rotate(GetTransform().GetRot().GetRight(), -sensitivityY);
        }

        private void Move(Vector dir, float amt)
        {
            m_transform = GetTransform().SetPos(GetTransform().GetPos().Add(dir.Mul(amt)));
        }

        private void Rotate(Vector axis, float angle)
        {
            m_transform = GetTransform().Rotate(new Quaternion(axis, angle));
        }
    }
}