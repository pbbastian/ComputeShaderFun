﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Editor.Tests
{
    public class ScanProgramTest
    {
        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var offsets = new[] {0, 14, 22};
                var warpSizes = new List<WarpSize> {WarpSize.Warp16, WarpSize.Warp32};
                if (SystemInfo.graphicsDeviceVendorID != 0x10DE)
                    warpSizes.Add(WarpSize.Warp64);
                var counts = new[] {17};
                var warpSpecificCounts = new Dictionary<WarpSize, int[]> {{WarpSize.Warp16, new[] {16 * 15}}, {WarpSize.Warp32, new[] {32 * 31}}, {WarpSize.Warp64, new[] {32 * 32}}};
                var relativeLimits = new[] {1, 0.8, 1.1};

                var tests =
                    from warpSize in warpSizes
                    from count in counts.Concat(warpSpecificCounts[warpSize])
                    from offset in offsets
                    from relativeLimit in relativeLimits
                    select new TestData {count = count, offset = offset, limit = (int) (count * relativeLimit), warpSize = warpSize};

                return tests.AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var input = Enumerable.Range(24, data.count).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 0; i < input.Length; i++)
                if (i >= data.offset && i < Math.Min(data.offset + data.limit, input.Length))
                    expected[i] = input.Skip(data.offset).Take(i - data.offset).Sum();
                else
                    expected[i] = input[i];

            var scanProgram = new ScanProgram(data.warpSize);
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            using (var cb = new CommandBuffer())
            {
                scanProgram.Dispatch(cb, data.offset, data.limit, inputBuffer, dummyBuffer);

                inputBuffer.SetData(input);
                Graphics.ExecuteCommandBuffer(cb);
                inputBuffer.GetData(output);
                // Debug.Log(string.Join(", ", output.Select(x => x.ToString()).ToArray()));
                Assert.AreEqual(expected, output);
            }
        }

        public struct TestData
        {
            public int count;
            public int offset;
            public int limit;
            public WarpSize warpSize;

            public override string ToString()
            {
                return string.Format("count={0}, offset={1}, limit={2}, warpSize={3}", count, offset, limit, (int) warpSize);
            }
        }
    }
}
