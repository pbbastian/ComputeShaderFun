using System;
using UnityEngine;

namespace RayTracer.Runtime
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public class BasicRayTracerBehavior : MonoBehaviour
    {
        private IRayTracingContext m_Context;
        private Material m_Material;

        void Awake()
        {
            m_Material = new Material(Shader.Find("Hidden/RayTracerIE"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (enabled && m_Context != null && m_Context.Render())
                Graphics.Blit(m_Context.renderTexture, destination);
        }

        void OnDisable()
        {
            m_Context.Dispose();
            m_Context = null;
        }

        void Update()
        {
            if (enabled && m_Context == null)
            {
                m_Context = new RayTracingContext();
                m_Context.BuildScene();
                m_Context.camera = GetComponent<Camera>();
                m_Context.renderTexture = new RenderTexture(m_Context.camera.pixelWidth, m_Context.camera.pixelHeight, 8) { enableRandomWrite = true };
                m_Context.renderTexture.Create();
            }
        }
    }
}
