using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class ZeroProgram
    {
        private static readonly int s_BufferId = Shader.PropertyToID("g_Buffer");
        
        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public ZeroProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/Zero");
            m_KernelIndex = m_Shader.FindKernel("Zero");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
        }

        public void Dispatch(ComputeBuffer buffer, int count)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_BufferId, buffer);
            m_Shader.Dispatch(m_KernelIndex, count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}

