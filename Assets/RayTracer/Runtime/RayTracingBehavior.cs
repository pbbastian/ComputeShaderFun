using UnityEngine;

namespace RayTracer.Runtime
{
    [RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public class RayTracingBehavior : MonoBehaviour
    {
        private Camera m_Camera;
        private RayTracingContext m_Context;
        private RenderTexture m_RenderTexture;
        private bool m_RayTrace;

        void OnEnable()
        {
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_RayTrace)
            {
                //if (destination == null)
                //    destination = source;
                m_Context.renderTexture = m_RenderTexture;
                if (m_Context.Render(m_Camera))
                    Graphics.Blit(m_RenderTexture, destination);
            }
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                m_Camera = GetComponent<Camera>();
                m_Context = new RayTracingContext();
                m_Context.BuildScene();
                m_RenderTexture = new RenderTexture(m_Camera.pixelWidth, m_Camera.pixelHeight, 8) {enableRandomWrite = true};
                m_RenderTexture.Create();
                m_RayTrace = !m_RayTrace;
                Debug.LogFormat("Ray-tracing is {0}", m_RayTrace ? "on" : "off");
            }
        }
    }
}
