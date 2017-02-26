using System.Runtime.InteropServices;
using Assets.RayTracer.Runtime.Util;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms.Types
{
    public struct Aabb
    {
        public Vector3 min;
        public Vector3 max;

        public static readonly int s_Size = ShaderSizes.s_Vector3 * 2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AlignedAabb
    {
        [FieldOffset(0)] public Vector3 min;

        [FieldOffset(12)] public Vector3 max;

        public AlignedAabb Merge(AlignedAabb other)
        {
            return new AlignedAabb
            {
                min = Vector3.Min(min, other.min),
                max = Vector3.Max(max, other.max)
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is AlignedAabb)
            {
                var other = (AlignedAabb) obj;
                return this == other;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(AlignedAabb bounds1, AlignedAabb bounds2)
        {
            return bounds1.min == bounds2.min && bounds1.max == bounds2.max;
        }

        public static bool operator !=(AlignedAabb bounds1, AlignedAabb bounds2)
        {
            return !(bounds1 == bounds2);
        }

        public override string ToString()
        {
            return new DebugStringBuilder
            {
                {"min", min},
                {"max", max}
            }.ToString();
        }

        public static readonly int s_Size = ShaderSizes.s_Vector3 * 2;

        public static implicit operator AlignedAabb(Aabb bounds)
        {
            return new AlignedAabb {min = bounds.min, max = bounds.max};
        }
    }
}
