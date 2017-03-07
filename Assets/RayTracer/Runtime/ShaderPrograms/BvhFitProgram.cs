using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class BvhFitProgram
    {
        private const string kParentIndices = "_parentIndices";
        private const string kNodeCounters = "_nodeCounters";
        private const string kNodes = "_nodes";
        private const string kInternalNodeCount = "_internalNodeCount";

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

        public void Dispatch(CommandBuffer cb, StructuredBuffer<int> parentIndices, StructuredBuffer<int> nodeCounters, StructuredBuffer<AlignedBvhNode> nodes)
        {
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kParentIndices, parentIndices);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kNodeCounters, nodeCounters);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kNodes, nodes);
            cb.SetComputeIntParam(m_Shader, kInternalNodeCount, nodes.count);

            cb.DispatchCompute(m_Shader, m_KernelIndex, (nodes.count + 1).CeilDiv(m_SizeX), 1, 1);
        }
    }
}
