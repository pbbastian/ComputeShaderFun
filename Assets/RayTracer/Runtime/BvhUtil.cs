using System;
using System.Collections.Generic;
using System.Linq;
using RayTracer.Runtime.Components;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RayTracer.Runtime
{
    public static class BvhUtil
    {
        public static BvhContext CreateBvh()
        {
            var scene = SceneManager.GetActiveScene();
            var gameObjects = scene.GetComponentsInChildren<RayTracingObject>().Select(x => x.gameObject).Where(x => x.activeInHierarchy).ToList();
            var sceneBounds = new Aabb()
            {
                min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
                max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity)
            };

            int triangleCount = 0, vertexCount = 0, objectCount = gameObjects.Count;
            foreach (var gameObject in gameObjects)
            {
                // RayTracingObject requires MeshFilter, so we can be sure that it's there.
                var meshFilter = gameObject.GetComponent<MeshFilter>();
                triangleCount += meshFilter.sharedMesh.triangles.Length/3;
                vertexCount += meshFilter.sharedMesh.vertices.Length;
            }

            var vertexData = new Vector4[vertexCount];
            var normalData = new Vector3[vertexCount];
            var objectIndexData = new uint[vertexCount];
            //var flatTriangleData = new IndexedTriangle[triangleCount];
            var triangleData = new IndexedTriangle[triangleCount];
            var transformData = new Matrix4x4[objectCount];

            int vertexIndex = 0, triangleIndex = 0, transformIndex = 0;
            foreach (var gameObject in gameObjects)
            {
                var meshFilter = gameObject.GetComponent<MeshFilter>();
                var mesh = meshFilter.sharedMesh;
                sceneBounds = sceneBounds.Merge(new Aabb { min = mesh.bounds.min, max = mesh.bounds.max });

                //mesh.triangles.CopyTo(flatTriangleData, triangleIndex);
                for (var i = 0; i < mesh.triangles.Length / 3; i++)
                for (var j = 0; j < 3; j++)
                    triangleData[triangleIndex + i][j] = (uint) (mesh.triangles[i*3 + j] + vertexIndex);
                triangleIndex += mesh.triangles.Length / 3;

                for (var i = 0; i < mesh.vertices.Length; i++)
                    vertexData[vertexIndex + i] = meshFilter.transform.TransformPoint(mesh.vertices[i]);
                //mesh.normals.CopyTo(normalData, vertexIndex);
                //for (var i = vertexIndex; i < vertexIndex + mesh.vertices.Length; i++)
                //    objectIndexData[i] = (uint) transformIndex;
                vertexIndex += mesh.vertices.Length;

                transformData[transformIndex] = meshFilter.transform.localToWorldMatrix;
                transformIndex++;
            }

            var transformProgram = new TransformProgram();
            var leafInitProgram = new LeafInitProgram();
            var sortProgram = new RadixSortProgram(WarpSize.Warp32);
            var leafReorderProgram = new LeafReorderProgram();
            var bvhConstructProgram = new BvhConstructProgram();
            var bvhFitProgram = new BvhFitProgram();

            var verticesBuffer = new StructuredBuffer<Vector4>(vertexCount, ShaderSizes.s_Vector4);
            var trianglesBuffer2 = new StructuredBuffer<IndexedTriangle>(triangleCount, IndexedTriangle.s_Size);
            var nodesBuffer = new StructuredBuffer<AlignedBvhNode>(triangleCount - 1, AlignedBvhNode.s_Size);

            //var normalsBuffer = new StructuredBuffer<Vector3>(vertexCount, ShaderSizes.s_Vector3);
            //var objectIndexBuffer = new StructuredBuffer<uint>(vertexCount, ShaderSizes.s_UInt);
            using (var trianglesBuffer1 = new StructuredBuffer<IndexedTriangle>(triangleCount, IndexedTriangle.s_Size))
            using (var transformBuffer = new StructuredBuffer<Matrix4x4>(objectCount, ShaderSizes.s_Matrix4X4))
            using (var leafBoundsBuffer1 = new StructuredBuffer<AlignedAabb>(triangleCount, AlignedAabb.s_Size))
            using (var leafBoundsBuffer2 = new StructuredBuffer<AlignedAabb>(triangleCount, AlignedAabb.s_Size))
            using (var leafKeysBuffer = new StructuredBuffer<int>(triangleCount, ShaderSizes.s_Int))
            using (var leafKeysBackBuffer = new StructuredBuffer<int>(triangleCount, ShaderSizes.s_Int))
            using (var leafIndexBuffer = new StructuredBuffer<int>(triangleCount, ShaderSizes.s_Int))
            using (var leafIndexBackBuffer = new StructuredBuffer<int>(triangleCount, ShaderSizes.s_Int))
            using (var leafHistogramBuffer = new StructuredBuffer<int>(triangleCount*16, ShaderSizes.s_Int))
            using (var leafHistogramGroupResultsBuffer = new StructuredBuffer<int>(sortProgram.GetHistogramGroupCount(triangleCount), ShaderSizes.s_Int))
            using (var leafCountBuffer = new StructuredBuffer<int>(16, ShaderSizes.s_Int))
            using (var dummyBuffer = new StructuredBuffer<int>(1, ShaderSizes.s_Int))
            using (var parentIndicesBuffer = new StructuredBuffer<int>(triangleCount * 2 - 2, ShaderSizes.s_Int))
            using (var nodeCountersBuffer = new StructuredBuffer<int>(triangleCount - 1, ShaderSizes.s_Int))
            {
                verticesBuffer.data = vertexData;
                //normalsBuffer.data = normalData;
                //objectIndexBuffer.data = objectIndexData;
                trianglesBuffer1.data = triangleData;
                transformBuffer.data = transformData;

                //transformProgram.Dispatch(verticesBuffer/*, normalsBuffer*/, objectIndexBuffer, transformBuffer);
                leafInitProgram.Dispatch(sceneBounds, trianglesBuffer1, verticesBuffer, leafBoundsBuffer1, leafKeysBuffer);
                var leafBounds = leafBoundsBuffer1.data;
                Debug.Log(string.Join("\n", leafBounds.Select(x => x.ToString()).ToArray()));
                sortProgram.Dispatch(leafKeysBuffer, leafKeysBackBuffer, leafIndexBuffer, leafIndexBackBuffer, leafHistogramBuffer, leafHistogramGroupResultsBuffer, leafCountBuffer, dummyBuffer, triangleCount);
                leafReorderProgram.Dispatch(leafIndexBuffer, leafBoundsBuffer1, leafBoundsBuffer2, trianglesBuffer1, trianglesBuffer2);
                
                bvhConstructProgram.Dispatch(leafKeysBuffer, leafBoundsBuffer2, nodesBuffer, parentIndicesBuffer);
                bvhFitProgram.Dispatch(parentIndicesBuffer, nodeCountersBuffer, nodesBuffer);
            }

            return new BvhContext
            {
                nodesBuffer = nodesBuffer,
                trianglesBuffer = trianglesBuffer2,
                verticesBuffer = verticesBuffer
            };
        }

        public static bool IsValidForBvh(GameObject gameObject)
        {
            return gameObject.GetComponent<RayTracingObject>() != null
                   && gameObject.GetComponent<MeshFilter>() != null;
        }

        public static IEnumerable<T> GetComponentsInChildren<T>(this Scene scene)
        {
            var rootComponents = scene.GetRootGameObjects().Select(go => go.GetComponent<T>()).Where(c => c != null);
            var childComponents = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>());
            return rootComponents.Concat(childComponents);
        }
    }
}
