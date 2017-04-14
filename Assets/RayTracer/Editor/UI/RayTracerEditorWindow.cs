using UnityEditor;

namespace RayTracer.Editor.UI
{
    public class RayTracerEditorWindow : AbstractRayTracerEditorWindow
    {
        static RayTracerEditorWindow s_Window;

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
            s_Window.OnEnable();
            return window;
        }

        [MenuItem("Window/Ray tracer")]
        static void Init()
        {
            s_Window = GetWindow<RayTracerEditorWindow>(typeof(SceneView));
            s_Window.Show();
        }
    }
}
