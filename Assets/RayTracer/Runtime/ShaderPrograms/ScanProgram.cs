using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct ScanData
    {
        public int itemCount;
        public ComputeBuffer buffer;
        public ComputeBuffer groupResultsBuffer;
    }

    public class ScanProgram
    {
        private static readonly int s_BufferId = Shader.PropertyToID("g_Buffer");
        private static readonly int s_GroupResultsBufferId = Shader.PropertyToID("g_GroupResultsBuffer");
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

        public void Dispatch(ScanData data)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_BufferId, data.buffer);
            m_Shader.SetBuffer(m_KernelIndex, s_GroupResultsBufferId, data.groupResultsBuffer);
            m_Shader.Dispatch(m_KernelIndex, GetGroupCount(data.itemCount), 1, 1);
        }
    }
}
