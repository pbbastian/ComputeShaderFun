using System.IO;
using RayTracer.Runtime;
using RayTracer.Runtime.ShaderPrograms.Types;
using UnityEngine;

namespace ShadowRenderPipeline
{
    public static class BinaryRWExtensions
    {
        #region Vector3

        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        #endregion

        #region Vector4

        public static void Write(this BinaryWriter writer, Vector4 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        #endregion

        #region Aabb

        public static void Write(this BinaryWriter writer, Aabb aabb)
        {
            writer.Write(aabb.min);
            writer.Write(aabb.max);
        }

        public static Aabb ReadAabb(this BinaryReader reader)
        {
            return new Aabb { min = reader.ReadVector3(), max = reader.ReadVector3() };
        }

        #endregion

        #region AlignedBvhNode

        public static void Write(this BinaryWriter writer, AlignedBvhNode node)
        {
            writer.Write(node.leftBounds);
            writer.Write(node.rightBounds);
            writer.Write(node.left);
            writer.Write(node.right);
            writer.Write(node.isLeftLeaf);
            writer.Write(node.isRightLeaf);
        }

        public static AlignedBvhNode ReadAlignedBvhNode(this BinaryReader reader)
        {
            return new AlignedBvhNode { leftBounds = reader.ReadAabb(), rightBounds = reader.ReadAabb(), left = reader.ReadInt32(), right = reader.ReadInt32(), isLeftLeaf = reader.ReadBoolean(), isRightLeaf = reader.ReadBoolean() };
        }

        #endregion

        #region IndexedTriangle

        public static void Write(this BinaryWriter writer, IndexedTriangle triangle)
        {
            writer.Write(triangle.v1);
            writer.Write(triangle.v2);
            writer.Write(triangle.v3);
        }

        public static IndexedTriangle ReadIndexedTriangle(this BinaryReader reader)
        {
            return new IndexedTriangle { v1 = reader.ReadUInt32(), v2 = reader.ReadUInt32(), v3 = reader.ReadUInt32() };
        }

        #endregion

        #region SerializedBvhContext

        public static void Write(this BinaryWriter writer, SerializedBvhContext context)
        {
            writer.Write(context.nodesBuffer.Length);
            writer.Write(context.trianglesBuffer.Length);
            writer.Write(context.verticesBuffer.Length);

            foreach (var node in context.nodesBuffer)
                writer.Write(node);

            foreach (var triangle in context.trianglesBuffer)
                writer.Write(triangle);

            foreach (var vertex in context.verticesBuffer)
                writer.Write(vertex);
        }

        public static SerializedBvhContext ReadBvhContext(this BinaryReader reader)
        {
            var context = new SerializedBvhContext
            {
                nodesBuffer = new AlignedBvhNode[reader.ReadInt32()],
                trianglesBuffer = new IndexedTriangle[reader.ReadInt32()],
                verticesBuffer = new Vector4[reader.ReadInt32()]
            };
            for (var i = 0; i < context.nodesBuffer.Length; i++)
                context.nodesBuffer[i] = reader.ReadAlignedBvhNode();
            for (var i = 0; i < context.trianglesBuffer.Length; i++)
                context.trianglesBuffer[i] = reader.ReadIndexedTriangle();
            for (var i = 0; i < context.verticesBuffer.Length; i++)
                context.verticesBuffer[i] = reader.ReadVector4();
            return context;
        }

        #endregion
    }
}
