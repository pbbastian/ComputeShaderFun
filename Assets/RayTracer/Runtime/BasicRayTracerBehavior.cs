using System;
using UnityEngine;

namespace RayTracer.Runtime
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public class BasicRayTracerBehavior : MonoBehaviour
    {
        private Camera m_Camera;
        private IRayTracingContext m_Context;
        private RenderTexture m_RenderTexture;
        private bool m_RayTrace;
        private Material m_Material;

        void Awake()
        {
            m_Material = new Material(Shader.Find("Hidden/RayTracerIE"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_RayTrace)
            {
                //if (destination == null)
                //    destination = source;
                m_Context.renderTexture = m_RenderTexture;
                m_Context.camera = m_Camera;
                //if (m_Context.Render())
                //    Graphics.Blit(m_RenderTexture, destination);
                Graphics.Blit(source, destination, m_Material);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.T))
            {
                m_Camera = GetComponent<Camera>();
                if (Input.GetKeyDown(KeyCode.R))
                    m_Context = new RayTracingContext();
                else
                    m_Context = new BvhRayTracingContext();
                m_Context.BuildScene();
                m_RenderTexture = new RenderTexture(m_Camera.pixelWidth, m_Camera.pixelHeight, 8) {enableRandomWrite = true};
                m_RenderTexture.Create();
                m_RayTrace = !m_RayTrace;
            }
        }
    }
}
