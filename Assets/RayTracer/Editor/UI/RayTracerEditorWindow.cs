using UnityEditor;
using UnityEngine;

namespace RayTracer.Editor.UI
{
    public class RayTracerEditorWindow : AbstractRayTracerEditorWindow
    {
        [MenuItem("Window/Ray tracer")]
        static void Init()
        {
            var window = GetWindow<RayTracerEditorWindow>();
            window.Show();
        }
    }
}
