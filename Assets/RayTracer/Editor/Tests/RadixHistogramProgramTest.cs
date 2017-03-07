using System;
using System.Collections.Generic;
using System.Linq;
using Assets.RayTracer.Runtime.Util;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;
using UnityEngine.Rendering;

namespace RayTracer.Editor.Tests
{
    public class RadixHistogramProgramTest
    {
        public struct TestData
        {
            public int patternCount;
            public int keyShift;

            public override string ToString()
            {
                return new DebugStringBuilder
                {
                    {"patternCount", patternCount},
                    {"keyShift", keyShift}
                }.ToString();
            }
        }

        public static IEnumerable<TestCaseData> testCases
        {
            get
            {
                var patternCounts = new[] {16, 256, 317};
                var keyShifts = new[] {0, 4, 8, 28};
                return patternCounts
                    .SelectMany(patternCount => keyShifts.Select(keyShift => new TestData {patternCount = patternCount, keyShift = keyShift}))
                    .AsNamedTestCase();
            }
        }

        [TestCaseSource("testCases")]
        public void VerifyOutput(TestData data)
        {
            var program = new RadixHistogramProgram();
            var inputPattern = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF}.Select(x => x << data.keyShift).ToArray();

            var expectedHistogramPattern = new int[inputPattern.Length * inputPattern.Length];
            for (var i = 0; i < inputPattern.Length; i++)
                expectedHistogramPattern[i * inputPattern.Length + i] = 1;

            var input = new int[inputPattern.Length * data.patternCount];
            for (var i = 0; i < data.patternCount; i++)
                inputPattern.CopyTo(input, i * inputPattern.Length);

            var expectedHistogram = new int[input.Length * 16];
            for (var i = 0; i < 16; i++)
            for (var j = 0; j < data.patternCount; j++)
                expectedHistogram[i * input.Length + j * inputPattern.Length + i] = 1;

            using (var keyBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var histogramBuffer = new ComputeBuffer(input.Length * 16, sizeof(int)))
            using (var cb = new CommandBuffer())
            {
                program.Dispatch(cb, keyBuffer, histogramBuffer, input.Length, data.keyShift);

                keyBuffer.SetData(input);
                Graphics.ExecuteCommandBuffer(cb);
                var output = new int[expectedHistogram.Length];
                histogramBuffer.GetData(output);

                Assert.AreEqual(expectedHistogram, output);
            }
        }
    }
}
