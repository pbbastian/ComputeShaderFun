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
            public int count;
            public WarpSize warpSize;

            public override string ToString()
            {
                return string.Format("Count={0}, WarpSize={1}", count, (int) warpSize);
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var countMismatch =
                    new[] {WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64}
                    .Select(warpSize => new TestData {count = 10, warpSize = warpSize})
                    .AsNamedTestCase("Count mismatch");

                var countMatch =
                    new[] {WarpSize.Warp32, WarpSize.Warp64}.Select((warpSize) => new TestData {count = 1024, warpSize = warpSize})
                    .Concat(new[] {new TestData {count = 256, warpSize = WarpSize.Warp16}})
                    .AsNamedTestCase("Count match");

                return countMismatch.Concat(countMatch);
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

        //[Test]
        //public void CountMatch_Warp16()
        //{
        //    SingleThreadGroup(256, WarpSize.Warp16);
        //}

        //[Test]
        //public void CountMatch_Warp32()
        //{
        //    SingleThreadGroup(1024, WarpSize.Warp32);
        //}

        //[Test]
        //public void CountMatch_Warp64()
        //{
        //    SingleThreadGroup(1024, WarpSize.Warp64);
        //}

        //[Test]
        //public void CountMismatch_Warp16()
        //{
        //    SingleThreadGroup(10, WarpSize.Warp16);
        //}

        //[Test]
        //public void CountMismatch_Warp32()
        //{
        //    SingleThreadGroup(10, WarpSize.Warp32);
        //}

        //[Test]
        //public void CountMismatch_Warp64()
        //{
        //    SingleThreadGroup(10, WarpSize.Warp64);
        //}
    }
}
