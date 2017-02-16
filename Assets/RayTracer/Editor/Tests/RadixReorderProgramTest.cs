using System.Collections.Generic;
using System.Linq;
using Assets.RayTracer.Runtime.Util;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class RadixReorderProgramTest
    {
        public struct TestData
        {
            public int keyShift;
            public int patternCount;

            public override string ToString()
            {
                return new DebugStringBuilder
                {
                    {"keyShift", keyShift},
                    {"patternCount", patternCount}
                }.ToString();
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var keyShifts = new[] {0, 4, 24};
                var patternCounts = new[] {1, 16, 27};

                var tests =
                    from keyShift in keyShifts
                    from patternCount in patternCounts
                    select new TestData {keyShift = keyShift, patternCount = patternCount};

                return tests.AsNamedTestCase();
            }
        }

        public static readonly bool s_Debug = false;

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var program = new RadixReorderProgram();
            var inputPattern = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF}.Select(x => x << data.keyShift).ToArray();
            var input = new int[16 * data.patternCount];
            var expected = new int[input.Length];
            var histogram = new int[input.Length * 16];
            var count = Enumerable.Repeat(data.patternCount, 16).ToArray();
            for (var i = 0; i < data.patternCount; i++)
            {
                inputPattern.CopyTo(input, i * inputPattern.Length);
                for (int j = 0; j < 16; j++)
                {
                    histogram[i * 16 + j * input.Length + j] = 1;
                    expected[data.patternCount * j + i] = inputPattern[j];
                }
            }

            var scannedCount = new int[count.Length];
            var scannedHistogram = new int[histogram.Length];

            for (var i = 0; i < 16; i++)
            {
                scannedCount[i] = count.Take(i).Sum();
                var startIndex = i * input.Length;
                for (var j = 0; j < input.Length; j++)
                {
                    scannedHistogram[startIndex + j] = histogram.Skip(startIndex).Take(j).Sum();
                }
            }

            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var outputBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var histogramBuffer = new ComputeBuffer(histogram.Length, sizeof(int)))
            using (var countBuffer = new ComputeBuffer(count.Length, sizeof(int)))
            {
                inputBuffer.SetData(input);
                histogramBuffer.SetData(scannedHistogram);
                countBuffer.SetData(scannedCount);
                program.Dispatch(new RadixReorderData(inputBuffer, outputBuffer, histogramBuffer, countBuffer, input.Length, data.keyShift));
                var output = new int[input.Length];
                outputBuffer.GetData(output);

                if (s_Debug)
                {
                    Debug.Log("Input: " + string.Join(", ", input.Select(x => x.ToString()).ToArray()));
                    Debug.Log("Count: " + string.Join(", ", scannedCount.Select(x => x.ToString()).ToArray()));
                    Debug.Log("Expected: " + string.Join(", ", expected.Select(x => x.ToString()).ToArray()));
                    Debug.Log("Output: " + string.Join(", ", output.Select(x => x.ToString()).ToArray()));
                    for (var i = 0; i < 16; i++)
                    {
                        Debug.LogFormat("{0} = {1}", i.ToString().PadLeft(2, '0'), string.Join(", ", scannedHistogram.Skip(i * input.Length).Take(input.Length).Select(x => x.ToString()).ToArray()));
                    }
                }

                Assert.AreEqual(expected, output);
            }
        }
    }
}
