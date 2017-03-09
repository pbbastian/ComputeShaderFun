using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ImageEffects
{
    public class BvhShadowsImageEffect : MonoBehaviour
    {
        private BvhContext m_BvhContext;
        private Camera m_Camera;
        private CommandBuffer m_Cb;
        private Light m_Light;

        private Material m_Material;
        private bool m_SceneLoaded;

        private void OnPreRender()
        {
            if (m_Material != null && m_Light != null && m_Camera != null)
            {
                m_Material.SetVector("_light", m_Light.transform.forward);
                m_Material.SetMatrix("_projection", m_Camera.nonJitteredProjectionMatrix);
                m_Material.SetMatrix("_InverseView", m_Camera.cameraToWorldMatrix);
                m_Material.SetBuffer("_nodes", m_BvhContext.nodesBuffer);
                m_Material.SetBuffer("_triangles", m_BvhContext.trianglesBuffer);
                m_Material.SetBuffer("_vertices", m_BvhContext.verticesBuffer);
            }
        }

        private void OnEnable()
        {
            if (!m_SceneLoaded)
                return;

            Cleanup();

            m_Light = FindObjectOfType<Light>();
            m_Camera = GetComponent<Camera>();
            m_BvhContext = BvhUtil.CreateBvh();

            m_Material = new Material(Shader.Find("Hidden/BvhShadows"));

            m_Cb = new CommandBuffer {name = "BVH shadows"};
            m_Cb.GetTemporaryRT(Uniforms.TempId, -1, -1, 0, FilterMode.Bilinear);
            m_Cb.Blit(BuiltinRenderTextureType.CameraTarget, Uniforms.TempId, m_Material);
            m_Cb.Blit(Uniforms.TempId, BuiltinRenderTextureType.CameraTarget);
            m_Cb.ReleaseTemporaryRT(Uniforms.TempId);

            m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_Cb);
        }

        private void Cleanup()
        {
            if (m_Cb != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_Cb);
                m_Cb.Dispose();
                m_Cb = null;
            }

            if (m_Material != null)
            {
                DestroyImmediate(m_Material);
                m_Material = null;
            }

            if (m_BvhContext != null)
            {
                m_BvhContext.Dispose();
                m_BvhContext = null;
            }

            m_Light = null;
            m_Camera = null;
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void Update()
        {
            if (!m_SceneLoaded)
            {
                m_SceneLoaded = true;
                OnEnable();
            }
        }

        private static class Uniforms
        {
            public static readonly int TempId = Shader.PropertyToID("_BvhShadowsTemp");
        }
    }
}
