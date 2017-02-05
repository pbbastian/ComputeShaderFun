using System.Linq;
using NUnit.Framework;
using RayTracer.Runtime.Shaders;
using UnityEngine;

namespace RayTracer.Editor.Tests
{
    public class ScanShaderTest
    {
        [Test]
        public void SingleThreadGroup_CountMatch()
        {
            var input = Enumerable.Range(0, 512).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var scanShader = new ScanShader();
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            using (var outputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            {
                inputBuffer.SetData(input);
                scanShader.inputBuffer = inputBuffer;
                scanShader.outputBuffer = outputBuffer;
                scanShader.Dispatch(input.Length);
                outputBuffer.GetData(output);
                // Debug.Log(string.Join(", ", output.Select(x => x.ToString()).ToArray()));

                Assert.AreEqual(expected, output);
            }
        }

        [Test]
        public void SingleThreadGroup_CountMismatch()
        {
            var input = Enumerable.Range(0, 10).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var scanShader = new ScanShader();
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            using (var outputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            {
                inputBuffer.SetData(input);
                scanShader.inputBuffer = inputBuffer;
                scanShader.outputBuffer = outputBuffer;
                scanShader.Dispatch(input.Length);
                outputBuffer.GetData(output);
                // Debug.Log(string.Join(", ", output.Select(x => x.ToString()).ToArray()));

                Assert.AreEqual(expected, output);
            }
        }

        [Test]
        public void MultipleThreadGroups_CountMatch()
        {
            var input = Enumerable.Range(0, 2049).Select(x => x + 1).ToArray();
            var output = new int[input.Length];
            var expected = new int[input.Length];
            for (var i = 1; i < input.Length; i++)
                expected[i] = input.Take(i).Sum();

            var scanShader = new ScanShader();
            using (var inputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            using (var outputBuffer = new ComputeBuffer(input.Length, sizeof(float)))
            {
                inputBuffer.SetData(input);
                scanShader.inputBuffer = inputBuffer;
                scanShader.outputBuffer = outputBuffer;
                scanShader.Dispatch(input.Length);
                outputBuffer.GetData(output);

                Assert.AreEqual(expected, output);
            }
        }
    }
}
