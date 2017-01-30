using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public class TextureShaderParameter<T> : IShaderParameter<T>
        where T : Texture
    {
        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private int m_NameID;
        private T m_Texture;

        public T value
        {
            get { return m_Texture; }
            set
            {
                m_Shader.SetTexture(m_KernelIndex, m_NameID, value);
                m_Texture = value;
            }
        }

        public TextureShaderParameter(ComputeShader shader, int kernelIndex, string name)
        {
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            m_NameID = Shader.PropertyToID(name);
        }
    }
}
