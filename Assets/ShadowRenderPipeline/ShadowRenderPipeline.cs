using System;
using RayTracer.Runtime;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ShadowRenderPipeline
{
    public class ShadowRenderPipeline : IRenderPipeline
    {
        ShadowRenderPipelineAsset m_Asset;
        DateTime m_BvhBuildDateTime;
        BvhContext m_BvhContext;

        int m_CameraColorID;
        int m_CameraDepthStencilID;
        int m_ShadowmapID;
        int[] m_GBufferID = new int[4];

        RenderTargetIdentifier m_CameraColorRT;
        RenderTargetIdentifier m_CameraDepthStencilRT;
        RenderTargetIdentifier m_ShadowmapRT;
        RenderTargetIdentifier[] m_GBufferRT = new RenderTargetIdentifier[4];

        Material m_DeferredLightingMat;
        Material m_FxaaMaterial;
        ComputeShader m_ShadowsCompute;
        StructuredBuffer<int> m_WorkCounterBuffer;

        public ShadowRenderPipeline(ShadowRenderPipelineAsset asset)
        {
            m_Asset = asset;

            m_CameraColorID = Shader.PropertyToID("_CameraColorTexture");
            m_CameraDepthStencilID = Shader.PropertyToID("_CameraDepthTexture");
            m_ShadowmapID = Shader.PropertyToID("_ShadowmapTexture");

            m_CameraColorRT = new RenderTargetIdentifier(m_CameraColorID);
            m_CameraDepthStencilRT = new RenderTargetIdentifier(m_CameraDepthStencilID);
            m_ShadowmapRT = new RenderTargetIdentifier(m_ShadowmapID);

            for (var i = 0; i < m_GBufferID.Length; i++)
            {
                m_GBufferID[i] = Shader.PropertyToID("_CameraGBufferTexture" + i);
                m_GBufferRT[i] = new RenderTargetIdentifier(m_GBufferID[i]);
            }

            m_DeferredLightingMat = CheckShaderAndCreateMaterial(Shader.Find("Hidden/DeferredLighting"));
            m_FxaaMaterial = CheckShaderAndCreateMaterial(Shader.Find("Hidden/Fast Approximate Anti-aliasing"));
            m_ShadowsCompute = Resources.Load<ComputeShader>(ShadowsCompute.Path);
            m_WorkCounterBuffer = new StructuredBuffer<int>(1, ShaderSizes.s_Int);

            m_BvhBuildDateTime = DateTime.MinValue;

//            UpdateBvhContext(true);
        }

        public static Material CheckShaderAndCreateMaterial(Shader s)
        {
            if (s == null || !s.isSupported)
                return null;

            var material = new Material(s);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        public void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            if (disposed)
                throw new ObjectDisposedException(string.Format("{0} has been disposed. Do not call Render on disposed RenderLoops.", this));

            UpdateBvhContext();

            foreach (var camera in cameras)
            {
                // Culling
                CullingParameters cullingParams;
                if (!CullResults.GetCullingParameters(camera, out cullingParams))
                    continue;
                cullingParams.shadowDistance = 100f;
                CullResults cullResults = CullResults.Cull(ref cullingParams, context);

                var shadowsRendered = DrawShadows(ref context, ref cullResults);

                // Setup camera for rendering (sets render target, view/projection matrices and other
                // per-camera built-in shader variables).
                context.SetupCameraProperties(camera);

                // clear depth buffer
                using (var cmd = new CommandBuffer { name = "Init G-Buffer" })
                {
                    cmd.SetGlobalMatrix("_InverseView", camera.cameraToWorldMatrix);
                    cmd.GetTemporaryRT(m_CameraColorID, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
                    cmd.GetTemporaryRT(m_CameraDepthStencilID, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.Depth);
                    cmd.GetTemporaryRT(m_GBufferID[0], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, true);
                    cmd.GetTemporaryRT(m_GBufferID[1], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, true);
                    cmd.GetTemporaryRT(m_GBufferID[2], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear, 1, true);
                    cmd.GetTemporaryRT(m_GBufferID[3], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, true);
                    cmd.SetRenderTarget(m_GBufferRT, m_CameraDepthStencilRT);
                    cmd.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(cmd);
                }

                // Setup global lighting shader variables
                SetupLightShaderVariables(cullResults.visibleLights, context);

                // Draw opaque objects using BasicPass shader pass
                var settings = new DrawRendererSettings(cullResults, camera, new ShaderPassName("BasicPass"));
                settings.sorting.flags = SortFlags.CommonOpaque;
                settings.inputFilter.SetQueuesOpaque();
                context.DrawRenderers(ref settings);

                if (m_Asset.shadowsEnabled && shadowsRendered)
                {
                    var shadowsKernel = m_ShadowsCompute.FindKernel(ShadowsCompute.Kernels.Shadows);
                    using (var cmd = new CommandBuffer { name = "Compute BVH shadows" })
                    {
                        cmd.SetRenderTarget(m_CameraColorRT, m_CameraDepthStencilRT);
                        cmd.SetComputeBufferParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.WorkCounter, m_WorkCounterBuffer);
                        cmd.SetComputeIntParam(m_ShadowsCompute, ShadowsCompute.ThreadGroupCount, 512);
                        cmd.SetComputeVectorParam(m_ShadowsCompute, ShadowsCompute.ZBufferParams, camera.GetZBufferParams(true));
                        cmd.SetComputeVectorParam(m_ShadowsCompute, ShadowsCompute.Light, cullResults.visibleLights[0].localToWorld.GetColumn(3));
                        cmd.SetComputeMatrix4x4Param(m_ShadowsCompute, ShadowsCompute.InverseView, camera.cameraToWorldMatrix);
                        cmd.SetComputeMatrix4x4Param(m_ShadowsCompute, ShadowsCompute.Projection, camera.projectionMatrix);
                        cmd.SetComputeVectorParam(m_ShadowsCompute, ShadowsCompute.Size, new Vector2(camera.pixelWidth, camera.pixelHeight));
                        cmd.SetComputeBufferParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.NodeBuffer, m_BvhContext.nodesBuffer);
                        cmd.SetComputeBufferParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.TriangleBuffer, m_BvhContext.trianglesBuffer);
                        cmd.SetComputeBufferParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.VertexBuffer, m_BvhContext.verticesBuffer);
                        cmd.SetComputeTextureParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.DepthTexture, m_CameraDepthStencilRT);
                        cmd.SetComputeTextureParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.NormalTexture, m_GBufferRT[2]);
                        cmd.SetComputeTextureParam(m_ShadowsCompute, shadowsKernel, ShadowsCompute.TargetTexture, m_GBufferRT[3]);
                        cmd.DispatchCompute(m_ShadowsCompute, shadowsKernel, 512, 1, 1);
                        context.ExecuteCommandBuffer(cmd);
                    }
                }

                using (var cmd = new CommandBuffer { name = "Deferred Lighting" })
                {
                    if (!m_Asset.shadowsEnabled)
                    {
                        cmd.SetRenderTarget(m_GBufferRT[3]);
                        cmd.ClearRenderTarget(false, true, Color.white);
                    }
                    cmd.Blit(m_GBufferRT[0], m_CameraColorRT, m_DeferredLightingMat);
                    cmd.SetRenderTarget(m_CameraColorRT, m_CameraDepthStencilRT);
                    context.ExecuteCommandBuffer(cmd);
                }

                // Draw skybox
                context.DrawSkybox(camera);

                // Draw transparent objects using BasicPass shader pass
                settings.sorting.flags = SortFlags.CommonTransparent;
                settings.inputFilter.SetQueuesTransparent();
                context.DrawRenderers(ref settings);

                using (var cmd = new CommandBuffer { name = "Post-processing" +  (m_Asset.antiAliasingSettings.enabled ? " (FXAA)" : "")})
                {
                    var source = m_CameraColorRT;
                    if (m_Asset.debugSettings.enabled)
                    {
                        var outputBuffer = m_Asset.debugSettings.outputBuffer;
                        if (outputBuffer == OutputBuffer.Depth)
                            source = m_CameraDepthStencilRT;
                        if (outputBuffer == OutputBuffer.GBuffer0)
                            source = m_GBufferRT[0];
                        if (outputBuffer == OutputBuffer.GBuffer1)
                            source = m_GBufferRT[1];
                        if (outputBuffer == OutputBuffer.GBuffer2)
                            source = m_GBufferRT[2];
                        if (outputBuffer == OutputBuffer.GBuffer3)
                            source = m_GBufferRT[3];
                        if (outputBuffer == OutputBuffer.Shadowmap)
                            source = m_ShadowmapRT;
                    }

                    if (m_Asset.antiAliasingSettings.enabled && (!m_Asset.debugSettings.enabled || m_Asset.debugSettings.outputBuffer != OutputBuffer.Shadowmap))
                    {
                        var preset = m_Asset.antiAliasingSettings.preset;

                        cmd.SetGlobalVector(Fxaa.Uniforms.qualitySettings, new Vector3(preset.qualitySettings.subpixelAliasingRemovalAmount,
                            preset.qualitySettings.edgeDetectionThreshold, preset.qualitySettings.minimumRequiredLuminance));

                        cmd.SetGlobalVector(Fxaa.Uniforms.consoleSettings, new Vector4(preset.consoleSettings.subpixelSpreadAmount,
                            preset.consoleSettings.edgeSharpnessAmount, preset.consoleSettings.edgeDetectionThreshold,
                            preset.consoleSettings.minimumRequiredLuminance));

                        cmd.Blit(source, BuiltinRenderTextureType.CameraTarget, m_FxaaMaterial, 0);
                    }
                    else
                    {
                        cmd.Blit(source, BuiltinRenderTextureType.CameraTarget);
                    }
                    context.ExecuteCommandBuffer(cmd);
                }

                using (var cmd = new CommandBuffer { name = "Release buffers" })
                {
                    cmd.ReleaseTemporaryRT(m_GBufferID[0]);
                    cmd.ReleaseTemporaryRT(m_GBufferID[1]);
                    cmd.ReleaseTemporaryRT(m_GBufferID[2]);
                    cmd.ReleaseTemporaryRT(m_GBufferID[3]);
                    cmd.ReleaseTemporaryRT(m_CameraColorID);
                    cmd.ReleaseTemporaryRT(m_CameraDepthStencilID);
                    if (shadowsRendered)
                        cmd.ReleaseTemporaryRT(m_ShadowmapID);
                    context.ExecuteCommandBuffer(cmd);
                }

                context.Submit();
            }
        }

        void UpdateBvhContext(bool force = false)
        {
//            Debug.LogFormat("Trying to deserialize BVH... ({2}) {0} < {1}", m_BvhBuildDateTime, m_Asset.bvhBuildDateTime, m_Asset.bvhContext.isValid ? "valid" : "invalid");
            if ((force || m_BvhBuildDateTime < m_Asset.bvhBuildDateTime) && m_Asset.bvhContext.isValid)
            {
//                Debug.Log("Deserializing BVH...");
                if (m_BvhContext != null)
                    m_BvhContext.Dispose();

                m_BvhContext = m_Asset.bvhContext.Deserialize();
                m_BvhBuildDateTime = m_Asset.bvhBuildDateTime;
            }
        }

        bool DrawShadows(ref ScriptableRenderContext context, ref CullResults cullResults)
        {
            if (cullResults.visibleLights.Length == 0)
                return false;
            Matrix4x4 view, proj;
            var settings = new DrawShadowsSettings(cullResults, 0);
            var needsRendering = cullResults.ComputeSpotShadowMatricesAndCullingPrimitives(0, out view, out proj, out settings.splitData);

            if (!needsRendering)
                return false;

            using (var cmd = new CommandBuffer { name = "Prepare shadowmap" })
            {
                cmd.GetTemporaryRT(m_ShadowmapID, 512, 512, 24, FilterMode.Bilinear, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
                cmd.SetRenderTarget(m_ShadowmapRT);
                cmd.ClearRenderTarget(true, true, Color.black);
                cmd.SetViewProjectionMatrices(view, proj);
                context.ExecuteCommandBuffer(cmd);
            }

            context.DrawShadows(ref settings);

            return true;
        }

        // Setup lighting variables for shader to use
        static void SetupLightShaderVariables(VisibleLight[] lights, ScriptableRenderContext context)
        {
            // We only support up to 8 visible lights here. More complex approaches would
            // be doing some sort of per-object light setups, but here we go for simplest possible
            // approach.
            const int kMaxLights = 8;

            // Just take first 8 lights. Possible improvements: sort lights by intensity or distance
            // to the viewer, so that "most important" lights in the scene are picked, and not the 8
            // that happened to be first.
            int lightCount = Mathf.Min(lights.Length, kMaxLights);

            // Prepare light data
            Vector4[] lightColors = new Vector4[kMaxLights];
            Vector4[] lightPositions = new Vector4[kMaxLights];
            Vector4[] lightSpotDirections = new Vector4[kMaxLights];
            Vector4[] lightAtten = new Vector4[kMaxLights];
            for (var i = 0; i < lightCount; ++i)
            {
                VisibleLight light = lights[i];
                lightColors[i] = light.finalColor;
                if (light.lightType == LightType.Directional)
                {
                    // light position for directional lights is: (-direction, 0)
                    var dir = light.localToWorld.GetColumn(2);
                    lightPositions[i] = new Vector4(-dir.x, -dir.y, -dir.z, 0);
                }
                else
                {
                    // light position for point/spot lights is: (position, 1)
                    var pos = light.localToWorld.GetColumn(3);
                    lightPositions[i] = new Vector4(pos.x, pos.y, pos.z, 1);
                }

                // attenuation set in a way where distance attenuation can be computed:
                //  float lengthSq = dot(toLight, toLight);
                //  float atten = 1.0 / (1.0 + lengthSq * LightAtten[i].z);
                // and spot cone attenuation:
                //  float rho = max (0, dot(normalize(toLight), SpotDirection[i].xyz));
                //  float spotAtt = (rho - LightAtten[i].x) * LightAtten[i].y;
                //  spotAtt = saturate(spotAtt);
                // and the above works for all light types, i.e. spot light code works out
                // to correct math for point & directional lights as well.

                float rangeSq = light.range * light.range;

                float quadAtten = (light.lightType == LightType.Directional) ? 0.0f : 25.0f / rangeSq;

                // spot direction & attenuation
                if (light.lightType == LightType.Spot)
                {
                    var dir = light.localToWorld.GetColumn(2);
                    lightSpotDirections[i] = new Vector4(-dir.x, -dir.y, -dir.z, 0);

                    float radAngle = Mathf.Deg2Rad * light.spotAngle;
                    float cosTheta = Mathf.Cos(radAngle * 0.25f);
                    float cosPhi = Mathf.Cos(radAngle * 0.5f);
                    float cosDiff = cosTheta - cosPhi;
                    lightAtten[i] = new Vector4(cosPhi, (cosDiff != 0.0f) ? 1.0f / cosDiff : 1.0f, quadAtten, rangeSq);
                }
                else
                {
                    // non-spot light
                    lightSpotDirections[i] = new Vector4(0, 0, 1, 0);
                    lightAtten[i] = new Vector4(-1, 1, quadAtten, rangeSq);
                }
            }

            // ambient lighting spherical harmonics values
            const int kSHCoefficients = 7;
            Vector4[] shConstants = new Vector4[kSHCoefficients];
            SphericalHarmonicsL2 ambientSH = RenderSettings.ambientProbe * RenderSettings.ambientIntensity;
            GetShaderConstantsFromNormalizedSH(ref ambientSH, shConstants);

            // setup global shader variables to contain all the data computed above
            CommandBuffer cmd = new CommandBuffer { name = "Setup light shader variables" };
            cmd.SetGlobalVectorArray("globalLightColor", lightColors);
            cmd.SetGlobalVectorArray("globalLightPos", lightPositions);
            cmd.SetGlobalVectorArray("globalLightSpotDir", lightSpotDirections);
            cmd.SetGlobalVectorArray("globalLightAtten", lightAtten);
            cmd.SetGlobalVector("globalLightCount", new Vector4(lightCount, 0, 0, 0));
            cmd.SetGlobalVectorArray("globalSH", shConstants);
            context.ExecuteCommandBuffer(cmd);
            cmd.Dispose();
        }

        // Prepare L2 spherical harmonics values for efficient evaluation in a shader

        static void GetShaderConstantsFromNormalizedSH(ref SphericalHarmonicsL2 ambientProbe, Vector4[] outCoefficients)
        {
            for (int channelIdx = 0; channelIdx < 3; ++channelIdx)
            {
                // Constant + Linear
                // In the shader we multiply the normal is not swizzled, so it's normal.xyz.
                // Swizzle the coefficients to be in { x, y, z, DC } order.
                outCoefficients[channelIdx].x = ambientProbe[channelIdx, 3];
                outCoefficients[channelIdx].y = ambientProbe[channelIdx, 1];
                outCoefficients[channelIdx].z = ambientProbe[channelIdx, 2];
                outCoefficients[channelIdx].w = ambientProbe[channelIdx, 0] - ambientProbe[channelIdx, 6];

                // Quadratic polynomials
                outCoefficients[channelIdx + 3].x = ambientProbe[channelIdx, 4];
                outCoefficients[channelIdx + 3].y = ambientProbe[channelIdx, 5];
                outCoefficients[channelIdx + 3].z = ambientProbe[channelIdx, 6] * 3.0f;
                outCoefficients[channelIdx + 3].w = ambientProbe[channelIdx, 7];
            }

            // Final quadratic polynomial
            outCoefficients[6].x = ambientProbe[0, 8];
            outCoefficients[6].y = ambientProbe[1, 8];
            outCoefficients[6].z = ambientProbe[2, 8];
            outCoefficients[6].w = 1.0f;
        }

        public bool disposed { get; private set; }

        public void Dispose()
        {
            if (m_BvhContext != null)
                m_BvhContext.Dispose();
            m_BvhBuildDateTime = DateTime.MinValue;
            if (m_WorkCounterBuffer != null)
                m_WorkCounterBuffer.Dispose();
            disposed = true;
        }
    }
}
