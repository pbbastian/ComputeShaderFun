using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.Shaders
{
    public struct GroupAddData
    {
        public int itemCount;
        public ComputeBuffer perThreadBuffer;
        public ComputeBuffer perGroupBuffer;
    }

    public class GroupAddShader
    {
        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private int m_SizeX;

        private static readonly int s_PerThreadBufferId = Shader.PropertyToID("g_PerThreadBuffer");
        private static readonly int s_PerGroupBufferId = Shader.PropertyToID("g_PerGroupBuffer");

        public GroupAddShader(WarpSize warpSize)
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/GroupAdd");
            var kernelName = "GroupAdd_Warp" + (int) warpSize;
            m_KernelIndex = m_Shader.FindKernel(kernelName);
            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(GroupAddData data)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_PerThreadBufferId, data.perThreadBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_PerGroupBufferId, data.perGroupBuffer);
            m_Shader.Dispatch(m_KernelIndex, data.itemCount.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
