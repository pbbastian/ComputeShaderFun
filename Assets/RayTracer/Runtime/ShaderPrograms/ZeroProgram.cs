using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class ZeroProgram
    {
        const string kBuffer = "g_Buffer";

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public ZeroProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/Zero");
            m_KernelIndex = m_Shader.FindKernel("Zero");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(CommandBuffer cb, ComputeBuffer buffer, int count)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kBuffer, buffer);
            cb.DispatchCompute(m_Shader, m_KernelIndex, count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
