using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public class BvhRayTracerProgram
    {
        public static readonly int s_LightId = Shader.PropertyToID("_light");
        public static readonly int s_InverseCameraMatrixId = Shader.PropertyToID("_inverseCameraMatrix");
        public static readonly int s_OriginId = Shader.PropertyToID("_origin");
        public static readonly int s_NodesId = Shader.PropertyToID("_nodes");
        public static readonly int s_TrianglesId = Shader.PropertyToID("_triangles");
        public static readonly int s_VerticesId = Shader.PropertyToID("_vertices");
        public static readonly int s_ResultId = Shader.PropertyToID("_result");

        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private int m_SizeX;
        private int m_SizeY;

        public BvhRayTracerProgram()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/BasicRayTracer");
            var kernelIndex = shader.FindKernel("Trace");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            m_SizeX = (int)x;
            m_SizeY = (int)y;
        }

        public void Dispatch(Vector3 light, Matrix4x4 inverseCameraMatrix, Vector3 origin, StructuredBuffer<AlignedBvhNode> nodes, StructuredBuffer<IndexedTriangle> triangles, StructuredBuffer<Vector3> vertices, RenderTexture result)
        {
            m_Shader.SetVector(s_LightId, light);
            m_Shader.SetMatrix(s_LightId, inverseCameraMatrix);
            m_Shader.SetVector(s_OriginId, origin);
            m_Shader.SetBuffer(m_KernelIndex, s_NodesId, nodes);
            m_Shader.SetBuffer(m_KernelIndex, s_TrianglesId, triangles);
            m_Shader.SetBuffer(m_KernelIndex, s_VerticesId, vertices);
            m_Shader.SetTexture(m_KernelIndex, s_ResultId, result);
            m_Shader.Dispatch(m_KernelIndex, result.width.CeilDiv(m_SizeX), result.height.CeilDiv(m_SizeY), 1);
        }
    }
}
