using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct RadixReorderData
    {
        public ComputeBuffer inputBuffer;
        public ComputeBuffer outputBuffer;
        public ComputeBuffer histogramBuffer;
        public ComputeBuffer countBuffer;
        public int limit;
        public int keyShift;

        public RadixReorderData(ComputeBuffer inputBuffer, ComputeBuffer outputBuffer, ComputeBuffer histogramBuffer, ComputeBuffer countBuffer, int limit, int keyShift)
        {
            this.inputBuffer = inputBuffer;
            this.outputBuffer = outputBuffer;
            this.histogramBuffer = histogramBuffer;
            this.countBuffer = countBuffer;
            this.limit = limit;
            this.keyShift = keyShift;
        }
    }

    public class RadixReorderProgram
    {
        private static readonly int s_InputBufferId = Shader.PropertyToID("g_InputBuffer");
        private static readonly int s_OutputBufferId = Shader.PropertyToID("g_OutputBuffer");
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

        public void Dispatch(RadixReorderData data)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_InputBufferId, data.inputBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_OutputBufferId, data.outputBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_HistogramBufferId, data.histogramBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_CountBufferId, data.countBuffer);
            m_Shader.SetInt(s_LimitId, data.limit);
            m_Shader.SetInt(s_KeyShiftId, data.keyShift);
            m_Shader.Dispatch(m_KernelIndex, data.limit.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
