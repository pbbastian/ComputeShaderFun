using System;
using System.Globalization;
using System.Linq;
using Assets.ShadowRenderPipeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace ShadowRenderPipeline.Editor
{
    [CustomEditor(typeof(ShadowRenderPipelineAsset))]
    public class ShadowRenderPipelineInspector : UnityEditor.Editor
    {
        class Styles
        {
            public readonly string notActiveLabel = "Shadow Render Pipeline is not active.";
            public readonly GUIContent makeActiveLabel = new GUIContent("Make active");
            public readonly GUIContent bvhLabel = new GUIContent("Bounding Volume Hierarchy");
            public readonly GUIContent destroyBvhLabel = new GUIContent("Destroy BVH");
            public readonly GUIContent buildBvhLabel = new GUIContent("Build BVH");
            public readonly GUIContent lastBuiltLabel = new GUIContent("Last built");
            public readonly GUIContent notAvailableLabel = new GUIContent("--");
            public readonly GUIContent nodesLabel = new GUIContent("Nodes");
            public readonly GUIContent trianglesLabel = new GUIContent("Triangles");
            public readonly GUIContent verticesLabel = new GUIContent("Vertices");
            public readonly GUIContent shadowsLabel = new GUIContent("Shadows");
            public readonly GUIContent debugLabel = new GUIContent("Debug");
            public readonly GUIContent outputBufferLabel = new GUIContent("Output buffer");
            public readonly GUIContent antiAliasingLabel = new GUIContent("Anti-aliasing");
            public readonly GUIContent methodLabel = new GUIContent("Method");
            public readonly GUIContent presetLabel = new GUIContent("Preset");
            public readonly GUIContent fxaaLabel = new GUIContent("FXAA");
            public readonly GUIContent[] fxaaPresets = new[] { "Extreme Performance", "Performance", "Default", "Quality", "Extreme Quality" }.Select(x => new GUIContent(x)).ToArray();
            public readonly GUIContent shadowmapVariantLabel = new GUIContent("Shadowmap Variant");
            public readonly GUILayoutOption buttonWidth = GUILayout.MaxWidth(100f);
            public readonly GUIStyle groupHeaderStyle = EditorStyles.boldLabel;
        }

        static Styles s_Styles;

        static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();
                return s_Styles;
            }
        }

        static void InactiveGUI(ShadowRenderPipelineAsset asset)
        {
            EditorGUILayout.HelpBox(styles.notActiveLabel, MessageType.Warning);
            if (GUILayout.Button(styles.makeActiveLabel, GUILayout.ExpandWidth(false)))
                GraphicsSettings.renderPipelineAsset = asset;
        }

        static void ActiveGUI(ShadowRenderPipelineAsset asset)
        {
            GUILayout.Label(styles.bvhLabel, styles.groupHeaderStyle);
            {
                EditorGUILayout.LabelField(styles.lastBuiltLabel, asset.bvhContext.isValid ? new GUIContent(asset.bvhBuildDateTime.ToString(CultureInfo.CurrentCulture)) : styles.notAvailableLabel);
                EditorGUILayout.LabelField(styles.nodesLabel, asset.bvhContext.isValid ? new GUIContent(asset.bvhContext.nodesBuffer.Length.ToString()) : styles.notAvailableLabel);
                EditorGUILayout.LabelField(styles.trianglesLabel, asset.bvhContext.isValid ? new GUIContent(asset.bvhContext.trianglesBuffer.Length.ToString()) : styles.notAvailableLabel);
                EditorGUILayout.LabelField(styles.verticesLabel, asset.bvhContext.isValid ? new GUIContent(asset.bvhContext.verticesBuffer.Length.ToString()) : styles.notAvailableLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(styles.destroyBvhLabel, styles.buttonWidth))
                        asset.DestroyBvh();
                    if (GUILayout.Button(styles.buildBvhLabel, styles.buttonWidth))
                        asset.BuildBvh();
                }
            }
            EditorGUILayout.Space();

            using (var toggle = new EditorGUILayout.ToggleGroupScope(styles.shadowsLabel, asset.shadowSettings.enabled))
            {
                asset.shadowSettings.enabled = toggle.enabled;
                asset.shadowSettings.shadowmapVariant = (ShadowmapVariant)EditorGUILayout.EnumPopup(styles.shadowmapVariantLabel, asset.shadowSettings.shadowmapVariant);
            }
            EditorGUILayout.Space();

            using (var toggle = new EditorGUILayout.ToggleGroupScope(styles.antiAliasingLabel, asset.antiAliasingSettings.enabled))
            {
                asset.antiAliasingSettings.enabled = toggle.enabled;
                EditorGUILayout.Popup(styles.methodLabel, 0, new[] { styles.fxaaLabel });
                var preset = Array.IndexOf(Fxaa.availablePresets, asset.antiAliasingSettings.preset);
                preset = EditorGUILayout.Popup(styles.presetLabel, preset, styles.fxaaPresets);
                asset.antiAliasingSettings.preset = Fxaa.availablePresets[preset];
            }
            EditorGUILayout.Space();

            using (var toggle = new EditorGUILayout.ToggleGroupScope(styles.debugLabel, asset.debugSettings.enabled))
            {
                asset.debugSettings.enabled = toggle.enabled;
                asset.debugSettings.outputBuffer = (OutputBuffer)EditorGUILayout.EnumPopup(styles.outputBufferLabel, asset.debugSettings.outputBuffer);
            }
            EditorGUILayout.Space();

        }

        public override void OnInspectorGUI()
        {
            var asset = target as ShadowRenderPipelineAsset;
            if (asset != GraphicsSettings.renderPipelineAsset)
            {
                InactiveGUI(asset);
                return;
            }
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                ActiveGUI(asset);
                if (check.changed)
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}
