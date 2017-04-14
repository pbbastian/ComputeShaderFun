using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class BvhConstructProgram
    {
        const string kKeys = "_keys";
        const string kLeafBounds = "_leafBounds";
        const string kNodes = "_nodes";
        const string kParentIndices = "_parentIndices";
        const string kInternalNodeCount = "_internalNodeCount";

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public BvhConstructProgram()
        {
            m_Shader = Resources.Load<ComputeShader>("Shaders/BvhConstruct");
            m_KernelIndex = m_Shader.FindKernel("BvhConstruct");

            uint x, y, z;
            m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(CommandBuffer cb, StructuredBuffer<int> keys, StructuredBuffer<AlignedAabb> leafBounds, StructuredBuffer<AlignedBvhNode> nodes, StructuredBuffer<int> parentIndices)
        {
            Assert.AreEqual(keys.count, leafBounds.count);
            Assert.AreEqual(keys.count - 1, nodes.count);
            Assert.AreEqual(keys.count * 2 - 2, parentIndices.count);

            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kKeys, keys);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kLeafBounds, leafBounds);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kNodes, nodes);
            cb.SetComputeBufferParam(m_Shader, m_KernelIndex, kParentIndices, parentIndices);
            cb.SetComputeIntParam(m_Shader, kInternalNodeCount, nodes.count);

            cb.DispatchCompute(m_Shader, m_KernelIndex, nodes.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
