using System;
using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime
{
    [Serializable]
    public struct SerializedBvhContext
    {
        public AlignedBvhNode[] nodesBuffer;

        public IndexedTriangle[] trianglesBuffer;

        public Vector4[] verticesBuffer;

        public bool isValid
        {
            get { return IsBufferValid(nodesBuffer) && IsBufferValid(trianglesBuffer) && IsBufferValid(verticesBuffer); }
        }

        public BvhContext Deserialize()
        {
            return new BvhContext
            {
                nodesBuffer = DeserializeBuffer(nodesBuffer, AlignedBvhNode.s_Size),
                trianglesBuffer = DeserializeBuffer(trianglesBuffer, IndexedTriangle.s_Size),
                verticesBuffer = DeserializeBuffer(verticesBuffer, ShaderSizes.s_Vector4)
            };
        }

        static StructuredBuffer<T> DeserializeBuffer<T>(T[] array, int stride) where T : struct
        {
            if (array == null)
                return null;
            return new StructuredBuffer<T>(array.Length, stride) { data = array };
        }

        static bool IsBufferValid<T>(T[] buffer)
        {
            return buffer != null && buffer.Length > 0;
        }
    }
}
