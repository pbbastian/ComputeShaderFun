using System;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.Shaders
{
    public class TransformShader
    {
        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private uint m_SizeX;

        public TransformShader()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/Transform");
            if (shader == null)
                throw new Exception("Resource 'Shaders/Transform' not found.");
            var kernelIndex = shader.FindKernel("CSMain");
            if (kernelIndex == -1)
                throw new Exception("Kernel 'CSMain' not found in shader.");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out m_SizeX, out y, out z);

            m_VertexBuffer = new BufferShaderParameter(shader, kernelIndex, "g_VertexBuffer");
            m_NormalBuffer = new BufferShaderParameter(shader, kernelIndex, "g_NormalBuffer");
            m_ObjectIndexBuffer = new BufferShaderParameter(shader, kernelIndex, "g_ObjectIndexBuffer");
            m_TransformBuffer = new BufferShaderParameter(shader, kernelIndex, "g_TransformBuffer");
        }

        public ComputeBuffer vertexBuffer { get { return m_VertexBuffer.value; } set { m_VertexBuffer.value = value; } }
        private BufferShaderParameter m_VertexBuffer;

        public ComputeBuffer normalBuffer { get { return m_NormalBuffer.value; } set { m_NormalBuffer.value = value; } }
        private BufferShaderParameter m_NormalBuffer;

        public ComputeBuffer objectIndexBuffer { get { return m_ObjectIndexBuffer.value; } set { m_ObjectIndexBuffer.value = value; } }
        private BufferShaderParameter m_ObjectIndexBuffer;

        public ComputeBuffer transformBuffer { get { return m_TransformBuffer.value; } set { m_TransformBuffer.value = value; } }
        private BufferShaderParameter m_TransformBuffer;

        public void Dispatch(int vertexCount)
        {
            m_Shader.Dispatch(m_KernelIndex, Mathf.CeilToInt((float)vertexCount / m_SizeX), 1, 1);
        }
    }
}
