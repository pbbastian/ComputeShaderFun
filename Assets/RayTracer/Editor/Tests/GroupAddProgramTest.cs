using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class GroupAddProgramTest
    {
        public struct TestData
        {
            public int count;
            public WarpSize warpSize;

            public override string ToString()
            {
                return string.Format("count={0}, warpSize={1}", count, (int) warpSize);
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var tests = new[] {WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64}.Select(warpSize => new TestData {count = 3456, warpSize = warpSize});
                return tests.AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var groupAddProgram = new GroupAddProgram(data.warpSize);

            var perThreadInput = Enumerable.Range(37, data.count).ToArray();
            var perGroupInput = Enumerable.Range(412, groupAddProgram.GetGroupCount(data.count)).ToArray();
            var expected = perThreadInput.ToArray();
            for (var i = 0; i < perGroupInput.Length; i++)
            {
                for (var j = i * groupAddProgram.GroupSize; j < Math.Min((i + 1) * groupAddProgram.GroupSize, perThreadInput.Length); j++)
                    expected[j] += perGroupInput[i];
            }

            using (var perThreadBuffer = new ComputeBuffer(data.count, sizeof(int)))
            using (var perGroupBuffer = new ComputeBuffer(groupAddProgram.GetGroupCount(data.count), sizeof(int)))
            {
                perThreadBuffer.SetData(perThreadInput);
                perGroupBuffer.SetData(perGroupInput);
                groupAddProgram.Dispatch(new GroupAddData
                {
                    itemCount = data.count,
                    perThreadBuffer = perThreadBuffer,
                    perGroupBuffer = perGroupBuffer
                });

                var output = new int[data.count];
                perThreadBuffer.GetData(output);

                Assert.AreEqual(expected, output);
            }
        }
    }
}
