using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.Util;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class RadixCountProgramTest
    {
        public struct KeyTestData
        {
            public int keyMask;
            public int keyShift;

            public override string ToString()
            {
                return string.Format("keyMask={0}, keyShift={1}", Convert.ToString(keyMask, 2).PadLeft(8, '0'), keyShift);
            }
        }

        public struct TestData
        {
            public int count;
            public KeyTestData keyData;

            public override string ToString()
            {
                return string.Format("count={0}, {1}", count, keyData);
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var keyTests = new[] {new KeyTestData {keyMask = 0xF, keyShift = 0}, new KeyTestData {keyMask = 0xF0, keyShift = 4}};
                var sizes = new[] {262144, 262144 - 16 * 7, 262123};
                return keyTests.SelectMany(keyTest => sizes.Select(size => new TestData {count = size, keyData = keyTest})).AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var combinations = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF}.Select(x => x << data.keyData.keyShift).ToArray();
            var keys = new int[data.count];
            var counts = new int[16];
            var expectedCounts = new int[16];

            var groups = data.count.CeilDiv(16);
            for (var i = 0; i < groups; i++)
            {
                var length = Math.Min(16, data.count - i * 16);
                Array.Copy(combinations, 0, keys, i * 16, length);
                for (var j = 0; j < length; j++)
                    expectedCounts[j]++;
            }

            var countProgram = new RadixCountProgram();
            using (var keyBuffer = new ComputeBuffer(keys.Length, sizeof(int)))
            using (var countBuffer = new ComputeBuffer(16, sizeof(int)))
            {
                keyBuffer.SetData(keys);
                countBuffer.SetData(counts);

                countProgram.Dispatch(new RadixCountData
                {
                    itemCount = keys.Length,
                    keyShift = data.keyData.keyShift,
                    keyBuffer = keyBuffer,
                    countBuffer = countBuffer
                });

                countBuffer.GetData(counts);

                Assert.AreEqual(expectedCounts, counts);
            }
        }
    }
}
