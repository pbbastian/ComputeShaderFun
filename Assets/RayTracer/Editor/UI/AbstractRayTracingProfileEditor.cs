using RayTracer.Runtime;
using UnityEditor;
using UnityEngine;

namespace RayTracer.Editor.UI
{
    public abstract class AbstractRayTracingProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var asset = target as RayTracingProfileAsset;
            if (asset == null)
                return;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Label("Render size", "boldLabel");
                asset.profile.renderWidth = EditorGUILayout.IntField("Width", asset.profile.renderWidth);
                asset.profile.renderHeight = EditorGUILayout.IntField("Height", asset.profile.renderHeight);


                if (check.changed)
                    RayTracerEditorWindow.window.Repaint();
            }

            var activeAsset = RayTracerEditorWindow.window.asset;
            GUI.enabled = activeAsset == null || activeAsset.name != asset.name;
            if (GUILayout.Button("Use profile"))
                RayTracerEditorWindow.window.asset = asset;
            GUI.enabled = true;
        }
    }
}
