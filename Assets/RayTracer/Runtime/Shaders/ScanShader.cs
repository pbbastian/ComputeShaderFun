using System;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.Shaders
{
    public class ScanShader
    {
        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private uint m_SizeX;

        private BufferShaderParameter m_InputBuffer;
        private BufferShaderParameter m_OutputBuffer;

        public ScanShader()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/Scan");
            if (shader == null)
                throw new Exception("Resource 'Shaders/Scan' not found.");
            var kernelIndex = shader.FindKernel("CSMain");
            if (kernelIndex == -1)
                throw new Exception("Kernel 'CSMain' not found in shader.");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out m_SizeX, out y, out z);

            m_InputBuffer = new BufferShaderParameter(shader, kernelIndex, "g_InputBuffer");
            m_OutputBuffer = new BufferShaderParameter(shader, kernelIndex, "g_OutputBuffer");
        }

        public ComputeBuffer inputBuffer { get { return m_InputBuffer.value; } set { m_InputBuffer.value = value; } }
        public ComputeBuffer outputBuffer { get { return m_OutputBuffer.value; } set { m_OutputBuffer.value = value; } }

        public void Dispatch(int itemCount)
        {
            m_Shader.Dispatch(m_KernelIndex, Mathf.CeilToInt((float)itemCount / (m_SizeX)), 1, 1);
        }
    }
}
