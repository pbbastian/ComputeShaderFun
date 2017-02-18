using System;
using System.Collections.Generic;
using System.Linq;
using Assets.RayTracer.Runtime.Util;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class GlobalScanProgramTest
    {
        public struct TestData
        {
            public int count;
            public WarpSize warpSize;
            public int offset;
            public int limit;
            public int seed;

            public override string ToString()
            {
                return new DebugStringBuilder
                {
                    { "count", count },
                    { "warpSize", warpSize },
                    { "offset", offset },
                    { "limit", limit },
                    { "seed", seed }
                }.ToString();
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var warpSizes = new[] {WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64};
                var counts = new[] {1024 * 3, 4657};
                var offsets = new[] {0, 3, 1025, 17};
                var relativeLimits = new[] {1, 0.7};

                var tests =
                    from warpSize in warpSizes
                    from count in counts
                    from offset in offsets
                    from relativeLimit in relativeLimits
                    select new TestData {count = count, warpSize = warpSize, offset = offset, limit = (int)(relativeLimit * count), seed = 3456};
                
                return tests.AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var random = new System.Random(data.seed);
            var input = new int[data.count];
            for (var i = 0; i < data.count; i++)
                input[i] = random.Next(0, 2^30);
            var output = new int[input.Length];
            var expected = input.ToArray();
            for (var i = data.offset; i < Math.Min(data.offset + data.limit, input.Length); i++)
                expected[i] = input.Skip(data.offset).Take(i - data.offset).Sum();

            var globalScanProgram = new GlobalScanProgram(data.warpSize);
            using (var scanBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var groupResultsBuffer = new ComputeBuffer(globalScanProgram.GetGroupCount(input.Length), sizeof(int)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                scanBuffer.SetData(input);
                globalScanProgram.Dispatch(new GlobalScanData
                {
                    offset = data.offset,
                    limit = data.limit,
                    buffer = scanBuffer,
                    groupResultsBuffer = groupResultsBuffer,
                    dummyBuffer = dummyBuffer
                });

                scanBuffer.GetData(output);
                Assert.AreEqual(expected, output);
            }
        }
    }
}
