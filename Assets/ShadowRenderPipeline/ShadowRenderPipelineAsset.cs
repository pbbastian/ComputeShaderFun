using System;
using System.IO;
using Assets.ShadowRenderPipeline;
using RayTracer.Runtime;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        string m_BvhContextId;

        SerializedBvhContext m_BvhContext;

        [SerializeField]
        string m_SerializedBvhBuildDateTime;

        DateTime m_BvhBuildDateTime;

        [SerializeField]
        ShadowSettings m_ShadowSettings;

        [SerializeField]
        DebugSettings m_DebugSettings;

        [SerializeField]
        AntiAliasingSettings m_AntiAliasingSettings;

        public SerializedBvhContext bvhContext
        {
            get
            {
                if (!m_BvhContext.isValid)
                    LoadBvhContext();
                return m_BvhContext;
            }
        }

        void LoadBvhContext()
        {
            m_BvhContext = default(SerializedBvhContext);
            var path = Path.Combine(Application.persistentDataPath, $"{m_BvhContextId}.bvh");
            if (!File.Exists(path))
                return;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var reader = new BinaryReader(stream);
                m_BvhContext = reader.ReadBvhContext();

                //                var formatter = new BinaryFormatter();
                //                var selector = new SurrogateSelector();
                //                selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3Surrogate());
                //                selector.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), new Vector4Surrogate());
                //                formatter.SurrogateSelector = selector;
                //                m_BvhContext = (SerializedBvhContext)formatter.Deserialize(stream);
            }
        }

        public DateTime bvhBuildDateTime => m_BvhBuildDateTime;

        public ShadowSettings shadowSettings
        {
            get { return m_ShadowSettings = m_ShadowSettings ?? new ShadowSettings(); }
            set { m_ShadowSettings = value; }
        }

        public DebugSettings debugSettings
        {
            get { return m_DebugSettings = m_DebugSettings ?? new DebugSettings(); }
            set { m_DebugSettings = value; }
        }

        public AntiAliasingSettings antiAliasingSettings
        {
            get { return m_AntiAliasingSettings = m_AntiAliasingSettings ?? new AntiAliasingSettings(); }
            set { m_AntiAliasingSettings = value; }
        }

        public void BuildBvh()
        {

            DestroyBvh();
            m_BvhContext = BvhUtil.CreateBvh().SerializeAndDispose();
            m_BvhBuildDateTime = DateTime.Now;
            m_BvhContextId = $"{SceneManager.GetActiveScene().name}-{m_BvhBuildDateTime:yyyy-MM-dd_hh-mm-ss-tt}";
            var path = Path.Combine(Application.persistentDataPath, $"{m_BvhContextId}.bvh");
            Debug.Log(path);

            // http://stackoverflow.com/questions/6478579/improve-binary-serialization-performance-for-large-list-of-structs
            using (var stream = new FileStream(path, FileMode.Create))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(m_BvhContext);

                //                var formatter = new BinaryFormatter();
                //                var selector = new SurrogateSelector();
                //                selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3Surrogate());
                //                selector.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), new Vector4Surrogate());
                //                formatter.SurrogateSelector = selector;
                //                formatter.Serialize(stream, m_BvhContext);
            }
        }

        public void DestroyBvh()
        {
            if (m_BvhContextId != null && File.Exists(m_BvhContextId))
                File.Delete(m_BvhContextId);
            m_BvhContextId = null;
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
