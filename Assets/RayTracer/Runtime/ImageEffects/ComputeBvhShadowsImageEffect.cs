using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ImageEffects
{
    [RequireComponent(typeof(Camera))]
    public class ComputeBvhShadowsImageEffect : MonoBehaviour
    {
        private BvhContext m_BvhContext;
        private Camera m_Camera;
        private CommandBuffer m_Cb;
        private ComputeKernel m_Kernel;
        private Light m_Light;
        private bool m_SceneLoaded;
        private const bool kReversedZ = true;

        private void OnPreRender()
        {
            if (m_Kernel != null)
            {
                m_Kernel.SetValue(BvhShadowsProgram.ZBufferParams, m_Camera.GetZBufferParams(true));
                m_Kernel.SetValue(BvhShadowsProgram.Light, m_Light.transform.forward);
                m_Kernel.SetValue(BvhShadowsProgram.InverseView, m_Camera.cameraToWorldMatrix);
                m_Kernel.SetValue(BvhShadowsProgram.Projection, m_Camera.projectionMatrix);
                m_Kernel.SetValue(BvhShadowsProgram.Size, new Vector2(m_Camera.pixelWidth, m_Camera.pixelHeight));
                m_Kernel.SetBuffer(BvhShadowsProgram.NodeBuffer, m_BvhContext.nodesBuffer);
                m_Kernel.SetBuffer(BvhShadowsProgram.TriangleBuffer, m_BvhContext.trianglesBuffer);
                m_Kernel.SetBuffer(BvhShadowsProgram.VertexBuffer, m_BvhContext.verticesBuffer);
            }
        }

        private void OnEnable()
        {
            if (!m_SceneLoaded)
                return;

            Cleanup();

            m_Light = FindObjectOfType<Light>();
            m_BvhContext = BvhUtil.CreateBvh();
            m_Camera = GetComponent<Camera>();
            m_Camera.depthTextureMode |= DepthTextureMode.Depth;

            m_Kernel = BvhShadowsProgram.CreateKernel();

            m_Cb = new CommandBuffer {name = "Compute BVH shadows"};
            m_Cb.GetTemporaryRT(Uniforms.TempId, m_Camera.pixelWidth, m_Camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, true);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.MainTexture, BuiltinRenderTextureType.CameraTarget);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.DepthTexture, BuiltinRenderTextureType.ResolvedDepth);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.NormalTexture, BuiltinRenderTextureType.GBuffer2);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.TargetTexture, Uniforms.TempId);
            m_Cb.DispatchCompute(m_Kernel, m_Camera.pixelWidth.CeilDiv(m_Kernel.threadGroupSize.x), m_Camera.pixelHeight.CeilDiv(m_Kernel.threadGroupSize.y), 1);
            m_Cb.Blit(Uniforms.TempId, BuiltinRenderTextureType.CameraTarget);
            m_Cb.ReleaseTemporaryRT(Uniforms.TempId);

            m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_Cb);
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (m_Cb != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_Cb);
                m_Cb.Dispose();
                m_Cb = null;
            }

            if (m_BvhContext != null)
            {
                m_BvhContext.Dispose();
                m_BvhContext = null;
            }

            m_Light = null;
            m_Camera = null;
            m_Kernel = null;
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
            public static readonly int WorldPositionId = Shader.PropertyToID("_WorldPosition");
        }
    }
}
