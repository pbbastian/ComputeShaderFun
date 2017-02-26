using System;
using System.Collections.Generic;
using System.Linq;
using RayTracer.Runtime.Components;
using RayTracer.Runtime.ShaderPrograms;
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
            var triangleData = new uint[triangleCount * 3];
            var transformData = new Matrix4x4[objectCount];

            int vertexIndex = 0, triangleIndex = 0, transformIndex = 0;
            foreach (var gameObject in gameObjects)
            {
                var meshFilter = gameObject.GetComponent<MeshFilter>();
                var mesh = meshFilter.sharedMesh;

                mesh.triangles.CopyTo(triangleData, triangleIndex);
                for (var i = triangleIndex; i < triangleIndex + mesh.triangles.Length; i++)
                    triangleData[i] += (uint) vertexIndex;
                triangleIndex += mesh.triangles.Length;

                mesh.vertices.CopyTo(vertexData, vertexIndex);
                mesh.normals.CopyTo(normalData, vertexIndex);
                for (var i = vertexIndex; i < vertexIndex + mesh.vertices.Length; i++)
                    objectIndexData[i] = (uint)transformIndex;
                vertexIndex += mesh.vertices.Length;
                
                transformData[transformIndex] = meshFilter.transform.localToWorldMatrix;
                transformIndex++;
            }

            var vertexBuffer = new StructuredBuffer<Vector3>(vertexCount, sizeof(float) * 3);
            var normalBuffer = new StructuredBuffer<Vector3>(vertexCount, sizeof(float) * 3);
            var objectIndexBuffer = new StructuredBuffer<uint>(vertexCount, sizeof(uint));
            var triangleBuffer = new StructuredBuffer<Triangle>(triangleCount, sizeof(uint) * 3);
            var transformBuffer = new StructuredBuffer<Matrix4x4>(objectCount, sizeof(float) * 4 * 4);

            vertexBuffer.data = vertexData;
            normalBuffer.data = normalData;
            objectIndexBuffer.data = objectIndexData;
            triangleBuffer.computeBuffer.SetData(triangleData);
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
