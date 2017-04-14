using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixReorderProgram
    {
        const string kInputKeyBuffer = "g_InputKeyBuffer";
        const string kInputIndexBuffer = "g_InputIndexBuffer";
        const string kOutputKeyBuffer = "g_OutputKeyBuffer";
        const string kOutputIndexBuffer = "g_OutputIndexBuffer";
        const string kHistogramBuffer = "g_HistogramBuffer";
        const string kCountBuffer = "g_CountBuffer";
        const string kLimit = "g_Limit";
        const string kKeyShift = "g_KeyShift";

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public RadixReorderProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/RadixReorder");
            var kernelName = "RadixReorder";
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(CommandBuffer cb, ComputeBuffer inputKeyBuffer, ComputeBuffer outputKeyBuffer, ComputeBuffer inputIndexBuffer, ComputeBuffer outputIndexBuffer, ComputeBuffer histogramBuffer, ComputeBuffer countBuffer, int limit, int keyShift)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kInputKeyBuffer, inputKeyBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kInputIndexBuffer, inputIndexBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kOutputKeyBuffer, outputKeyBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kOutputIndexBuffer, outputIndexBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kHistogramBuffer, histogramBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kCountBuffer, countBuffer);
            cb.SetComputeIntParam(m_Shader, kLimit, limit);
            cb.SetComputeIntParam(m_Shader, kKeyShift, keyShift);
            cb.DispatchCompute(m_Shader, m_KernelIndex, limit.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
