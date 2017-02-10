using UnityEngine;

namespace RayTracer.Runtime.Shaders
{
    public struct RadixCountData
    {
        public int itemCount;
        public int keyMask;
        public ComputeBuffer keyBuffer;
        public ComputeBuffer countBuffer;
    }

    public class RadixCountShader
    {
        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private int m_SizeX;

        public RadixCountShader()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/Scan");
            var kernelName = "RadixCount";
            m_KernelIndex = m_Shader.FindKernel(kernelName);

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
        }

        public void Dispatch(RadixCountData data)
        {
            m_Shader.Dispatch(m_KernelIndex, 6144, 1, 1);
        }
    }
}
