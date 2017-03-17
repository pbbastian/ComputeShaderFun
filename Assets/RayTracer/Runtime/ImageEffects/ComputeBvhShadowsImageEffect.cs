using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ImageEffects
{
    [RequireComponent(typeof(Camera))]
    public class ComputeBvhShadowsImageEffect : MonoBehaviour
    {
        BvhContext m_BvhContext;
        Camera m_Camera;
        CommandBuffer m_Cb;
        ComputeKernel m_Kernel;
        Light m_Light;
        bool m_SceneLoaded;
        BvhShadowsProgram.Variant m_Variant;
        int m_ThreadGroups;
        StructuredBuffer<int> m_WorkCounterBuffer;

        public BvhShadowsProgram.Variant variant;
        public int threadGroups = 100;

        void OnPreRender()
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

        void OnEnable()
        {
            if (!m_SceneLoaded)
                return;

            Cleanup();
            
            m_Light = FindObjectOfType<Light>();
            m_BvhContext = BvhUtil.CreateBvh();
            m_Camera = GetComponent<Camera>();
            m_Camera.depthTextureMode |= DepthTextureMode.Depth;

            CreateCommandBuffer();
        }

        void CreateCommandBuffer()
        {
            CleanupCommandBuffer();

            m_Variant = variant;

            m_ThreadGroups = threadGroups;
            m_Kernel = BvhShadowsProgram.CreateKernel(m_Variant);

            if (m_Variant != BvhShadowsProgram.Variant.Original)
            {
                m_WorkCounterBuffer = new StructuredBuffer<int>(1, ShaderSizes.s_Int);
                m_Kernel.SetBuffer(BvhShadowsProgram.WorkCounter, m_WorkCounterBuffer);
                m_Kernel.SetValue(BvhShadowsProgram.ThreadGroupCount, m_ThreadGroups);
            }

            m_Cb = new CommandBuffer {name = "Compute BVH shadows"};
            m_Cb.GetTemporaryRT(Uniforms.TempId, m_Camera.pixelWidth, m_Camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, true);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.MainTexture, BuiltinRenderTextureType.CameraTarget);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.DepthTexture, BuiltinRenderTextureType.ResolvedDepth);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.NormalTexture, BuiltinRenderTextureType.GBuffer2);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.TargetTexture, Uniforms.TempId);
            var threadGroupsX = m_Variant != BvhShadowsProgram.Variant.Original ? m_ThreadGroups : m_Camera.pixelWidth.CeilDiv(m_Kernel.threadGroupSize.x);
            var threadGroupsY = m_Variant != BvhShadowsProgram.Variant.Original ? 1 : m_Camera.pixelHeight.CeilDiv(m_Kernel.threadGroupSize.y);
            m_Cb.DispatchCompute(m_Kernel, threadGroupsX, threadGroupsY, 1);
            m_Cb.Blit(Uniforms.TempId, BuiltinRenderTextureType.CameraTarget);
            m_Cb.ReleaseTemporaryRT(Uniforms.TempId);

            m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_Cb);
        }

        void OnDisable()
        {
            Cleanup();
        }

        void Cleanup()
        {
            CleanupCommandBuffer();

            if (m_BvhContext != null)
            {
                m_BvhContext.Dispose();
                m_BvhContext = null;
            }

            m_Light = null;
            m_Camera = null;
        }

        void CleanupCommandBuffer()
        {
            if (m_Cb != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_Cb);
                m_Cb.Dispose();
                m_Cb = null;
            }

            m_Kernel = null;

            if (m_WorkCounterBuffer != null)
            {
                m_WorkCounterBuffer.Dispose();
                m_WorkCounterBuffer = null;
            }
        }

        void Update()
        {
            if (!m_SceneLoaded)
            {
                m_SceneLoaded = true;
                OnEnable();
            }
            else if (m_Variant != variant)
            {
                CreateCommandBuffer();
            }
            else if (m_ThreadGroups != threadGroups)
            {
                CreateCommandBuffer();
            }
        }

        static class Uniforms
        {
            public static readonly int TempId = Shader.PropertyToID("_BvhShadowsTemp");
            public static readonly int WorldPositionId = Shader.PropertyToID("_WorldPosition");
        }
    }
}
