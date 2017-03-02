using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;

namespace RayTracer.Runtime
{
    public class BvhRayTracingContext : IRayTracingContext
    {
        private BvhRayTracerProgram m_Shader;
        private BvhContext m_BvhContext;

        public RenderTexture renderTexture { get; set; }
        public Camera camera { get; set; }

        public BvhRayTracingContext()
        {
            m_Shader = new BvhRayTracerProgram();
        }

        public void BuildScene()
        {
            m_BvhContext = BvhUtil.CreateBvh();
        }

        public bool Validate()
        {
            return renderTexture != null
                   //&& renderTexture.IsCreated()
                   //&& renderTexture.enableRandomWrite
                   && m_BvhContext != null
                   && m_Shader != null
                   && camera != null;
        }

        public bool Render()
        {
            if (!Validate())
                return false;

            var light = Object.FindObjectOfType<Light>();
            var scaleMatrix = Matrix4x4.TRS(new Vector3(-1, -1, 0), Quaternion.identity, new Vector3(2f / renderTexture.width, 2f / renderTexture.height, 1));
            var inverseCameraMatrix = (camera.projectionMatrix * camera.worldToCameraMatrix).inverse * scaleMatrix;

            var nodes = m_BvhContext.nodesBuffer.data;
            var triangles = m_BvhContext.trianglesBuffer.data;
            var vertices = m_BvhContext.verticesBuffer.data;

            m_Shader.Dispatch(light.transform.position, inverseCameraMatrix, camera.transform.position, m_BvhContext.nodesBuffer, m_BvhContext.trianglesBuffer, m_BvhContext.verticesBuffer, renderTexture);

            return true;
        }

        public void Dispose()
        {
            if (m_BvhContext != null) m_BvhContext.Dispose();
        }
    }
}
