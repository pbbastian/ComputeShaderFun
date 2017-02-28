using System;
using System.Diagnostics;
using System.Linq;
using Assets.RayTracer.Editor.Util;
using RayTracer.Editor.Util;
using RayTracer.Runtime;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RayTracer.Editor.UI
{
    public abstract class AbstractRayTracerEditorWindow : EditorWindow
    {
        private RayTracingProfileAsset m_Asset;
        private Texture2D m_BlackTexture;
        private IRayTracingContext m_Context;
        private RenderTexture m_RenderTexture;
        private Rect m_RemainingRect;
        private bool m_FillWindow;
        private RayTracingProfileAsset[] m_Assets;
        private int[] m_PopupValues;
        private string[] m_PopupNames;
        private int m_AccelerationPopupValue;
        private int[] m_AccelerationPopupValues;
        private string[] m_AccelerationPopupNames;

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

            m_Assets = Resources.LoadAll<RayTracingProfileAsset>("");
            m_PopupValues = new[] {-1}.Concat(m_Assets.Select(a => a.GetInstanceID())).ToArray();
            m_PopupNames = new[] {"None"}.Concat(m_Assets.Select(a => a.name)).ToArray();

            m_AccelerationPopupValue = 0;
            m_AccelerationPopupValues = new[] {0, 1};
            m_AccelerationPopupNames = new[] {"No acceleration structure", "Bounding Volume Hierarchy"};

            m_Context = new RayTracingContext();
        }

        public void OnDisable()
        {
            m_Context.Dispose();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var currentPopupValue = m_Asset != null ? m_Asset.GetInstanceID() : -1;
            var popupValue = EditorGUILayout.IntPopup(currentPopupValue, m_PopupNames, m_PopupValues, EditorStyles.toolbarPopup, GUILayout.ExpandWidth(false));
            if (popupValue == -1)
                m_Asset = null;
            else if (currentPopupValue != popupValue)
                m_Asset = m_Assets.FirstOrDefault(a => a.GetInstanceID() == popupValue);

            var newAccelerationPopupValue = EditorGUILayout.IntPopup(m_AccelerationPopupValue, m_AccelerationPopupNames, m_AccelerationPopupValues, EditorStyles.toolbarPopup, GUILayout.ExpandWidth(false));
            if (newAccelerationPopupValue != m_AccelerationPopupValue)
            {
                m_Context.Dispose();
                if (newAccelerationPopupValue == 0)
                    m_Context = new RayTracingContext();
                else if (newAccelerationPopupValue == 1)
                    m_Context = new BvhRayTracingContext();
                m_AccelerationPopupValue = newAccelerationPopupValue;
                Debug.Log(m_AccelerationPopupValue);
            }
            EditorGUILayout.Space();

            m_FillWindow = GUILayout.Toggle(m_FillWindow, "Fill window", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));

            GUI.enabled = asset != null;
            if (GUILayout.Button("Build scene", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                BuildScene();
            if (GUILayout.Button("Render", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
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

            if (m_FillWindow)
                return;
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

        private void BuildScene()
        {
            var sw = new Stopwatch();
            sw.Start();
            m_Context.BuildScene();
            sw.Stop();
            Debug.LogFormat("Scene built in {0} seconds", sw.Elapsed.TotalSeconds);
        }

        private void Render()
        {
            var sw = new Stopwatch();
            sw.Start();
            var width = m_FillWindow ? (int) m_RemainingRect.width : asset.profile.renderWidth;
            var height = m_FillWindow ? (int) m_RemainingRect.height : asset.profile.renderHeight;
            if (m_RenderTexture != null && m_RenderTexture.IsCreated())
                m_RenderTexture.Release();
            m_RenderTexture = new RenderTexture(width, height, 24) {enableRandomWrite = true};
            m_RenderTexture.Create();
            m_Context.renderTexture = m_RenderTexture;
            m_Context.camera = FindObjectOfType<Camera>();
            Debug.Log(m_Context.Render());
            Repaint();
            sw.Stop();
            Debug.LogFormat("Rendering took {0} seconds", sw.Elapsed.TotalSeconds);
        }
    }
}
