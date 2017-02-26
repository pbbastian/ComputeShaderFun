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
        public static void CreateBvh()
        {
            var scene = SceneManager.GetActiveScene();
            var gameObjects = scene.GetComponentsInChildren<RayTracingObject>().Select(x => x.gameObject).ToList();

            int triangleCount = 0, vertexCount = 0, objectCount = gameObjects.Count;
            foreach (var gameObject in gameObjects)
            {
                // RayTracingObject requires MeshFilter, so we can be sure that it's there.
                var meshFilter = gameObject.GetComponent<MeshFilter>();
                triangleCount += meshFilter.sharedMesh.triangles.Length;
                vertexCount += meshFilter.sharedMesh.vertices.Length;
            }

            var vertexData = new Vector3[vertexCount];
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

                //mesh.triangles.CopyTo(flatTriangleData, triangleIndex);
                for (var i = 0; i < mesh.triangles.Length/3; i++)
                for (var j = 0; j < 3; j++)
                    triangleData[i][j] = (uint) (mesh.triangles[triangleIndex + i + j] + vertexIndex);
                triangleIndex += mesh.triangles.Length/3;

                mesh.vertices.CopyTo(vertexData, vertexIndex);
                mesh.normals.CopyTo(normalData, vertexIndex);
                for (var i = vertexIndex; i < vertexIndex + mesh.vertices.Length; i++)
                    objectIndexData[i] = (uint)transformIndex;
                vertexIndex += mesh.vertices.Length;
                
                transformData[transformIndex] = meshFilter.transform.localToWorldMatrix;
                transformIndex++;
            }

            var vertexBuffer = new StructuredBuffer<Vector3>(vertexCount, ShaderSizes.s_Vector3);
            var normalBuffer = new StructuredBuffer<Vector3>(vertexCount, ShaderSizes.s_Vector3);
            var objectIndexBuffer = new StructuredBuffer<uint>(vertexCount, ShaderSizes.s_UInt);
            var triangleBuffer = new StructuredBuffer<IndexedTriangle>(triangleCount, IndexedTriangle.s_Size);
            var transformBuffer = new StructuredBuffer<Matrix4x4>(objectCount, ShaderSizes.s_Matrix4X4);

            vertexBuffer.data = vertexData;
            normalBuffer.data = normalData;
            objectIndexBuffer.data = objectIndexData;
            triangleBuffer.data = triangleData;
            transformBuffer.data = transformData;

            var transformProgram = new TransformProgram();
            transformProgram.Dispatch(vertexBuffer, normalBuffer, objectIndexBuffer, transformBuffer);
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
