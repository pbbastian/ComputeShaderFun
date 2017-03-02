using UnityEngine;

namespace RayTracer.Runtime.ImageEffects
{
    public class BvhShadowsImageEffect : MonoBehaviour
    {
        private Material m_Material;
        private BvhContext m_BvhContext;

        void Awake()
        {
            m_Material = new Material(Shader.Find("Hidden/BvhShadows"));
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_BvhContext == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            var matrix = GetComponent<Camera>().cameraToWorldMatrix;
            m_Material.SetMatrix("_InverseView", matrix);
            m_Material.SetBuffer("_nodes", m_BvhContext.nodesBuffer);
            m_Material.SetBuffer("_triangles", m_BvhContext.trianglesBuffer);
            m_Material.SetBuffer("_vertices", m_BvhContext.verticesBuffer);
            m_Material.SetVector("_light", FindObjectOfType<Light>().transform.forward);
            Graphics.Blit(source, destination, m_Material);
        }

        void OnDisable()
        {
            m_BvhContext.Dispose();
            m_BvhContext = null;
        }

        void Update()
        {
            if (enabled && m_BvhContext == null)
            {
                Debug.Log(FindObjectOfType<Light>().transform.forward);
                m_BvhContext = BvhUtil.CreateBvh();
            }
        }
    }
}
