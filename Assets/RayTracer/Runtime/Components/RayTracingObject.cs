using UnityEngine;

namespace RayTracer.Runtime.Components
{
	[RequireComponent(typeof(MeshFilter))]
    public class RayTracingObject : MonoBehaviour
    {
        public Color albedo = Color.gray;
    }
}
