using System;
using UnityEngine;

namespace RayTracer.Runtime.Shaders
{
	public sealed class BasicRayTracerShader
	{
		private ComputeShader m_Shader;
		private int m_TraceKernel;

		private BasicRayTracerShader()
		{
		}

		public Vector2 imageSize
		{
			set { m_Shader.SetVector("g_ImageSize", value); }
		}

		public Vector3 origin
		{
			set { m_Shader.SetVector("g_Origin", value); }
		}

		public Vector3 direction
		{
			set { m_Shader.SetVector("g_Direction", value); }
		}

		public Vector3 light
		{
			set { m_Shader.SetVector("g_Light", value); }
		}

		public float fieldOfView
		{
			set { m_Shader.SetFloat("g_FOV", value); }
		}

		public ComputeBuffer triangleBuffer
		{
			set { m_Shader.SetBuffer(m_TraceKernel, "g_Triangles", value); }
		}

		public RenderTexture result
		{
			set { m_Shader.SetTexture(m_TraceKernel, "g_Result", value); }
		}

		public void DispatchTrace(int totalX, int totalY)
		{
			m_Shader.Dispatch(m_TraceKernel, Mathf.CeilToInt(totalX / 8f), Mathf.CeilToInt(totalY / 8f), 1);
		}

		public static BasicRayTracerShader Create()
		{
			var shader = Resources.Load<ComputeShader>("Shaders/BasicRayTracer");
			if (shader == null)
				throw new Exception("Resource 'Shaders/BasicRayTracer' not found.");
			var traceKernel = shader.FindKernel("Trace");
			if (traceKernel == -1)
				throw new Exception("Kernel 'Trace' not found in shader.");
			return new BasicRayTracerShader
			{
				m_Shader = shader,
				m_TraceKernel = traceKernel
			};
		}
	}
}
