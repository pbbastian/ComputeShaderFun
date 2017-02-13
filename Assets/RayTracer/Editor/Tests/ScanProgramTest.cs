using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class ScanProgramTest
    {
        public struct TestData
        {
            public string name;
            public int count;
            public WarpSize warpSize;

            public TestData Name(string name)
            {
                this.name = name;
                return this;
            }

            public override string ToString()
            {
                return string.Format("{0} (count={1}, warpSize={2})", name, count, (int)warpSize);
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var countMismatch = new[] {WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64}
                    .Select(warpSize => new TestData {name = "Unrelated to thread group size", count = 17, warpSize = warpSize});

                var countMatch = new[] {WarpSize.Warp32, WarpSize.Warp64}.Select((warpSize) => new TestData {count = 1024, warpSize = warpSize})
                    .Concat(new[] {new TestData {count = 256, warpSize = WarpSize.Warp16}})
                    .Select(x => x.Name("Equal to thread group size"));

                return countMismatch.Concat(countMatch).AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var input = Enumerable.Range(24, data.count).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var scanProgram = new ScanProgram(data.warpSize);
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                inputBuffer.SetData(input);
                scanProgram.Dispatch(new ScanData
                {
                    itemCount = input.Length,
                    buffer = inputBuffer,
                    groupResultsBuffer = dummyBuffer
                });
                inputBuffer.GetData(output);
                // Debug.Log(string.Join(", ", output.Select(x => x.ToString()).ToArray()));
                Assert.AreEqual(expected, output);
            }
        }
    }
}
