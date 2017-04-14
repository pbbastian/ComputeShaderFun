using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class LeafReorderProgram
    {
        const string kIndices = "_indices";
        const string kBoundsInput = "_boundsInput";
        const string kBoundsOutput = "_boundsOutput";
        const string kTrianglesInput = "_trianglesInput";
        const string kTrianglesOutput = "_trianglesOutput";
        const string kLimit = "_limit";

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public LeafReorderProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/LeafReorder");
            m_KernelIndex = m_Shader.FindKernel("LeafReorder");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(CommandBuffer cb, StructuredBuffer<int> indices, StructuredBuffer<AlignedAabb> boundsInput, StructuredBuffer<AlignedAabb> boundsOutput, StructuredBuffer<IndexedTriangle> trianglesInput, StructuredBuffer<IndexedTriangle> trianglesOutput)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kIndices, indices);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kBoundsInput, boundsInput);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kBoundsOutput, boundsOutput);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kTrianglesInput, trianglesInput);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kTrianglesOutput, trianglesOutput);
            cb.SetComputeIntParam(m_Shader, kLimit, indices.count);
            cb.DispatchCompute(m_Shader, m_KernelIndex, indices.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
