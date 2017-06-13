using Assets.BenchPipeline;
using Assets.BenchPipeline.Runtime;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace BenchPipeline.Editor
{
    public class CreateBenchmarkPipelineAction : EndNameEditAction
    {
        [MenuItem("Assets/Create/Benchmark Pipeline")]
        static void CreateAsset()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateBenchmarkPipelineAction>(), "BenchmarkPipeline.asset", null, null);
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = CreateInstance<BenchmarkPipelineAsset>();
            AssetDatabase.CreateAsset(instance, pathName);
        }
    }
}

