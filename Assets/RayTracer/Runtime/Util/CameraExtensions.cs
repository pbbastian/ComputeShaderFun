using System;
using UnityEngine;

namespace RayTracer.Runtime.Util
{
    public static class CameraExtensions
    {
        public static Vector4 GetZBufferParams(this Camera camera, bool reversedZ)
        {
            var projFar = camera.farClipPlane;
            var projNear = camera.nearClipPlane;

            var invNear = (Math.Abs(projNear) < 1e-6) ? 1f : 1f / projNear;
            var invFar = (Math.Abs(projNear) < 1e-6) ? 1f : 1f / projFar;

            var zc0 = 1f - projFar * invNear;
            var zc1 = projFar * invNear;

            Vector4 zBufferParams = new Vector4(zc0, zc1, zc0 * invFar, zc1 * invFar);

            if (SystemInfo.usesReversedZBuffer)
            {
                zBufferParams.y += zBufferParams.x;
                zBufferParams.x = -zBufferParams.x;
                zBufferParams.w += zBufferParams.z;
                zBufferParams.z = -zBufferParams.z;
            }
            
            return zBufferParams;
        }
    }
}
