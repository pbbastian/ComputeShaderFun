using System;
using System.Runtime.InteropServices;
using Assets.RayTracer.Runtime.Util;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Runtime.ShaderPrograms.Types
{
    [Serializable]
    public struct Aabb
    {
        public Vector3 min;
        public Vector3 max;

        public Aabb Merge(Aabb other)
        {
            return new Aabb
            {
                min = Vector3.Min(min, other.min),
                max = Vector3.Max(max, other.max)
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is Aabb)
            {
                var other = (Aabb) obj;
                return this == other;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(Aabb bounds1, Aabb bounds2)
        {
            return bounds1.min == bounds2.min && bounds1.max == bounds2.max;
        }

        public static bool operator !=(Aabb bounds1, Aabb bounds2)
        {
            return !(bounds1 == bounds2);
        }

        public bool Equals(Aabb other)
        {
            return min.Equals(other.min) && max.Equals(other.max);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (min.GetHashCode() * 397) ^ max.GetHashCode();
            }
        }

        public override string ToString()
        {
            return new DebugStringBuilder
            {
                {"min", min},
                {"max", max}
            }.ToString();
        }

        public static readonly int Size = ShaderSizes.s_Vector3 * 2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AlignedAabb
    {
        [FieldOffset(0)] public Aabb aabb;

        [FieldOffset(28)]
        int padding;

        public override bool Equals(object obj)
        {
            if (obj is Aabb)
                return aabb.Equals(obj);
            if (obj is AlignedAabb)
            {
                var other = (AlignedAabb) obj;
                return aabb == other.aabb;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(AlignedAabb bounds1, AlignedAabb bounds2)
        {
            return bounds1.aabb == bounds2.aabb;
        }

        public static bool operator !=(AlignedAabb bounds1, AlignedAabb bounds2)
        {
            return bounds1.aabb != bounds2.aabb;
        }

        public bool Equals(AlignedAabb other)
        {
            return aabb.Equals(other.aabb);
        }

        public override int GetHashCode()
        {
            return aabb.GetHashCode();
        }

        public override string ToString()
        {
            return aabb.ToString();
        }

        public static readonly int s_Size = ShaderSizes.s_Vector4 * 2;

        public static implicit operator AlignedAabb(Aabb aabb)
        {
            return new AlignedAabb {aabb = aabb};
        }
    }
}
