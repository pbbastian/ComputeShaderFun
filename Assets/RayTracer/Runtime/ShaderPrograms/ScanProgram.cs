using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class ScanProgram
    {
        private const string kBuffer = "g_Buffer";
        private const string kGroupResultsBuffer = "g_GroupResultsBuffer";
        private const string kLimit = "g_Limit";
        private const string kOffset = "g_Offset";

        private int m_KernelIndex;
        private ComputeShader m_Shader;

        public ScanProgram(WarpSize warpSize)
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/Scan");
            var kernelName = "CSMain_Warp" + (int) warpSize;
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            groupSize = (int) x;
        }

        public int groupSize { get; private set; }

        public int GetGroupCount(int itemCount)
        {
            return itemCount.CeilDiv(groupSize);
        }

        public void Dispatch(CommandBuffer cb, int offset, int limit, ComputeBuffer buffer, ComputeBuffer groupResultsBuffer)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kBuffer, buffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kGroupResultsBuffer, groupResultsBuffer);
            cb.SetComputeIntParam(m_Shader, kLimit, limit);
            cb.SetComputeIntParam(m_Shader, kOffset, offset);
            cb.DispatchCompute(m_Shader, m_KernelIndex, GetGroupCount(limit), 1, 1);
        }
    }
}
