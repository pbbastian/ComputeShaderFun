using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class LeafReorderProgram
    {
        private static readonly int s_IndicesId = Shader.PropertyToID("_indices");
        private static readonly int s_BoundsInputId = Shader.PropertyToID("_boundsInput");
        private static readonly int s_BoundsOutputId = Shader.PropertyToID("_boundsOutput");
        private static readonly int s_TrianglesInputId = Shader.PropertyToID("_trianglesInput");
        private static readonly int s_TrianglesOutputId = Shader.PropertyToID("_trianglesOutput");
        private static readonly int s_LimitId = Shader.PropertyToID("_limit");

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public LeafReorderProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/LeafReorder");
            m_KernelIndex = m_Shader.FindKernel("LeafReorder");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
        }

        public void Dispatch(StructuredBuffer<int> indices, StructuredBuffer<AlignedAabb> boundsInput, StructuredBuffer<AlignedAabb> boundsOutput, StructuredBuffer<IndexedTriangle> trianglesInput, StructuredBuffer<IndexedTriangle> trianglesOutput)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_IndicesId, indices);
            m_Shader.SetBuffer(m_KernelIndex, s_BoundsInputId, boundsInput);
            m_Shader.SetBuffer(m_KernelIndex, s_BoundsOutputId, boundsOutput);
            m_Shader.SetBuffer(m_KernelIndex, s_TrianglesInputId, trianglesInput);
            m_Shader.SetBuffer(m_KernelIndex, s_TrianglesOutputId, trianglesOutput);
            m_Shader.SetInt(s_LimitId, indices.count);
            m_Shader.Dispatch(m_KernelIndex, indices.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
