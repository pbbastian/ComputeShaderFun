using System;
using RayTracer.Runtime.Components;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RayTracer.Runtime
{
    public sealed class RayTracingContext : IRayTracingContext
    {
        public RayTracingContext()
        {
        }

        private RenderTexture m_RenderTexture;
        private SceneBuilder m_SceneBuilder = new SceneBuilder();
        private ComputeBuffer m_TriangleBuffer;
        private BasicRayTracerProgram m_Shader;
        private Camera m_Camera;

        public RenderTexture renderTexture
        {
            get { return m_RenderTexture; }
            set { m_RenderTexture = value; }
        }

        public Camera camera
        {
            get { return m_Camera; }
            set { m_Camera = value; }
        }

        public void BuildScene()
        {
            m_SceneBuilder.Add(SceneManager.GetActiveScene());
            if (m_TriangleBuffer != null)
                m_TriangleBuffer.Release();
            m_TriangleBuffer = m_SceneBuilder.BuildTriangleBuffer();
            m_Shader = new BasicRayTracerProgram();
        }

        public bool Validate()
        {
            return renderTexture != null
                   && renderTexture.IsCreated()
                   && renderTexture.enableRandomWrite
                   && m_TriangleBuffer != null
                   && m_Shader != null
                   && m_Camera != null;
        }

        public bool Render()
        {
            if (!Validate())
                return false;

            var light = UnityEngine.Object.FindObjectOfType<Light>();
            var scaleMatrix = Matrix4x4.TRS(new Vector3(-1, -1, 0), Quaternion.identity, new Vector3(2f / renderTexture.width, 2f / renderTexture.height, 1));
            var inverseCameraMatrix = (camera.projectionMatrix * camera.worldToCameraMatrix).inverse * scaleMatrix;
            m_Shader.Dispatch(inverseCameraMatrix, camera.transform.position, light.gameObject.transform.forward, new StructuredBuffer<Triangle>(m_TriangleBuffer), renderTexture);
            return true;
        }

        public void Dispose()
        {
            if (m_TriangleBuffer != null) m_TriangleBuffer.Dispose();
        }
    }
}
