using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class LeafInitProgram
    {
        private const string kTriangles = "_triangles";
        private const string kVertices = "_vertices";
        private const string kLeafBounds = "_leafBounds";
        private const string kLeafKeys = "_leafKeys";
        private const string kSceneBounds = "_sceneBounds";

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public LeafInitProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/LeafInit");
            m_KernelIndex = m_Shader.FindKernel("LeafInit");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
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
