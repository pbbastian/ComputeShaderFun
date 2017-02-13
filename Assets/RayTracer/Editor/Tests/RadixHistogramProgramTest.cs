using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;

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
                return string.Format("patternCount={0}, keyShift={1}", patternCount, keyShift);
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

            var expectedHistogram = new int[expectedHistogramPattern.Length * data.patternCount];
            for (var i = 0; i < data.patternCount; i++)
                expectedHistogramPattern.CopyTo(expectedHistogram, i * expectedHistogramPattern.Length);

            using (var keyBuffer = new ComputeBuffer(inputPattern.Length * data.patternCount, sizeof(int)))
            using (var histogramBuffer = new ComputeBuffer(inputPattern.Length * data.patternCount, 16 * sizeof(int)))
            {
                keyBuffer.SetData(input);
                
                program.Dispatch(new RadixHistogramData
                {
                    keyBuffer = keyBuffer,
                    histogramBuffer = histogramBuffer,
                    itemCount = input.Length,
                    keyShift = data.keyShift
                });

                var output = new int[expectedHistogram.Length];
                histogramBuffer.GetData(output);

                Assert.AreEqual(expectedHistogram, output);
            }
        }
    }
}
