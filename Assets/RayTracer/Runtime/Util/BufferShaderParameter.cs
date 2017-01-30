using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class BufferShaderParameter : IShaderParameter<ComputeBuffer>
    {
        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private int m_NameID;
        private ComputeBuffer m_Buffer;

        public ComputeBuffer value
        {
            get { return m_Buffer; }
            set
            {
                m_Shader.SetBuffer(m_KernelIndex, m_NameID, value);
                m_Buffer = value;
            }
        }

        public BufferShaderParameter(ComputeShader shader, int kernelIndex, string name)
        {
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            m_NameID = Shader.PropertyToID(name);
        }
    }
}
