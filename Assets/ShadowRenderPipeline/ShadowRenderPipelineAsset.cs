using System;
using RayTracer.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ShadowRenderPipeline
{
    [ExecuteInEditMode]
    public class ShadowRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [MenuItem("RenderPipeline/Create ShadowRenderPipeline")]
        static void CreateBasicRenderPipeline()
        {
            var instance = CreateInstance<ShadowRenderPipelineAsset>();
            AssetDatabase.CreateAsset(instance, "Assets/ShadowRenderPipeline/ShadowRenderPipeline.asset");
        }
#endif

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new ShadowRenderPipeline(this);
        }

        [SerializeField]
        SerializedBvhContext m_BvhContext;

        [SerializeField]
        string m_SerializedBvhBuildDateTime;

        [SerializeField]
        DateTime m_BvhBuildDateTime;

        [SerializeField]
        bool m_ShadowsEnabled;

        [SerializeField]
        DebugSettings m_DebugSettings;

        [SerializeField]
        AntiAliasingSettings m_AntiAliasingSettings;

        public SerializedBvhContext bvhContext { get { return m_BvhContext; } }

        public DateTime bvhBuildDateTime { get { return m_BvhBuildDateTime; } }

        public bool shadowsEnabled
        {
            get { return m_ShadowsEnabled; }
            set { m_ShadowsEnabled = value; }
        }

        public DebugSettings debugSettings
        {
            get { return m_DebugSettings ?? (m_DebugSettings = CreateInstance<DebugSettings>()); }
            set { m_DebugSettings = value; }
        }

        public AntiAliasingSettings antiAliasingSettings
        {
            get { return m_AntiAliasingSettings ?? (m_AntiAliasingSettings = CreateInstance<AntiAliasingSettings>()); }
            set { m_AntiAliasingSettings = value; }
        }

        public void BuildBvh()
        {
            m_BvhContext = BvhUtil.CreateBvh().SerializeAndDispose();
            m_BvhBuildDateTime = DateTime.Now;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void DestroyBvh()
        {
            m_BvhContext = new SerializedBvhContext();
            m_BvhBuildDateTime = DateTime.Now;
        }

        public void OnBeforeSerialize()
        {
            m_SerializedBvhBuildDateTime = m_BvhBuildDateTime.ToString("s");
        }

        public void OnAfterDeserialize()
        {
            if (m_SerializedBvhBuildDateTime != null)
                m_BvhBuildDateTime = DateTime.Parse(m_SerializedBvhBuildDateTime);
        }
    }
}
