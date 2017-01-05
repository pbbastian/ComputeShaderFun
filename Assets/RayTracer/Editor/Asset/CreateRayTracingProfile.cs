using System.IO;
using RayTracer.Runtime;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace RayTracer.Editor.Asset
{
    public class CreateRayTracingProfile : EndNameEditAction
    {
        [MenuItem("Assets/Create/Ray Tracing Profile")]
        public static void Create()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateRayTracingProfile>(), "New Ray Tracing Profile.rtprofile", null, null);
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var asset = CreateInstance<RayTracingProfileAsset>();
            asset.name = Path.GetFullPath(pathName);
            AssetDatabase.CreateAsset(asset, pathName);
        }
    }
}
