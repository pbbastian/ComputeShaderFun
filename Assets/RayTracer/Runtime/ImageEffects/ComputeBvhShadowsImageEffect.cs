using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Runtime.ImageEffects
{
    public class ComputeBvhShadowsImageEffect : MonoBehaviour
    {
        private BvhContext m_BvhContext;
        private Camera m_Camera;
        private CommandBuffer m_Cb;
        private ComputeKernel m_Kernel;
        private Light m_Light;
        private bool m_SceneLoaded;

        private void OnPreRender()
        {
            m_Kernel.SetValue(BvhShadowsProgram.Light, m_Light.transform.forward);
        }

        private void OnEnable()
        {
            if (!m_SceneLoaded)
                return;

            m_Light = FindObjectOfType<Light>();
            m_Camera = GetComponent<Camera>();
            m_BvhContext = BvhUtil.CreateBvh();

            m_Kernel = BvhShadowsProgram.CreateKernel();

            m_Cb = new CommandBuffer {name = "BVH shadows"};
            m_Cb.GetTemporaryRT(Uniforms.TempId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, true);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.MainTex, BuiltinRenderTextureType.CameraTarget);
            m_Cb.SetTexture(m_Kernel, BvhShadowsProgram.TargetTex, Uniforms.TempId);
            m_Cb.Blit(Uniforms.TempId, BuiltinRenderTextureType.CameraTarget);
            m_Cb.ReleaseTemporaryRT(Uniforms.TempId);
        }

        private static class Uniforms
        {
            public static readonly int TempId = Shader.PropertyToID("_BvhShadowsTemp");
        }
    }
}
