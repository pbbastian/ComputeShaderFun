using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class LeafInitProgram
    {
        const string kTriangles = "_triangles";
        const string kVertices = "_vertices";
        const string kLeafBounds = "_leafBounds";
        const string kLeafKeys = "_leafKeys";
        const string kSceneBounds = "_sceneBounds";

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public LeafInitProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/LeafInit");
            m_KernelIndex = m_Shader.FindKernel("LeafInit");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(CommandBuffer cb, Aabb sceneBounds, StructuredBuffer<IndexedTriangle> triangles, StructuredBuffer<Vector4> vertices, StructuredBuffer<AlignedAabb> leafBounds, StructuredBuffer<int> leafKeys)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kTriangles, triangles);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kVertices, vertices);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kLeafBounds, leafBounds);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kLeafKeys, leafKeys);
            cb.SetComputeFloatParams(m_Shader, kSceneBounds, sceneBounds.min.x, sceneBounds.min.y, sceneBounds.min.z, sceneBounds.max.x, sceneBounds.max.y, sceneBounds.max.z);
            cb.DispatchCompute(m_Shader, m_KernelIndex, triangles.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
