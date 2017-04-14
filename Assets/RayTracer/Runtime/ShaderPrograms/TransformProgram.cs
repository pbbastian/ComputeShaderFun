using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class TransformProgram
    {
        static readonly int s_VerticesId = Shader.PropertyToID("_vertices");
        //private static readonly int s_NormalsId = Shader.PropertyToID("_normals");
        static readonly int s_ObjectIndicesId = Shader.PropertyToID("_objectIndices");
        static readonly int s_TransformsId = Shader.PropertyToID("_transforms");

        int m_KernelIndex;
        ComputeShader m_Shader;
        int m_SizeX;

        public TransformProgram()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/Transform");
            var kernelIndex = shader.FindKernel("Transform");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
        }

        public void Dispatch(StructuredBuffer<Vector4> vertices /*, StructuredBuffer<Vector3> normals*/, StructuredBuffer<uint> objectIndices, StructuredBuffer<Matrix4x4> transforms)
        {
            m_Shader.SetBuffer(m_KernelIndex, s_VerticesId, vertices);
            //m_Shader.SetBuffer(m_KernelIndex, s_NormalsId, normals);
            m_Shader.SetBuffer(m_KernelIndex, s_ObjectIndicesId, objectIndices);
            m_Shader.SetBuffer(m_KernelIndex, s_TransformsId, transforms);
            m_Shader.Dispatch(m_KernelIndex, vertices.count.CeilDiv(m_SizeX), 1, 1);
        }
    }
}
