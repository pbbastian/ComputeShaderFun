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
        private SerializedBvhContext m_BvhContext;

        [SerializeField]
        private string m_SerializedBvhBuildDateTime;

        private DateTime m_BvhBuildDateTime;

        public SerializedBvhContext bvhContext { get { return m_BvhContext; } }

        public DateTime bvhBuildDateTime { get { return m_BvhBuildDateTime; } }

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
