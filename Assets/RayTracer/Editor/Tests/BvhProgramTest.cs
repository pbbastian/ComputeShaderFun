using System;
using System.Collections.Generic;
using System.Linq;
using Assets.RayTracer.Runtime.Util;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using Random = System.Random;

namespace RayTracer.Editor.Tests
{
    public class BvhProgramTest
    {
        public struct TestData
        {
            public int keyCount;
            public int seed;

            public override string ToString()
            {
                return new DebugStringBuilder
                {
                    {"keyCount", keyCount},
                    {"seed", seed}
                }.ToString();
            }
        }

        public static IEnumerable<TestCaseData> testCaseDatas
        {
            get
            {
                var keyCounts = new[] {25, 1025, 7890};
                var seeds = new[] {48957345};
                var tests =
                    from keyCount in keyCounts
                    from seed in seeds
                    select new TestData {keyCount = keyCount, seed = seed};
                return tests.AsNamedTestCase();
            }
        }

        private void GenerateKeysAndBounds(TestData data, out int[] keys, out AlignedAabb[] leafBounds)
        {
            var random = new Random(data.seed);
            keys = new int[data.keyCount];
            leafBounds = new AlignedAabb[data.keyCount];
            for (var i = 0; i < data.keyCount; i++)
            {
                keys[i] = random.Next(0, int.MaxValue);
                // leafBounds[i] = new Aabb { min = random.NextVector3(), max = random.NextVector3() };
                leafBounds[i] = new Aabb { min = Vector3.left, max = Vector3.up };
            }
        }

        [TestCaseSource("testCaseDatas")]
        public void ConstructTest(TestData data)
        {
            var program = new BvhConstructProgram();

            int[] keys;
            AlignedAabb[] leafBounds;
            GenerateKeysAndBounds(data, out keys, out leafBounds);

            Array.Sort(keys);

            AlignedBvhNode[] nodes;
            int[] parentIndices;

            using (var keysBuffer = new StructuredBuffer<int>(keys.Length, ShaderSizes.s_Int))
            using (var leafBoundsBuffer = new StructuredBuffer<AlignedAabb>(leafBounds.Length, AlignedAabb.s_Size))
            using (var nodesBuffer = new StructuredBuffer<AlignedBvhNode>(data.keyCount - 1, AlignedBvhNode.s_Size))
            using (var parentIndicesBuffer = new StructuredBuffer<int>(data.keyCount * 2 - 2, ShaderSizes.s_Int))
            {
                keysBuffer.data = keys;
                leafBoundsBuffer.data = leafBounds;
                program.Dispatch(keysBuffer, leafBoundsBuffer, nodesBuffer, parentIndicesBuffer);
                nodes = nodesBuffer.data;
                parentIndices = parentIndicesBuffer.data;
            }

            var nodeVisits = new int[nodes.Length];
            var leafVisits = new int[keys.Length];

            TraverseTreeDepthFirst(nodes, 0, (index, isLeaf) =>
            {
                if (isLeaf)
                    leafVisits[index]++;
                else
                    nodeVisits[index]++;
            });

            // Assert that every internal node and leaf is visited exactly once.
            Assert.AreEqual(Enumerable.Repeat(1, nodeVisits.Length).ToArray(), nodeVisits);
            Assert.AreEqual(Enumerable.Repeat(1, leafVisits.Length).ToArray(), leafVisits);

            // Verify parent indices
            for (var i = 0; i < parentIndices.Length; i++)
            {
                var parent = nodes[parentIndices[i]];
                var j = i + 1;
                if (j >= nodes.Length)
                    j -= nodes.Length;
                Assert.IsTrue(parent.left == j || parent.right == j);
            }

            // Verify that leaf bounds are correctly copied to bottom internal nodes.
            TraverseTreeDepthFirst(nodes, 0, (index, isLeaf) =>
            {
                if (!isLeaf)
                {
                    var node = nodes[index];
                    if (node.isLeftLeaf)
                        Assert.AreEqual(leafBounds[node.left], node.leftBounds);
                    if (node.isRightLeaf)
                        Assert.AreEqual(leafBounds[node.right], node.rightBounds);
                }
            });
        }

        [TestCaseSource("testCaseDatas")]
        public void FitTest(TestData data)
        {
            var constructProgram = new BvhConstructProgram();
            var fitProgram = new BvhFitProgram();
            var zeroProgram = new ZeroProgram();

            int[] keys;
            AlignedAabb[] leafBounds;
            GenerateKeysAndBounds(data, out keys, out leafBounds);

            AlignedBvhNode[] nodes;

            using (var keysBuffer = new StructuredBuffer<int>(keys.Length, ShaderSizes.s_Int))
            using (var leafBoundsBuffer = new StructuredBuffer<AlignedAabb>(leafBounds.Length, AlignedAabb.s_Size))
            using (var nodesBuffer = new StructuredBuffer<AlignedBvhNode>(data.keyCount - 1, AlignedBvhNode.s_Size))
            using (var parentIndicesBuffer = new StructuredBuffer<int>(data.keyCount * 2 - 2, ShaderSizes.s_Int))
            using (var nodeCountersBuffer = new StructuredBuffer<int>(data.keyCount - 1, ShaderSizes.s_Int))
            {
                keysBuffer.data = keys;
                leafBoundsBuffer.data = leafBounds;
                constructProgram.Dispatch(keysBuffer, leafBoundsBuffer, nodesBuffer, parentIndicesBuffer);
                zeroProgram.Dispatch(nodeCountersBuffer.computeBuffer, data.keyCount - 1);
                fitProgram.Dispatch(parentIndicesBuffer, nodeCountersBuffer, nodesBuffer);
                nodes = nodesBuffer.data;
            }

            TraverseTreeDepthFirst(nodes, 0, (index, isLeaf) =>
            {
                if (isLeaf) return;
                var node = nodes[index];

                if (node.isLeftLeaf)
                {
                    var expected = leafBounds[node.left];
                    Assert.AreEqual(expected, node.leftBounds);
                }
                else
                {
                    var leftChild = nodes[node.left];
                    var expected = leftChild.leftBounds.Merge(leftChild.rightBounds);
                    Assert.AreEqual(expected, node.leftBounds);
                }

                if (node.isRightLeaf)
                {
                    var expected = leafBounds[node.right];
                    Assert.AreEqual(expected, node.rightBounds);
                }
                else
                {
                    var rightChild = nodes[node.right];
                    var expected = rightChild.leftBounds.Merge(rightChild.rightBounds);
                    Assert.AreEqual(expected, node.rightBounds);
                }
            });
        }

        public void TraverseTreeDepthFirst(AlignedBvhNode[] tree, int index, Action<int, bool> action)
        {
            var node = tree[index];
            action(index, false);
            
            if (!node.isLeftLeaf)
                TraverseTreeDepthFirst(tree, node.left, action);
            else
                action(node.left, true);

            if (!node.isRightLeaf)
                TraverseTreeDepthFirst(tree, node.right, action);
            else
                action(node.right, true);
        }
    }
}
