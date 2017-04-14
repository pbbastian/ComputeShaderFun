using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class GroupAddProgram
    {
        const string kPerThreadBuffer = "g_PerThreadBuffer";
        const string kPerGroupBuffer = "g_PerGroupBuffer";
        const string kLimit = "g_Limit";
        const string kOffset = "g_Offset";
        int m_KernelIndex;
        ComputeShader m_Shader;

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
