using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class GroupAddProgram
    {
        private const string kPerThreadBuffer = "g_PerThreadBuffer";
        private const string kPerGroupBuffer = "g_PerGroupBuffer";
        private const string kLimit = "g_Limit";
        private const string kOffset = "g_Offset";
        private int m_KernelIndex;
        private ComputeShader m_Shader;

        public GroupAddProgram(WarpSize warpSize)
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/GroupAdd");
            var kernelName = "GroupAdd_Warp" + (int) warpSize;
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

        public void Dispatch(CommandBuffer cb, ComputeBuffer perThreadBuffer, ComputeBuffer perGroupBuffer, int offset, int limit)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kPerThreadBuffer, perThreadBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kPerGroupBuffer, perGroupBuffer);
            cb.SetComputeIntParam(m_Shader, kLimit, limit);
            cb.SetComputeIntParam(m_Shader, kOffset, offset);
            cb.DispatchCompute(m_Shader, m_KernelIndex, GetGroupCount(limit), 1, 1);
        }
    }
}
