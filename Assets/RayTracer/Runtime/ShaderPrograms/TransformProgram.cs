using System;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class TransformProgram
    {
        private static readonly int s_VerticesId = Shader.PropertyToID("g_VertexBuffer");
        private static readonly int s_NormalsId = Shader.PropertyToID("g_NormalBuffer");
        private static readonly int s_ObjectIndicesId = Shader.PropertyToID("g_ObjectIndexBuffer");
        private static readonly int s_TransformsId = Shader.PropertyToID("g_TransformBuffer");

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public TransformProgram()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/Transform");
            if (shader == null)
                throw new Exception("Resource 'Shaders/Transform' not found.");
            var kernelIndex = shader.FindKernel("CSMain");
            if (kernelIndex == -1)
                throw new Exception("Kernel 'CSMain' not found in shader.");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(StructuredBuffer<Vector3> vertices, StructuredBuffer<Vector3> normals, StructuredBuffer<uint> objectIndices, StructuredBuffer<Matrix4x4> transforms)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_VerticesId, vertices);
            m_Shader.SetBuffer(m_KernelIndex, s_NormalsId, normals);
            m_Shader.SetBuffer(m_KernelIndex, s_ObjectIndicesId, objectIndices);
            m_Shader.SetBuffer(m_KernelIndex, s_TransformsId, transforms);
            m_Shader.Dispatch(m_KernelIndex, vertices.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
