﻿using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace ShadowRenderPipeline
{
    public class ShadowRenderPipeline : IRenderPipeline
    {
        readonly int m_CameraColorBuffer;
        readonly int m_CameraDepthStencilBuffer;
        readonly int[] m_GBuffer = new int[3];
        
        RenderTargetIdentifier m_CameraColorBufferRT;
        RenderTargetIdentifier m_CameraDepthStencilBufferRT;
        RenderTargetIdentifier[] m_GBufferRT = new RenderTargetIdentifier[3];

        Material m_DeferredLighting;

        public ShadowRenderPipeline()
        {
            m_CameraColorBuffer = Shader.PropertyToID("_CameraColorTexture");
            m_CameraDepthStencilBuffer = Shader.PropertyToID("_CameraDepthStencilBuffer");

            m_CameraColorBufferRT = new RenderTargetIdentifier(m_CameraColorBuffer);
            m_CameraDepthStencilBufferRT = new RenderTargetIdentifier(m_CameraDepthStencilBuffer);
            for (var i = 0; i < m_GBuffer.Length; i++)
            {
                m_GBuffer[i] = Shader.PropertyToID("_GBufferTexture" + i);
                m_GBufferRT[i] = new RenderTargetIdentifier(m_GBuffer[i]);
            }

            m_DeferredLighting = new Material(Shader.Find("Hidden/DeferredLighting"));
        }

        public void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            if (disposed)
                throw new ObjectDisposedException(string.Format("{0} has been disposed. Do not call Render on disposed RenderLoops.", this));

            foreach (var camera in cameras)
            {
                // Culling
                CullingParameters cullingParams;
                if (!CullResults.GetCullingParameters(camera, out cullingParams))
                    continue;
                CullResults cull = CullResults.Cull(ref cullingParams, context);

                // Setup camera for rendering (sets render target, view/projection matrices and other
                // per-camera built-in shader variables).
                context.SetupCameraProperties(camera);

                // clear depth buffer
                using (var cmd = new CommandBuffer() {name = "Init buffers"})
                {
                    cmd.GetTemporaryRT(m_CameraColorBuffer, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
                    cmd.GetTemporaryRT(m_CameraDepthStencilBuffer, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.Depth);
                    cmd.GetTemporaryRT(m_GBuffer[0], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1, true);
                    cmd.GetTemporaryRT(m_GBuffer[1], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, true);
                    cmd.GetTemporaryRT(m_GBuffer[2], camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear, 1, true);
                    cmd.SetRenderTarget(m_GBufferRT, m_CameraDepthStencilBufferRT);
                    cmd.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(cmd);
                }

                // Setup global lighting shader variables 
                SetupLightShaderVariables(cull.visibleLights, context);

                // Draw opaque objects using BasicPass shader pass
                var settings = new DrawRendererSettings(cull, camera, new ShaderPassName("BasicPass"));
                settings.sorting.flags = SortFlags.CommonOpaque;
                settings.inputFilter.SetQueuesOpaque();
                context.DrawRenderers(ref settings);

                // Draw skybox
                context.DrawSkybox(camera);

                // Draw transparent objects using BasicPass shader pass
                settings.sorting.flags = SortFlags.CommonTransparent;
                settings.inputFilter.SetQueuesTransparent();
                context.DrawRenderers(ref settings);

                using (var cmd = new CommandBuffer() {name = "Blit"})
                {
                    cmd.SetGlobalTexture(m_GBuffer[1], m_GBufferRT[1]);
                    cmd.Blit(m_GBuffer[0], BuiltinRenderTextureType.CameraTarget, m_DeferredLighting);
                    cmd.ReleaseTemporaryRT(m_CameraColorBuffer);
                    cmd.ReleaseTemporaryRT(m_CameraDepthStencilBuffer);
                    cmd.ReleaseTemporaryRT(m_GBuffer[0]);
                    cmd.ReleaseTemporaryRT(m_GBuffer[1]);
                    cmd.ReleaseTemporaryRT(m_GBuffer[2]);
                    context.ExecuteCommandBuffer(cmd);
                }

                context.Submit();
            }
        }

        // Setup lighting variables for shader to use
        private static void SetupLightShaderVariables(VisibleLight[] lights, ScriptableRenderContext context)
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
            CommandBuffer cmd = new CommandBuffer {name = "Setup light shader variables" };
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

        private static void GetShaderConstantsFromNormalizedSH(ref SphericalHarmonicsL2 ambientProbe, Vector4[] outCoefficients)
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
            disposed = true;
        }
    }
}
