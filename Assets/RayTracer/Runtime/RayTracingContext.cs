using System;
using RayTracer.Runtime.Components;
using RayTracer.Runtime.Shaders;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RayTracer.Runtime
{
	public sealed class RayTracingContext
	{
		public RayTracingContext()
		{
		}

		private RenderTexture m_RenderTexture;
		private SceneBuilder m_SceneBuilder = new SceneBuilder();
		private ComputeBuffer m_TriangleBuffer;

		public RenderTexture renderTexture
		{
			get { return m_RenderTexture; }
			set { m_RenderTexture = value; }
		}

		public void BuildScene()
		{
			m_SceneBuilder.Add(SceneManager.GetActiveScene());
			if (m_TriangleBuffer != null)
				m_TriangleBuffer.Release();
			m_TriangleBuffer = m_SceneBuilder.BuildTriangleBuffer();
		}

		public bool Validate()
		{
			return renderTexture != null
					&& renderTexture.IsCreated()
					&& renderTexture.enableRandomWrite
					&& m_TriangleBuffer != null;
		}

		public void Render()
		{
			if (!Validate())
				return;

			var light = UnityEngine.Object.FindObjectOfType<Light>();
			var rtCamera = UnityEngine.Object.FindObjectOfType<RayTracingCamera>();
			var camera = rtCamera.gameObject.GetComponent<Camera>();

			var shader = new BasicRayTracerShader();
			shader.imageSize.value = new Vector2(renderTexture.width, renderTexture.height);
			shader.origin.value = rtCamera.gameObject.transform.position;
			shader.direction.value = camera.gameObject.transform.forward;
			shader.light.value = light.gameObject.transform.forward;
			shader.fieldOfView.value = Mathf.Deg2Rad * camera.fieldOfView;
			shader.triangleBuffer.value = m_TriangleBuffer;
			shader.result.value = renderTexture;
			shader.DispatchTrace(renderTexture.width, renderTexture.height);
		}
	}
}
