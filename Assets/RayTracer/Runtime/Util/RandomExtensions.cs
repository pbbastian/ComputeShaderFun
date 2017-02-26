using System;
using UnityEngine;
using Random = System.Random;

namespace RayTracer.Runtime.Util
{
    public static class RandomExtensions
    {
        public static float NextFloat(this Random random)
        {
            var mantissa = random.NextDouble() * 2.0 - 1.0;
            var exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float) (mantissa * exponent);
        }

        public static Vector3 NextVector3(this Random random)
        {
            return new Vector3 {x = random.NextFloat(), y = random.NextFloat(), z = random.NextFloat()};
        }
    }
}
