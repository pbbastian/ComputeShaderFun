using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace ShadowRenderPipeline.Editor
{
    public class CreateShadowPipelineAction : EndNameEditAction
    {
        [MenuItem("Assets/Create/Shadow Render Pipeline")]
        static void CreateAsset()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateShadowPipelineAction>(), "ShadowRenderPipeline.asset", null, null);
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = CreateInstance<ShadowRenderPipelineAsset>();
            AssetDatabase.CreateAsset(instance, pathName);
        }
    }
}
