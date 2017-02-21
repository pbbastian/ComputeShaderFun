using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixReorderProgram
    {
        private static readonly int s_InputKeyBufferId = Shader.PropertyToID("g_InputKeyBuffer");
        private static readonly int s_InputIndexBufferId = Shader.PropertyToID("g_InputIndexBuffer");
        private static readonly int s_OutputKeyBufferId = Shader.PropertyToID("g_OutputKeyBuffer");
        private static readonly int s_OutputIndexBufferId = Shader.PropertyToID("g_OutputIndexBuffer");
        private static readonly int s_HistogramBufferId = Shader.PropertyToID("g_HistogramBuffer");
        private static readonly int s_CountBufferId = Shader.PropertyToID("g_CountBuffer");
        private static readonly int s_LimitId = Shader.PropertyToID("g_Limit");
        private static readonly int s_KeyShiftId = Shader.PropertyToID("g_KeyShift");

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public RadixReorderProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/RadixReorder");
            var kernelName = "RadixReorder";
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
        }

        public void Dispatch(ComputeBuffer inputKeyBuffer, ComputeBuffer outputKeyBuffer, ComputeBuffer inputIndexBuffer, ComputeBuffer outputIndexBuffer, ComputeBuffer histogramBuffer, ComputeBuffer countBuffer, int limit, int keyShift)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_InputKeyBufferId, inputKeyBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_InputIndexBufferId, inputIndexBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_OutputKeyBufferId, outputKeyBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_OutputIndexBufferId, outputIndexBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_HistogramBufferId, histogramBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_CountBufferId, countBuffer);
            m_Shader.SetInt(s_LimitId, limit);
            m_Shader.SetInt(s_KeyShiftId, keyShift);
            m_Shader.Dispatch(m_KernelIndex, limit.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
