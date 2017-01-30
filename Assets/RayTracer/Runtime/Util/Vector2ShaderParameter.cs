using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class Vector2ShaderParameter : IShaderParameter<Vector2>
    {
        private ComputeShader m_Shader;
        private int m_NameID;
        private Vector2 m_Value;

        public Vector2 value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                m_Shader.SetVector(m_NameID, value);
            }
        }

        public Vector2ShaderParameter(ComputeShader shader, string name)
        {
            m_Shader = shader;
            m_NameID = Shader.PropertyToID(name);
        }
    }
}
