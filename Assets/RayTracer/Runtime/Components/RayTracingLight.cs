using UnityEngine;

namespace RayTracer.Runtime.Components
{
    public class RayTracingLight : MonoBehaviour
    {
        public Color color = Color.white;

        [Range(0f, 8f)] public float intensity = 1f;

        public RayTracingLightType type = RayTracingLightType.Directional;
    }
}
