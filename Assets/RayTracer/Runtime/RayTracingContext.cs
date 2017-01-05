using UnityEngine;

namespace RayTracer.Runtime
{
    public class RayTracingContext
    {
        public RenderTexture renderTexture { get; private set; }

        public RayTracingContext(int width, int height)
        {
            renderTexture = new RenderTexture(width, height, 24) {enableRandomWrite = true};
        }
    }
}
