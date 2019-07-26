namespace Renderer.Lib
{
    public class Transform
    {
        private Vector m_pos;
        private Quaternion m_rot;
        private Vector m_scale;

        public Transform()
            : this(new Vector(0, 0, 0, 0))
        {
        }

        public Transform(Vector pos)
            : this(pos, new Quaternion(0, 0, 0, 1), new Vector(1, 1, 1, 1))
        {
        }

        public Transform(Vector pos, Quaternion rot, Vector scale)
        {
            m_pos = pos;
            m_rot = rot;
            m_scale = scale;
        }

        public Transform SetPos(Vector pos)
        {
            return new Transform(pos, m_rot, m_scale);
        }

        public Transform Rotate(Quaternion rotation)
        {
            return new Transform(m_pos, rotation.Mul(m_rot).Normalized(), m_scale);
        }

        public Transform LookAt(Vector point, Vector up)
        {
            return Rotate(GetLookAtRotation(point, up));
        }

        public Quaternion GetLookAtRotation(Vector point, Vector up)
        {
            return new Quaternion(new Matrix().InitRotation(point.Sub(m_pos).Normalized, up));
        }

        public Matrix GetTransformation()
        {
            Matrix translationMatrix = new Matrix().InitTranslation(m_pos.x, m_pos.y, m_pos.z);
            Matrix rotationMatrix = m_rot.ToRotationMatrix();
            Matrix scaleMatrix = new Matrix().InitScale(m_scale.x, m_scale.y, m_scale.z);
            return translationMatrix.Mul(rotationMatrix.Mul(scaleMatrix));
        }

        public Vector GetTransformedPos()
        {
            return m_pos;
        }

        public Quaternion GetTransformedRot()
        {
            return m_rot;
        }

        public Vector GetPos()
        {
            return m_pos;
        }

        public Quaternion GetRot()
        {
            return m_rot;
        }

        public Vector GetScale()
        {
            return m_scale;
        }
    }
}