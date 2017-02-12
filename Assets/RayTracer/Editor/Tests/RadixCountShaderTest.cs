using System;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.Shaders;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class RadixCountShaderTest
    {
        private void CountMatch(int keyMask, int keyShift, int count)
        {
            var combinations = new[] {0 << keyShift, 1 << keyShift, 2 << keyShift, 3 << keyShift, 4 << keyShift, 5 << keyShift, 6 << keyShift, 7 << keyShift, 8 << keyShift, 9 << keyShift, 0xA << keyShift, 0xB << keyShift, 0xC << keyShift, 0xD << keyShift, 0xE << keyShift, 0xF << keyShift};
            var keys = new int[count];
            var counts = new int[16];
            var expectedCounts = new int[16];

            var groups = count.CeilDiv(16);
            for (var i = 0; i < groups; i++)
            {
                var length = Math.Min(16, count - i * 16);
                Array.Copy(combinations, 0, keys, i * 16, length);
                for (var j = 0; j < length; j++)
                    expectedCounts[j]++;
            }

            var countShader = new RadixCountShader();
            using (var keyBuffer = new ComputeBuffer(keys.Length, sizeof(int)))
            using (var countBuffer = new ComputeBuffer(16, sizeof(int)))
            {
                keyBuffer.SetData(keys);
                countBuffer.SetData(counts);

                countShader.Dispatch(new RadixCountData
                {
                    itemCount = keys.Length,
                    keyMask = keyMask,
                    keyShift = keyShift,
                    keyBuffer = keyBuffer,
                    countBuffer = countBuffer
                });

                countBuffer.GetData(counts);

                Assert.AreEqual(expectedCounts, counts);
            }
        }

        [Test]
        public void NoShifting_CountMatch()
        {
            CountMatch(0xF, 0, 262144);
        }

        [Test]
        public void Shift4_CountMatch()
        {
            CountMatch(0xF0, 4, 262144);
        }

        [Test]
        public void NoShifting_CountSemiMatch()
        {
            CountMatch(0xF, 0, 262144 - 16 * 7);
        }

        [Test]
        public void Shift4_CountSemiMatch()
        {
            CountMatch(0xF0, 4, 262144 - 16 * 7);
        }

        [Test]
        public void NoShifting_CountMisMatch()
        {
            CountMatch(0xF, 0, 262123);
        }

        [Test]
        public void Shift4_CountMisMatch()
        {
            CountMatch(0xF0, 4, 262123);
        }
    }
}
