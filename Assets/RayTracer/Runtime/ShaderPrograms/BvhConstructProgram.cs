using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class BvhConstructProgram
    {
        private static readonly int s_KeysId = Shader.PropertyToID("_keys");
        private static readonly int s_LeafBoundsId = Shader.PropertyToID("_leafBounds");
        private static readonly int s_NodesId = Shader.PropertyToID("_nodes");
        private static readonly int s_ParentIndicesId = Shader.PropertyToID("_parentIndices");
        private static readonly int s_InternalNodeCountId = Shader.PropertyToID("_internalNodeCount");

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public BvhConstructProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/BvhConstruct");
            m_KernelIndex = m_Shader.FindKernel("BvhConstruct");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(StructuredBuffer<int> keys, StructuredBuffer<AlignedAabb> leafBounds, StructuredBuffer<AlignedBvhNode> nodes, StructuredBuffer<int> parentIndices)
        {
            Assert.AreEqual(keys.count, leafBounds.count);
            Assert.AreEqual(keys.count - 1, nodes.count);
            Assert.AreEqual(keys.count * 2 - 2, parentIndices.count);

            m_Shader.SetBuffer(m_KernelIndex, s_KeysId, keys);
            m_Shader.SetBuffer(m_KernelIndex, s_LeafBoundsId, leafBounds);
            m_Shader.SetBuffer(m_KernelIndex, s_NodesId, nodes);
            m_Shader.SetBuffer(m_KernelIndex, s_ParentIndicesId, parentIndices);
            m_Shader.SetInt(s_InternalNodeCountId, nodes.count);

            m_Shader.Dispatch(m_KernelIndex, nodes.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
