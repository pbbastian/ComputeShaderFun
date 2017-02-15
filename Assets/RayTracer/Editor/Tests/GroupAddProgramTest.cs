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
            public int offset;
            public int limit;
            public WarpSize warpSize;

            public override string ToString()
            {
                return string.Format("count={0}, warpSize={1}, offset={2}, limit={3}", count, (int) warpSize, offset, limit);
            }
        }

        public static IEnumerable<TestCaseData> testDatas
        {
            get
            {
                var warpSizes = new[] {WarpSize.Warp16, WarpSize.Warp32, WarpSize.Warp64};
                var counts = new[] {123, 1024, 3456};
                var offsets = new[] {0, 14, 22};
                var relativeLimits = new[] {1, 0.8};

                var tests =
                    from warpSize in warpSizes
                    from count in counts
                    from offset in offsets
                    from relativeLimit in relativeLimits
                    select new TestData {count = count, warpSize = warpSize, offset = offset, limit = (int) (relativeLimit * count)};
                
                return tests.AsNamedTestCase();
            }
        }

        [TestCaseSource("testDatas")]
        public void VerifyOutput(TestData data)
        {
            var groupAddProgram = new GroupAddProgram(data.warpSize);

            var perThreadInput = Enumerable.Range(37, data.count).ToArray();
            var perGroupInput = Enumerable.Range(412, groupAddProgram.GetGroupCount(data.limit)).ToArray();
            var expected = perThreadInput.ToArray();
            for (var i = 0; i < perGroupInput.Length; i++)
            {
                var limitIndex = data.offset + data.limit;
                var nextGroupIndex = data.offset + (i + 1) * groupAddProgram.GroupSize;
                for (var j = data.offset + i * groupAddProgram.GroupSize; j < Math.Min(perThreadInput.Length, Math.Min(limitIndex, nextGroupIndex)); j++)
                    expected[j] += perGroupInput[i];
            }

            using (var perThreadBuffer = new ComputeBuffer(data.count, sizeof(int)))
            using (var perGroupBuffer = new ComputeBuffer(groupAddProgram.GetGroupCount(data.limit), sizeof(int)))
            {
                perThreadBuffer.SetData(perThreadInput);
                perGroupBuffer.SetData(perGroupInput);
                groupAddProgram.Dispatch(new GroupAddData(perThreadBuffer, perGroupBuffer, data.offset, data.limit));

                var output = new int[data.count];
                perThreadBuffer.GetData(output);
                //Debug.Log(string.Join(", ", perThreadInput.Select(x => x.ToString()).ToArray()));
                //Debug.Log(string.Join(", ", perGroupInput.Select(x => x.ToString()).ToArray()));
                //Debug.Log(string.Join(", ", expected.Select(x => x.ToString()).ToArray()));
                //Debug.Log(string.Join(", ", output.Select(x => x.ToString()).ToArray()));

                Assert.AreEqual(expected, output);
            }
        }
    }
}
