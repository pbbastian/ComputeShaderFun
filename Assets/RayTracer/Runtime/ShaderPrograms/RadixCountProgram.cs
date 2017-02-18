using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public struct RadixCountData
    {
        public int itemCount;
        public int keyShift;
        public ComputeBuffer keyBuffer;
        public ComputeBuffer countBuffer;

        public RadixCountData(int itemCount, int keyShift, ComputeBuffer keyBuffer, ComputeBuffer countBuffer)
        {
            this.itemCount = itemCount;
            this.keyShift = keyShift;
            this.keyBuffer = keyBuffer;
            this.countBuffer = countBuffer;
        }
    }

    public class RadixCountProgram
    {
        private static readonly int s_KeyBufferId = Shader.PropertyToID("g_KeyBuffer");
        private static readonly int s_CountBufferId = Shader.PropertyToID("g_CountBuffer");
        private static readonly int s_SectionSizeId = Shader.PropertyToID("g_SectionSize");
        private static readonly int s_KeyShiftId = Shader.PropertyToID("g_KeyShift");
        private static readonly int s_ItemCountId = Shader.PropertyToID("g_ItemCount");

        private static readonly int s_GroupCount = 1024;
        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public RadixCountProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/RadixCount");
            var kernelName = "RadixCount";
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(RadixCountData data)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_KeyBufferId, data.keyBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_CountBufferId, data.countBuffer);
            m_Shader.SetInt(s_SectionSizeId, data.itemCount.CeilDiv(m_SizeX * s_GroupCount));
            // Debug.LogFormat("Section size: {0}", data.itemCount.CeilDiv(m_SizeX * s_GroupCount));
            m_Shader.SetInt(s_KeyShiftId, data.keyShift);
            m_Shader.SetInt(s_ItemCountId, data.itemCount);
            m_Shader.Dispatch(m_KernelIndex, s_GroupCount, 1, 1);
        }
    }
}
