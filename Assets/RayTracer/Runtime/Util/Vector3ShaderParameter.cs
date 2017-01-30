using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class Vector3ShaderParameter : IShaderParameter<Vector3>
    {
        private ComputeShader m_Shader;
        private int m_NameID;
        private Vector3 m_Value;

        public Vector3 value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                m_Shader.SetVector(m_NameID, value);
            }
        }

        public Vector3ShaderParameter(ComputeShader shader, string name)
        {
            m_Shader = shader;
            m_NameID = Shader.PropertyToID(name);
        }
    }
}
