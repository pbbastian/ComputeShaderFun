using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
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

            public override string ToString()
            {
                return string.Format("{0} (count={1}, warpSize={2})", name, count, (int) warpSize);
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var multiple = new[] {WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64}
                    .Select(warpSize => new TestData {name = "Multiple of thread group size", count = 1024 * 3, warpSize = warpSize});

                var countMatch = new[] { WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64 }
                    .Select(warpSize => new TestData { name = "Unrelated to thread group size", count = 4657, warpSize = warpSize });

                return multiple.Concat(countMatch).AsNamedTestCase();
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
                    itemCount = input.Length,
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
