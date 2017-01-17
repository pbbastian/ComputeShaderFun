using System;
using RayTracer.Runtime.Components;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime
{
    public sealed class RayTracingContext
    {
        private RayTracingContext()
        {
        }

        public RenderTexture renderTexture { get; private set; }

        public static RayTracingContext Create(RenderTexture renderTexture)
        {
            if (!renderTexture.IsCreated())
                throw new ArgumentException("Render texture must be created", "renderTexture");
            if (!renderTexture.enableRandomWrite)
                throw new ArgumentException("enableRandomWrite must be true", "renderTexture");
            var context = new RayTracingContext {renderTexture = renderTexture};
            return context;
        }

        public void Render()
        {
	        var light = UnityEngine.Object.FindObjectOfType<Light>();
            var rtCamera = UnityEngine.Object.FindObjectOfType<RayTracingCamera>();
            var camera = rtCamera.gameObject.GetComponent<Camera>();
            var shader = Resources.Load<ComputeShader>("Shaders/BasicRayTracer");
            //shader.SetMatrix("g_CameraToWorldMatrix", camera.cameraToWorldMatrix);
            shader.SetMatrix("g_CameraToWorldMatrix", rtCamera.gameObject.transform.localToWorldMatrix);
            shader.SetVector("g_ImageSize", new Vector4(renderTexture.width, renderTexture.height));
            shader.SetVector("g_Origin", rtCamera.gameObject.transform.position);
			Debug.Log(Mathf.Deg2Rad * camera.fieldOfView); 
			shader.SetVector("g_Direction", camera.gameObject.transform.forward);
			//Debug.Log(camera.);
			
			shader.SetVector("g_Light", light.gameObject.transform.forward);
            shader.SetFloat("g_FOV", Mathf.Deg2Rad * camera.fieldOfView);
            var kernel = shader.FindKernel("Trace");
            shader.SetTexture(kernel, "g_Result", renderTexture);
            shader.Dispatch(kernel, Mathf.CeilToInt(renderTexture.width / 8f), Mathf.CeilToInt(renderTexture.height / 8f), 1);
        }
    }
}
