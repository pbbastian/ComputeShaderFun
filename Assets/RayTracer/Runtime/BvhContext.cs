using System;
using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime
{
    public class BvhContext : IDisposable
    {
        public StructuredBuffer<AlignedBvhNode> nodesBuffer;
        public StructuredBuffer<IndexedTriangle> trianglesBuffer;
        public StructuredBuffer<Vector4> verticesBuffer;

        public void Dispose()
        {
            if (nodesBuffer != null) nodesBuffer.Dispose();
            if (trianglesBuffer != null) trianglesBuffer.Dispose();
            if (verticesBuffer != null) verticesBuffer.Dispose();
        }
    }
}
