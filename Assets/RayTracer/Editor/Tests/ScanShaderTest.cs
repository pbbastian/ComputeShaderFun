using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.Shaders;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class ScanShaderTest
    {
        private void SingleThreadGroup(int count, WarpSize warpSize)
        {
            var input = Enumerable.Range(24, count).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var scanShader = new ScanShader(warpSize);
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                inputBuffer.SetData(input);
                scanShader.Dispatch(new ScanData
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

        [Test]
        public void SingleThreadGroup_CountMatch_Warp16()
        {
            SingleThreadGroup(256, WarpSize.Warp16);
        }

        [Test]
        public void SingleThreadGroup_CountMatch_Warp32()
        {
            SingleThreadGroup(1024, WarpSize.Warp32);
        }

        [Test]
        public void SingleThreadGroup_CountMatch_Warp64()
        {
            SingleThreadGroup(1024, WarpSize.Warp64);
        }

        [Test]
        public void SingleThreadGroup_CountMismatch_Warp16()
        {
            SingleThreadGroup(10, WarpSize.Warp16);
        }

        [Test]
        public void SingleThreadGroup_CountMismatch_Warp32()
        {
            SingleThreadGroup(10, WarpSize.Warp32);
        }

        [Test]
        public void SingleThreadGroup_CountMismatch_Warp64()
        {
            SingleThreadGroup(10, WarpSize.Warp64);
        }
        
        private void MultipleThreadGroups_CountMatch(WarpSize warpSize)
        {
            Debug.Log(warpSize);
            var input = Enumerable.Range(0, 1024*3).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var output2 = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var scanShader = new ScanShader(warpSize);
            var groupAddShader = new GroupAddShader(warpSize);
            var groupCount = scanShader.GetGroupCount(input.Length);
            Debug.Log(groupCount);
            var groupOutput = new int[groupCount];
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(int)))
            using (var groupResultsBuffer = new ComputeBuffer(groupCount, sizeof(int)))
            using (var dummyBuffer = new ComputeBuffer(1, 4))
            {
                inputBuffer.SetData(input);
                scanShader.Dispatch(new ScanData
                {
                    itemCount = input.Length,
                    buffer = inputBuffer,
                    groupResultsBuffer = groupResultsBuffer
                });

                scanShader.Dispatch(new ScanData
                {
                    itemCount = groupCount,
                    buffer = groupResultsBuffer,
                    groupResultsBuffer = dummyBuffer
                });

                inputBuffer.GetData(output2);
                Debug.Log("Input before group add: " + string.Join(", ", output2.Skip(scanShader.groupSize).Select(x => x.ToString()).ToArray()));

                groupResultsBuffer.GetData(groupOutput);
                Debug.Log("Gropu results after scan: " + string.Join(", ", groupOutput.Select(x => x.ToString()).ToArray()));

                groupAddShader.Dispatch(new GroupAddData
                {
                    itemCount = input.Length,
                    perThreadBuffer = inputBuffer,
                    perGroupBuffer = groupResultsBuffer
                });
                
                inputBuffer.GetData(output);
                Debug.Log("Input after group add: " + string.Join(", ", output.Skip(scanShader.groupSize).Select(x => x.ToString()).ToArray()));

                Assert.AreEqual(expected, output);
            }
        }

        [Test]
        public void MultipleThreadGroups_CountMatch_Warp16()
        {
            MultipleThreadGroups_CountMatch(WarpSize.Warp16);
        }

        [Test]
        public void MultipleThreadGroups_CountMatch_Warp32()
        {
            MultipleThreadGroups_CountMatch(WarpSize.Warp32);
        }

        [Test]
        public void MultipleThreadGroups_CountMatch_Warp64()
        {
            MultipleThreadGroups_CountMatch(WarpSize.Warp64);
        }
    }
}
