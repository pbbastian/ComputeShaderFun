using UnityEngine;

namespace RayTracer.Runtime.Components
{
	[RequireComponent(typeof(MeshRenderer))]
    public class RayTracingObject : MonoBehaviour
    {
        public Color albedo = Color.gray;
    }
}
