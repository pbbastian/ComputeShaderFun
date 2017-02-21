using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
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

        public void Dispatch(int itemCount, int keyShift, ComputeBuffer keyBuffer, ComputeBuffer countBuffer)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_KeyBufferId, keyBuffer);
            m_Shader.SetBuffer(m_KernelIndex, s_CountBufferId, countBuffer);
            m_Shader.SetInt(s_SectionSizeId, itemCount.CeilDiv(m_SizeX * s_GroupCount));
            // Debug.LogFormat("Section size: {0}", itemCount.CeilDiv(m_SizeX * s_GroupCount));
            m_Shader.SetInt(s_KeyShiftId, keyShift);
            m_Shader.SetInt(s_ItemCountId, itemCount);
            m_Shader.Dispatch(m_KernelIndex, s_GroupCount, 1, 1);
        }
    }
}
