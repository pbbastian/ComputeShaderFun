using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct ScanData
    {
        public int offset;
        public int limit;
        public ComputeBuffer buffer;
        public ComputeBuffer groupResultsBuffer;

        public ScanData(int offset, int limit, ComputeBuffer buffer, ComputeBuffer groupResultsBuffer)
        {
            this.offset = offset;
            this.limit = limit;
            this.buffer = buffer;
            this.groupResultsBuffer = groupResultsBuffer;
        }
    }

    public class ScanProgram
    {
        private static readonly int s_BufferId = Shader.PropertyToID("g_Buffer");
        private static readonly int s_GroupResultsBufferId = Shader.PropertyToID("g_GroupResultsBuffer");
        private static readonly int s_LimitId = Shader.PropertyToID("g_Limit");
        private static readonly int s_OffsetId = Shader.PropertyToID("g_Offset");
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
            m_Shader.SetInt(s_LimitId, data.limit);
            m_Shader.SetInt(s_OffsetId, data.offset);
            m_Shader.Dispatch(m_KernelIndex, GetGroupCount(data.limit), 1, 1);
        }
    }
}
