using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.Shaders;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class GlobalScanShaderTest
    {
        private void CountMatch(WarpSize warpSize)
        {
            var input = Enumerable.Range(0, 1024 * 3).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var globalScanShader = new GlobalScanShader(warpSize);
            using (var scanBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var groupResultsBuffer = new ComputeBuffer(globalScanShader.GetGroupCount(input.Length), sizeof(int)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                scanBuffer.SetData(input);
                globalScanShader.Dispatch(new GlobalScanData
                {
                    itemCount = input.Length,
                    buffer = scanBuffer,
                    groupResultsBuffer = groupResultsBuffer,
                    dummyBuffer = dummyBuffer
                });

                scanBuffer.GetData(output);
                Assert.AreEqual(expected, output);
            }
        }

        [Test]
        public void CountMatch_Warp16()
        {
            CountMatch(WarpSize.Warp16);
        }

        [Test]
        public void CountMatch_Warp32()
        {
            CountMatch(WarpSize.Warp32);
        }

        [Test]
        public void CountMatch_Warp64()
        {
            CountMatch(WarpSize.Warp64);
        }
    }
}
