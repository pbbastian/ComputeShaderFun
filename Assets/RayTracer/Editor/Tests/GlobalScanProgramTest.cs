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
            public string name;
            public int count;
            public WarpSize warpSize;
            public int offset;
            public int limit;

            public override string ToString()
            {
                return new DebugStringBuilder
                {
                    { "count", count },
                    { "warpSize", warpSize },
                    { "offset", offset },
                    { "limit", limit }
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
                    select new TestData {count = count, warpSize = warpSize, offset = offset, limit = (int)(relativeLimit * count)};
                
                return tests.AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var input = Enumerable.Range(0, data.count).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var globalScanProgram = new GlobalScanProgram(data.warpSize);
            using (var scanBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var groupResultsBuffer = new ComputeBuffer(globalScanProgram.GetGroupCount(input.Length), sizeof(int)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                scanBuffer.SetData(input);
                globalScanProgram.Dispatch(new GlobalScanData
                {
                    limit = input.Length,
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
