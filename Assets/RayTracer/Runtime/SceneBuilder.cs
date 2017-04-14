using System.Collections.Generic;
using System.Linq;
using RayTracer.Runtime.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RayTracer.Runtime
{
    public class SceneBuilder
    {
        List<Vector3> m_Albedos = new List<Vector3>();
        List<Triangle> m_Vertices = new List<Triangle>();

        public void AddWithChildren(IEnumerable<GameObject> gameObjects)
        {
            foreach (var gameObject in gameObjects)
                AddWithChildren(gameObject);
        }

        public void Add(Scene scene)
        {
            AddWithChildren(scene.GetRootGameObjects().Where(x => x.activeInHierarchy));
        }

        public void AddWithChildren(GameObject gameObject)
        {
            Add(gameObject);
            foreach (var rto in gameObject.GetComponentsInChildren<RayTracingObject>(false).Select(x => x.gameObject).Where(x => x.activeInHierarchy))
                Add(rto.gameObject);
        }

        public void Add(GameObject gameObject)
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var rto = gameObject.GetComponent<RayTracingObject>();
            if (meshFilter == null || rto == null)
                return;
            Add(meshFilter.sharedMesh, meshFilter.transform, rto.albedo);
        }

        void Add(Mesh mesh, Transform transform, Color albedo)
        {
            var albedoVector = new Vector3(albedo.r, albedo.g, albedo.b);
            for (var i = 0; i < mesh.triangles.Length; i += 3)
            {
                m_Albedos.Add(albedoVector);
                var i0 = mesh.triangles[i + 0];
                var i1 = mesh.triangles[i + 1];
                var i2 = mesh.triangles[i + 2];
                var localNormal = Vector3.Normalize(mesh.normals[i0] + mesh.normals[i1] + mesh.normals[i2]);
                m_Vertices.Add(new Triangle(
                                            transform.TransformPoint(mesh.vertices[i0]),
                                            transform.TransformPoint(mesh.vertices[i1]),
                                            transform.TransformPoint(mesh.vertices[i2]),
                                            transform.TransformDirection(localNormal)
                                           ));
            }
        }

        public void Clear()
        {
            m_Vertices.Clear();
            m_Albedos.Clear();
        }

        public ComputeBuffer BuildTriangleBuffer()
        {
            var buffer = new ComputeBuffer(m_Vertices.Count, 4 * 4 * 3);
            buffer.SetData(m_Vertices.ToArray());
            return buffer;
        }

        public ComputeBuffer BuildMaterialBuffer()
        {
            var buffer = new ComputeBuffer(m_Albedos.Count, 4);
            buffer.SetData(m_Albedos.ToArray());
            return buffer;
        }
    }
}
