using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct GroupAddData
    {
        public ComputeBuffer perThreadBuffer;
        public ComputeBuffer perGroupBuffer;
        public int offset;
        public int limit;

        public GroupAddData(ComputeBuffer perThreadBuffer, ComputeBuffer perGroupBuffer, int offset, int limit)
        {
            this.perThreadBuffer = perThreadBuffer;
            this.perGroupBuffer = perGroupBuffer;
            this.offset = offset;
            this.limit = limit;
        }
    }

    public class GroupAddProgram
    {
        private static readonly int s_PerThreadBufferId = Shader.PropertyToID("g_PerThreadBuffer");
        private static readonly int s_PerGroupBufferId = Shader.PropertyToID("g_PerGroupBuffer");
        private static readonly int s_LimitId = Shader.PropertyToID("g_Limit");
        private static readonly int s_OffsetId = Shader.PropertyToID("g_Offset");
        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public GroupAddProgram(WarpSize warpSize)
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/GroupAdd");
            var kernelName = "GroupAdd_Warp" + (int) warpSize;
            m_KernelIndex = m_Shader.FindKernel(kernelName);
            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public int GroupSize { get { return m_SizeX; } }

        public int GetGroupCount(int itemCount)
        {
            return itemCount.CeilDiv(m_SizeX);
        }

        public void Dispatch(GroupAddData data)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_PerThreadBufferId, data.perThreadBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_PerGroupBufferId, data.perGroupBuffer);
            m_Shader.SetInt(s_LimitId, data.limit);
            m_Shader.SetInt(s_OffsetId, data.offset);
            m_Shader.Dispatch(m_KernelIndex, GetGroupCount(data.limit), 1, 1);
        }
    }
}
