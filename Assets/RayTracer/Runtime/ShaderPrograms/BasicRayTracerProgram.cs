using System;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms
{
    public sealed class BasicRayTracerProgram
    {
        private ComputeShader m_Shader;
        private int m_TraceKernel;

        public BasicRayTracerProgram()
        {
            var shader = Resources.Load<ComputeShader>("Shaders/BasicRayTracer");
            if (shader == null)
                throw new Exception("Resource 'Shaders/BasicRayTracer' not found.");
            var traceKernel = shader.FindKernel("Trace");
            if (traceKernel == -1)
                throw new Exception("Kernel 'Trace' not found in shader.");
            m_Shader = shader;
            m_TraceKernel = traceKernel;

            imageSize = new Vector2ShaderParameter(m_Shader, "g_ImageSize");
            origin = new Vector3ShaderParameter(m_Shader, "g_Origin");
            direction = new Vector3ShaderParameter(m_Shader, "g_Direction");
            light = new Vector3ShaderParameter(m_Shader, "g_Light");
            fieldOfView = new FloatShaderParameter(m_Shader, "g_FOV");
            triangleBuffer = new BufferShaderParameter(m_Shader, traceKernel, "g_TriangleBuffer");
            result = new TextureShaderParameter<RenderTexture>(m_Shader, traceKernel, "g_Result");
        }

        public IShaderParameter<Vector2> imageSize { get; private set; }

        public IShaderParameter<Vector3> origin { get; private set; }

        public IShaderParameter<Vector3> direction { get; private set; }

        public IShaderParameter<Vector3> light { get; private set; }

        public IShaderParameter<float> fieldOfView { get; private set; }

        public IShaderParameter<ComputeBuffer> triangleBuffer { get; private set; }

        public IShaderParameter<RenderTexture> result { get; private set; }

        public void DispatchTrace(int totalX, int totalY)
        {
            m_Shader.Dispatch(m_TraceKernel, Mathf.CeilToInt(totalX / 8f), Mathf.CeilToInt(totalY / 8f), 1);
        }
    }
}
