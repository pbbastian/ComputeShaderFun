using System;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public sealed class BasicRayTracerProgram
    {
        private static readonly int s_ImageSizeId = Shader.PropertyToID("g_ImageSize");
        private static readonly int s_OriginId = Shader.PropertyToID("g_Origin");
        private static readonly int s_DirectionId = Shader.PropertyToID("g_Direction");
        private static readonly int s_LightId = Shader.PropertyToID("g_Light");
        private static readonly int s_FieldOfViewId = Shader.PropertyToID("g_FOV");
        private static readonly int s_TrianglesId = Shader.PropertyToID("g_TriangleBuffer");
        private static readonly int s_ResultId = Shader.PropertyToID("g_Result");

        private ComputeShader m_Shader;
        private int m_KernelIndex;
        private int m_SizeX;
        private int m_SizeY;

        public BasicRayTracerProgram()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/BasicRayTracer");
            var kernelIndex = shader.FindKernel("Trace");
            m_Shader = shader;
            m_KernelIndex = kernelIndex;
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            m_SizeX = (int) x;
            m_SizeY = (int) y;
        }

        public void DispatchTrace(int totalX, int totalY)
        {
            m_Shader.Dispatch(m_KernelIndex, Mathf.CeilToInt(totalX / 8f), Mathf.CeilToInt(totalY / 8f), 1);
        }

        public void Dispatch(Vector3 origin, Vector3 direction, Vector3 light, float fov, StructuredBuffer<Triangle> triangles, RenderTexture result)
        {
            m_Shader.SetVector(s_ImageSizeId, new Vector2(result.width, result.height));
            m_Shader.SetVector(s_OriginId, origin);
            m_Shader.SetVector(s_DirectionId, direction);
            m_Shader.SetVector(s_LightId, light);
            m_Shader.SetFloat(s_FieldOfViewId, fov);
            m_Shader.SetBuffer(m_KernelIndex, s_TrianglesId, triangles);
            m_Shader.SetTexture(m_KernelIndex, s_ResultId, result);
            m_Shader.Dispatch(m_KernelIndex, result.width.CeilDiv(m_SizeX), result.height.CeilDiv(m_SizeY), 1);
        }
    }
}
