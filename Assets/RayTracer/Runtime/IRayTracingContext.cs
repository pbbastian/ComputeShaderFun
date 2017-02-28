using System;
using UnityEngine;

namespace RayTracer.Runtime
{
    public interface IRayTracingContext : IDisposable
    {
        RenderTexture renderTexture { get; set; }
        Camera camera { get; set; }
        void BuildScene();
        bool Validate();
        bool Render();
    }
}
