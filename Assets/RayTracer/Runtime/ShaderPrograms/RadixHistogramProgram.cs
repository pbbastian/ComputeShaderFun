using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixHistogramProgram
    {
        private static readonly int s_KeyBufferId = Shader.PropertyToID("g_KeyBuffer");
        private static readonly int s_HistogramBufferId = Shader.PropertyToID("g_HistogramBuffer");
        private static readonly int s_ItemCountId = Shader.PropertyToID("g_ItemCount");
        private static readonly int s_KeyShiftId = Shader.PropertyToID("g_KeyShift");

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public RadixHistogramProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/RadixHistogram");
            var kernelName = "RadixHistogram";
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
        }

        public void Dispatch(ComputeBuffer keyBuffer, ComputeBuffer histogramBuffer, int itemCount, int keyShift)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_KeyBufferId, keyBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_HistogramBufferId, histogramBuffer);
            m_Shader.SetInt(s_ItemCountId, itemCount);
            m_Shader.SetInt(s_KeyShiftId, keyShift);
            m_Shader.Dispatch(m_KernelIndex, itemCount.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
