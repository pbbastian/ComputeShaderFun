using System;
using System.Collections.Generic;
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
            nodesBuffer = null;
            trianglesBuffer = null;
            verticesBuffer = null;
        }

        public SerializedBvhContext Serialize()
        {
            var serializedContext = new SerializedBvhContext();
            serializedContext.nodesBuffer = SerializeBuffer(nodesBuffer);
            serializedContext.trianglesBuffer = SerializeBuffer(trianglesBuffer);
            serializedContext.verticesBuffer = SerializeBuffer(verticesBuffer);
            return serializedContext;
        }

        public SerializedBvhContext SerializeAndDispose()
        {
            var serializedContext = Serialize();
            Dispose();
            return serializedContext;
        }

        T[] SerializeBuffer<T>(StructuredBuffer<T> buffer) where T : struct
        {
            if (buffer == null)
                return null;
            return buffer.data;
        }
    }
}
