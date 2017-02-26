using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class BvhFitProgram
    {
        private static readonly int s_ParentIndicesId = Shader.PropertyToID("_parentIndices");
        private static readonly int s_NodeCountersId = Shader.PropertyToID("_nodeCounters");
        private static readonly int s_NodesId = Shader.PropertyToID("_nodes");
        private static readonly int s_InternalNodeCountId = Shader.PropertyToID("_internalNodeCount");

        private int m_KernelIndex;
        private ComputeShader m_Shader;
        private int m_SizeX;

        public BvhFitProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/BvhFit");
            m_KernelIndex = m_Shader.FindKernel("BvhFit");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(StructuredBuffer<int> parentIndices, StructuredBuffer<int> nodeCounters, StructuredBuffer<AlignedBvhNode> nodes)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_ParentIndicesId, parentIndices);
            m_Shader.SetBuffer(m_KernelIndex, s_NodeCountersId, nodeCounters);
            m_Shader.SetBuffer(m_KernelIndex, s_NodesId, nodes);
            m_Shader.SetInt(s_InternalNodeCountId, nodes.count);

            m_Shader.Dispatch(m_KernelIndex, (nodes.count + 1).CeilDiv(m_SizeX), 1, 1);
        }
    }
}
