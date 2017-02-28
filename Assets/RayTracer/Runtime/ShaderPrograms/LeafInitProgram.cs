using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class LeafInitProgram
    {
        private static readonly int s_TrianglesId = Shader.PropertyToID("_triangles");
        private static readonly int s_VerticesId = Shader.PropertyToID("_vertices");
        private static readonly int s_LeafBoundsId = Shader.PropertyToID("_leafBounds");
        private static readonly int s_LeafKeysId = Shader.PropertyToID("_leafKeys");
        private static readonly int s_SceneBoundsId = Shader.PropertyToID("_sceneBounds");

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

        public void Dispatch(Aabb sceneBounds, StructuredBuffer<IndexedTriangle> triangles, StructuredBuffer<Vector4> vertices, StructuredBuffer<AlignedAabb> leafBounds, StructuredBuffer<int> leafKeys)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_TrianglesId, triangles);
            m_Shader.SetBuffer(m_KernelIndex, s_VerticesId, vertices);
            m_Shader.SetBuffer(m_KernelIndex, s_LeafBoundsId, leafBounds);
            m_Shader.SetBuffer(m_KernelIndex, s_LeafKeysId, leafKeys);
            m_Shader.SetFloats(s_SceneBoundsId, sceneBounds.min.x, sceneBounds.min.y, sceneBounds.min.z, sceneBounds.max.x, sceneBounds.max.y, sceneBounds.max.z);
            m_Shader.Dispatch(m_KernelIndex, triangles.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
