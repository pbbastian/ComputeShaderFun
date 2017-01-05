using RayTracer.Runtime;
using UnityEditor;

namespace RayTracer.Editor.UI
{
    public class AbstractRayTracerEditorWindow : EditorWindow
    {
        private RayTracingContext m_Context;

        public void OnEnable()
        {
            titleContent.text = "Ray tracer";
            //m_Context = new RayTracingContext();
        }
    }
}
