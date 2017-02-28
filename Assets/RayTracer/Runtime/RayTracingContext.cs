using System;
using RayTracer.Runtime.Components;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RayTracer.Runtime
{
    public sealed class RayTracingContext
    {
        public RayTracingContext()
        {
        }

        private RenderTexture m_RenderTexture;
        private SceneBuilder m_SceneBuilder = new SceneBuilder();
        private ComputeBuffer m_TriangleBuffer;
        private BasicRayTracerProgram m_Shader;

        public RenderTexture renderTexture
        {
            get { return m_RenderTexture; }
            set { m_RenderTexture = value; }
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
                   && m_Shader != null;
        }

        public bool Render(Camera camera)
        {
            if (!Validate())
                return false;

            var light = UnityEngine.Object.FindObjectOfType<Light>();
            var scaleMatrix = Matrix4x4.TRS(new Vector3(-1, -1, 0), Quaternion.identity, new Vector3(2f / renderTexture.width, 2f / renderTexture.height, 1));
            var inverseCameraMatrix = (camera.projectionMatrix * camera.worldToCameraMatrix).inverse * scaleMatrix;
            m_Shader.Dispatch(inverseCameraMatrix, camera.transform.position, light.gameObject.transform.forward, new StructuredBuffer<Triangle>(m_TriangleBuffer), renderTexture);
            return true;
        }
    }
}
