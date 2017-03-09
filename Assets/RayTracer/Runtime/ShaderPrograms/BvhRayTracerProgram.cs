using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class BvhRayTracerProgram
    {
        public static readonly int LightId = Shader.PropertyToID("_light");
        public static readonly int InverseCameraMatrixId = Shader.PropertyToID("_inverseCameraMatrix");
        public static readonly int OriginId = Shader.PropertyToID("_origin");
        public static readonly int NodesId = Shader.PropertyToID("_nodes");
        public static readonly int TrianglesId = Shader.PropertyToID("_triangles");
        public static readonly int VerticesId = Shader.PropertyToID("_vertices");
        public static readonly int ResultId = Shader.PropertyToID("_result");
        private int m_KernelIndex;

        private ComputeShader m_Shader;
        private int m_SizeX;
        private int m_SizeY;

        public BvhRayTracerProgram()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/BvhRayTracer");
            var kernelIndex = shader.FindKernel("BvhRayTracer");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
            m_SizeY = (int) y;
        }

        public void Dispatch(Vector3 light, Matrix4x4 inverseCameraMatrix, Vector3 origin, StructuredBuffer<AlignedBvhNode> nodes, StructuredBuffer<IndexedTriangle> triangles, StructuredBuffer<Vector4> vertices, RenderTexture result)
        {
            m_Shader.SetVector(LightId, light);
            m_Shader.SetMatrix(InverseCameraMatrixId, inverseCameraMatrix);
            m_Shader.SetVector(OriginId, origin);
            m_Shader.SetBuffer(m_KernelIndex, NodesId, nodes);
            m_Shader.SetBuffer(m_KernelIndex, TrianglesId, triangles);
            m_Shader.SetBuffer(m_KernelIndex, VerticesId, vertices);
            m_Shader.SetTexture(m_KernelIndex, ResultId, result);
            m_Shader.Dispatch(m_KernelIndex, result.width.CeilDiv(m_SizeX), result.height.CeilDiv(m_SizeY), 1);
        }
    }
}
