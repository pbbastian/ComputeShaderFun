using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class RadixCountProgram
    {
        private const string kKeyBuffer = "g_KeyBuffer";
        private const string kCountBuffer = "g_CountBuffer";
        private const string kSectionSize = "g_SectionSize";
        private const string kKeyShift = "g_KeyShift";
        private const string kItemCount = "g_ItemCount";

        private static readonly int s_GroupCou = 1024;
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

        public void Dispatch(CommandBuffer cb, int itemCount, int keyShift, ComputeBuffer keyBuffer, ComputeBuffer countBuffer)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kKeyBuffer, keyBuffer);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kCountBuffer, countBuffer);
            cb.SetComputeIntParam(m_Shader, kSectionSize, itemCount.CeilDiv(m_SizeX * s_GroupCou));
            // Debug.LogFormat("Section size: {0}", itemCount.CeilDiv(m_SizeX * kGroupCou));
            cb.SetComputeIntParam(m_Shader, kKeyShift, keyShift);
            cb.SetComputeIntParam(m_Shader, kItemCount, itemCount);
            cb.DispatchCompute(m_Shader, m_KernelIndex, s_GroupCou, 1, 1);
        }
    }
}
