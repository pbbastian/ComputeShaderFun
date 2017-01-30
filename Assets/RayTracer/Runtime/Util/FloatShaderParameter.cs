using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public sealed class FloatShaderParameter : IShaderParameter<float>
    {
        private ComputeShader m_Shader;
        private int m_NameID;
        private float m_Value;

        public float value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                m_Shader.SetFloat(m_NameID, value);
            }
        }

        public FloatShaderParameter(ComputeShader shader, string name)
        {
            m_Shader = shader;
            m_NameID = Shader.PropertyToID(name);
        }
    }
}
