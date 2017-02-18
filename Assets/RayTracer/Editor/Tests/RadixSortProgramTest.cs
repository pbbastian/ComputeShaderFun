using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.RayTracer.Runtime.Util;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;
using Random = System.Random;

namespace RayTracer.Editor.Tests
{
    public class RadixSortProgramTest
    {
        public struct TestData
        {
            public int count;
            public int seed;
            public WarpSize warpSize;

            public override string ToString()
            {
                return new DebugStringBuilder
                {
                    {"count", count},
                    {"warpSize", warpSize},
                    {"seed", seed}
                }.ToString();
            }
        }

        public static IEnumerable<TestCaseData> tests
        {
            get
            {
                var counts = new[] {16, 256, 1024, 2345};
                var seeds = new[] {64589, 12309222};
                var warpSizes = Enum.GetValues(typeof(WarpSize)).OfType<WarpSize>().ToArray();

                var tests =
                    from count in counts
                    from warpSize in warpSizes
                    from seed in seeds
                    select new TestData {count = count, seed = seed, warpSize = warpSize};

                return tests.AsNamedTestCase();
            }
        }

        public static readonly bool s_Debug = true;

        [TestCaseSource("tests")]
        public void PseudoRandomTest(TestData data)
        {
            var program = new RadixSortProgram(data.warpSize);
            var random = new Random(data.seed);
            var input = new int[data.count];
            for (var i = 0; i < input.Length; i++)
                input[i] = random.Next(0, 16);
            var expected = input.ToArray();
            Array.Sort(expected);

            using (var keyBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var keyBackBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var histogramBuffer = new ComputeBuffer(input.Length * 16, sizeof(int)))
            using (var histogramGroupResultsBuffer = new ComputeBuffer(program.GetHistogramGroupCount(input.Length), sizeof(int)))
            using (var countBuffer = new ComputeBuffer(16, sizeof(int)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                keyBuffer.SetData(input);
                program.Dispatch(new RadixSortData(keyBuffer, keyBackBuffer, histogramBuffer, histogramGroupResultsBuffer, countBuffer, dummyBuffer, data.count));
                var output = new int[data.count];
                keyBackBuffer.GetData(output);

                if (s_Debug)
                {
                    var inputHistogram = new int[input.Length * 16];
                    var scannedInputHistogram = new int[input.Length * 16];
                    for (var i = 0; i < input.Length; i++)
                        inputHistogram[input[i] * input.Length + i] = 1;
                    for (var j = 0; j < 16; j++)
                    for (var i = 0; i < input.Length; i++)
                        scannedInputHistogram[j * input.Length + i] = inputHistogram.Skip(j * input.Length).Take(i).Sum();

                    Debug.Log("Input: " + string.Join(", ", input.Select(x => Convert.ToString(x, 2).PadLeft(4, '0')).ToArray()));
                    Debug.Log("Input: " + string.Join(", ", input.Select(x => x.ToString()).ToArray()));
                    Debug.Log("Expected: " + string.Join(", ", expected.Select(x => Convert.ToString(x, 2).PadLeft(32, '0')).ToArray()));
                    Debug.Log("Expected: " + string.Join(", ", expected.Select(x => x.ToString()).ToArray()));
                    Debug.Log("Output: " + string.Join(", ", output.Select(x => Convert.ToString(x, 2).PadLeft(32, '0').Substring(32 - 4 * 2, 8)).ToArray()));
                    Debug.Log("Output: " + string.Join(", ", output.Select(x => x.ToString()).ToArray()));

                    //var inputHistogram = new int[16];
                    //var scannedInputHistogram = new int[16];
                    //foreach (var x in input)
                    //    inputHistogram[x]++;
                    //for (var i = 0; i < scannedInputHistogram.Length; i++)
                    //    scannedInputHistogram[i] = inputHistogram.Take(i).Sum();


                    //Debug.Log("Input histogram: " + string.Join(", ", inputHistogram.Select((x, i) => string.Format("{0}={1}", i, x)).ToArray()));
                    //Debug.Log("Scanned input histogram: " + string.Join(", ", scannedInputHistogram.Select((x, i) => string.Format("{0}={1}", i, x)).ToArray()));

                    var outputHistogram = new int[output.Length];
                    foreach (var x in output)
                    {
                        if (x < 0 || x >= outputHistogram.Length)
                            outputHistogram = outputHistogram; //Debug.Log("Out of range");
                        else
                            outputHistogram[x]++;
                    }

                    Debug.Log("Output histogram: " + string.Join(", ", outputHistogram.Select((x, i) => string.Format("{0}={1}", i, x)).ToArray()));

                    var count = new int[16];
                    countBuffer.GetData(count);
                    Debug.Log("Count: " + string.Join(", ", count.Select(x => x.ToString()).ToArray()));

                    var histogram = new int[input.Length * 16];
                    histogramBuffer.GetData(histogram);
                    for (var i = 0; i < 16; i++)
                    {
                        var sb = new StringBuilder();
                        sb.Append(string.Format("{0} ({1}) =  ", Convert.ToString(i, 2).PadLeft(4, '0'), i));
                        for (var j = 0; j < input.Length; j++)
                        {
                            var index = i * input.Length + j;
                            sb.Append(string.Format("{0}:{1}={2}:{3}  ", j, index, scannedInputHistogram[index], histogram[index]));
                        }
                        Debug.Log(sb.ToString());
                        // Debug.LogFormat("{2} ({0}) = {1}", i.ToString().PadLeft(2, '0'), string.Join(", ", histogram.Skip(i * input.Length).Take(input.Length).Select(x => x.ToString()).ToArray()), Convert.ToString(i, 2).PadLeft(4, '0'));
                    }   
                    if (false)
                        for (var i = 0; i < 16; i++)
                        for (var j = 0; j < input.Length; j++)
                        {
                            if (histogram[i * input.Length + j] == 1 && input[j] != i)
                                Debug.LogError(string.Format("input[{0}] should be {1} but is {2}", j, i, input[j]));
                        }
                    else
                        Assert.AreEqual(scannedInputHistogram, histogram);
                }

                Assert.AreEqual(expected, output);
            }
        }
    }
}
