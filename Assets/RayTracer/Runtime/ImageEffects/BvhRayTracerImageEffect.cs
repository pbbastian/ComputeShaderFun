﻿using UnityEngine;

namespace RayTracer.Runtime
{
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class BvhRayTracerImageEffect : MonoBehaviour
    {
        private IRayTracingContext m_Context;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //source.enableRandomWrite = true;
            if (enabled && m_Context != null && m_Context.Render())
                Graphics.Blit(m_Context.renderTexture, destination);
        }

        private void OnDisable()
        {
            m_Context.Dispose();
            m_Context = null;
        }

        private void Update()
        {
            if (enabled && m_Context == null)
            {
                m_Context = new BvhRayTracingContext();
                m_Context.BuildScene();
                m_Context.camera = GetComponent<Camera>();
                m_Context.renderTexture = new RenderTexture(m_Context.camera.pixelWidth, m_Context.camera.pixelHeight, 8) {enableRandomWrite = true};
                m_Context.renderTexture.Create();
            }
        }
    }
}
