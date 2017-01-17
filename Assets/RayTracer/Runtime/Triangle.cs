using UnityEngine;

namespace RayTracer.Runtime
{
	public struct Triangle
	{
		public Vector3 a;
		public Vector3 b;
		public Vector3 c;
		public Vector3 normal;

		public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.normal = normal;
		}
	}
}
