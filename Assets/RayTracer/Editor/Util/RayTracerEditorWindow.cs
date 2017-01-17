using UnityEditor;

namespace RayTracer.Editor.UI
{
    public class RayTracerEditorWindow : AbstractRayTracerEditorWindow
    {
        private static RayTracerEditorWindow s_Window;

        public static RayTracerEditorWindow window
        {
            get
            {
                if (s_Window == null)
                    Init();
                return s_Window;
            }
        }

        public static RayTracerEditorWindow CreateOrGet()
        {
            s_Window = GetWindow<RayTracerEditorWindow>();
            return window;
        }

        [MenuItem("Window/Ray tracer")]
        private static void Init()
        {
            s_Window = GetWindow<RayTracerEditorWindow>(typeof(SceneView));
            s_Window.Show();
        }
    }
}
