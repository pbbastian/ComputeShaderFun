using System.Globalization;
using ShadowRenderPipeline.Editor.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowRenderPipeline.Editor
{
    [CustomEditor(typeof(ShadowRenderPipelineAsset))]
    public class ShadowRenderPipelineInspector : UnityEditor.Editor
    {
        private class Styles
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
            public readonly GUILayoutOption buttonWidth = GUILayout.MaxWidth(100f);
            public readonly GUIStyle groupHeaderStyle = EditorStyles.boldLabel;
        }

        private static Styles s_Styles;

        private static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();
                return s_Styles;
            }
        }

        private static void InactiveGUI(ShadowRenderPipelineAsset asset)
        {
            EditorGUILayout.HelpBox(styles.notActiveLabel, MessageType.Warning);
            if (GUILayout.Button(styles.makeActiveLabel, GUILayout.ExpandWidth(false)))
                GraphicsSettings.renderPipelineAsset = asset;
        }

        private static void ActiveGUI(ShadowRenderPipelineAsset asset)
        {
            GUILayout.Label(styles.bvhLabel, styles.groupHeaderStyle);
            using (new IndentScope())
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

            GUILayout.Label("Shadows", styles.groupHeaderStyle);
            using (new IndentScope())
            {
                //new EditorGUILayout.ToggleGroupScope()
            }
        }

        public override void OnInspectorGUI()
        {
            var asset = target as ShadowRenderPipelineAsset;
            if (asset != GraphicsSettings.renderPipelineAsset)
            {
                InactiveGUI(asset);
                return;
            }
            ActiveGUI(asset);
        }
    }
}
