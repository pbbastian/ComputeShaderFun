using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class Vector4ShaderParameter : IShaderParameter<Vector4>
    {
        private ComputeShader m_Shader;
        private int m_NameID;
        private Vector4 m_Value;

        public Vector4 value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                m_Shader.SetVector(m_NameID, value);
            }
        }

        public Vector4ShaderParameter(ComputeShader shader, string name)
        {
            m_Shader = shader;
            m_NameID = Shader.PropertyToID(name);
        }
    }
}
