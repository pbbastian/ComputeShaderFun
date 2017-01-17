using System;
using Assets.RayTracer.Editor.Util;
using RayTracer.Editor.Util;
using RayTracer.Runtime;
using UnityEditor;
using UnityEngine;

namespace RayTracer.Editor.UI
{
	public abstract class AbstractRayTracerEditorWindow : EditorWindow
	{
		private RayTracingProfileAsset m_Asset;
		private Texture2D m_BlackTexture;
		private RayTracingContext m_Context;
		private RenderTexture m_RenderTexture;
		private Rect m_RemainingRect;

		public RayTracingProfileAsset asset
		{
			get { return m_Asset; }
			set
			{
				if (value != null && m_Asset != null && value.name == m_Asset.name)
					return;
				var previousAsset = m_Asset;
				m_Asset = value;
				AssetDidChange(previousAsset);
			}
		}

		public void OnEnable()
		{
			titleContent.text = "Ray tracer";

			m_BlackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			m_BlackTexture.SetPixel(1, 1, Color.black);
			m_BlackTexture.Apply();
		}

		public void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			var profileLabel = new GUIContent("Active profile");
			asset = EditorGUILayout.ObjectField(profileLabel, asset, typeof(RayTracingProfileAsset), false) as RayTracingProfileAsset;
			GUI.enabled = asset != null;
			if (GUILayout.Button("Render"))
				Render();
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			var remainingRect = RayGUILayout.GetRemainingRect();
			if (Math.Abs(remainingRect.width - 1) > 1e-3 && Math.Abs(remainingRect.height - 1) > 1e-3)
				m_RemainingRect = remainingRect;
			if (asset == null)
				return;
			if (m_RenderTexture != null)
			{
				var renderRect = RayGUI.GetCenteredRect(m_RemainingRect, new Vector2(m_RenderTexture.width, m_RenderTexture.height));
				if (m_RenderTexture != null)
					GUI.DrawTexture(renderRect, m_RenderTexture);
			}

			var previewRect = RayGUI.GetCenteredRect(m_RemainingRect, new Vector2(asset.profile.renderWidth, asset.profile.renderHeight));
			var lines = new[]
			{
				// Top left
				new Vector3(previewRect.xMin - 1, previewRect.yMin - 1, 0f),
				new Vector3(previewRect.xMin + 9, previewRect.yMin - 1, 0f),
				new Vector3(previewRect.xMin - 1, previewRect.yMin - 1, 0f),
				new Vector3(previewRect.xMin - 1, previewRect.yMin + 9, 0f),
				// Top right
				new Vector3(previewRect.xMax + 1, previewRect.yMin - 1, 0f),
				new Vector3(previewRect.xMax - 9, previewRect.yMin - 1, 0f),
				new Vector3(previewRect.xMax + 1, previewRect.yMin - 1, 0f),
				new Vector3(previewRect.xMax + 1, previewRect.yMin + 9, 0f),
				// Bottom left
				new Vector3(previewRect.xMin - 1, previewRect.yMax + 1, 0f),
				new Vector3(previewRect.xMin + 9, previewRect.yMax + 1, 0f),
				new Vector3(previewRect.xMin - 1, previewRect.yMax + 1, 0f),
				new Vector3(previewRect.xMin - 1, previewRect.yMax - 9, 0f),
				// Bottom right
				new Vector3(previewRect.xMax + 1, previewRect.yMax + 1, 0f),
				new Vector3(previewRect.xMax - 9, previewRect.yMax + 1, 0f),
				new Vector3(previewRect.xMax + 1, previewRect.yMax + 1, 0f),
				new Vector3(previewRect.xMax + 1, previewRect.yMax - 9, 0f),
			};
			Handles.DrawLines(lines);
		}

		private void AssetDidChange(RayTracingProfileAsset previousAsset)
		{
			Repaint();
		}

		private void Render()
		{
			m_RenderTexture = new RenderTexture((int) m_RemainingRect.width, (int) m_RemainingRect.height, 24) {enableRandomWrite = true};
			m_RenderTexture.Create();
			m_Context = RayTracingContext.Create(m_RenderTexture);
			m_Context.Render();
			Repaint();
		}
	}
}
