using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixHistogramProgram
    {
        const string kKeyBuffer = "g_KeyBuffer";
        const string kHistogramBuffer = "g_HistogramBuffer";
        const string kItemCount = "g_ItemCount";
        const string kKeyShift = "g_KeyShift";

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public RadixHistogramProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/RadixHistogram");
            var kernelName = "RadixHistogram";
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(CommandBuffer cb, ComputeBuffer keyBuffer, ComputeBuffer histogramBuffer, int itemCount, int keyShift)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kKeyBuffer, keyBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kHistogramBuffer, histogramBuffer);
            cb.SetComputeIntParam(m_Shader, kItemCount, itemCount);
            cb.SetComputeIntParam(m_Shader, kKeyShift, keyShift);
            cb.DispatchCompute(m_Shader, m_KernelIndex, itemCount.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
