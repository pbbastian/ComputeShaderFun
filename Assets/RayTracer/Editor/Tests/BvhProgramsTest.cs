using System;
using System.Collections.Generic;
using System.Linq;
using Assets.RayTracer.Runtime.Util;
using NUnit.Framework;
using RayTracer.Runtime.ShaderPrograms;
using RayTracer.Runtime.ShaderPrograms.Types;
using RayTracer.Runtime.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

namespace RayTracer.Editor.Tests
{
    public class BvhProgramsTest
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
                var seeds = new[] {48957345, 84732623};
                var tests =
                    from keyCount in keyCounts
                    from seed in seeds
                    select new TestData {keyCount = keyCount, seed = seed};
                return tests.AsNamedTestCase();
            }
        }


        public static IEnumerable<TestCaseData> testCaseDatas2 = testCaseDatas.ToList();

        private void GenerateKeysAndBounds(TestData data, out int[] keys, out AlignedAabb[] leafBounds)
        {
            var random = new Random(data.seed);
            keys = new int[data.keyCount];
            leafBounds = new AlignedAabb[data.keyCount];
            for (var i = 0; i < data.keyCount; i++)
            {
                keys[i] = random.Next(0, int.MaxValue);
                // leafBounds[i] = new Aabb { min = random.NextVector3(), max = random.NextVector3() };
                leafBounds[i] = new Aabb {min = Vector3.left, max = Vector3.up};
            }

            Array.Sort(keys);
        }

        [TestCaseSource("testCaseDatas")]
        public void ConstructTest(TestData data)
        {
            var program = new BvhConstructProgram();

            int[] keys;
            AlignedAabb[] leafBounds;
            GenerateKeysAndBounds(data, out keys, out leafBounds);

            AlignedBvhNode[] nodes;
            int[] parentIndices;

            using (var keysBuffer = new StructuredBuffer<int>(keys.Length, ShaderSizes.s_Int))
            using (var leafBoundsBuffer = new StructuredBuffer<AlignedAabb>(leafBounds.Length, AlignedAabb.s_Size))
            using (var nodesBuffer = new StructuredBuffer<AlignedBvhNode>(data.keyCount - 1, AlignedBvhNode.s_Size))
            using (var parentIndicesBuffer = new StructuredBuffer<int>(data.keyCount * 2 - 2, ShaderSizes.s_Int))
            using (var cb = new CommandBuffer())
            {
                program.Dispatch(cb, keysBuffer, leafBoundsBuffer, nodesBuffer, parentIndicesBuffer);

                keysBuffer.data = keys;
                leafBoundsBuffer.data = leafBounds;
                Graphics.ExecuteCommandBuffer(cb);
                nodes = nodesBuffer.data;
                parentIndices = parentIndicesBuffer.data;
            }

            AssertVisitedOnce(nodes, keys, parentIndices);
            AssertLeafBounds(nodes, leafBounds);
            AssertUpTraversalPossible(nodes, keys, parentIndices);
        }

        [TestCaseSource("testCaseDatas2")]
        public void ConstructAndFitTest(TestData data)
        {
            var constructProgram = new BvhConstructProgram();
            var fitProgram = new BvhFitProgram();
            var zeroProgram = new ZeroProgram();

            int[] keys;
            AlignedAabb[] leafBounds;
            GenerateKeysAndBounds(data, out keys, out leafBounds);

            AlignedBvhNode[] nodes;
            int[] parentIndices;

            using (var keysBuffer = new StructuredBuffer<int>(keys.Length, ShaderSizes.s_Int))
            using (var leafBoundsBuffer = new StructuredBuffer<AlignedAabb>(leafBounds.Length, AlignedAabb.s_Size))
            using (var nodesBuffer = new StructuredBuffer<AlignedBvhNode>(data.keyCount - 1, AlignedBvhNode.s_Size))
            using (var parentIndicesBuffer = new StructuredBuffer<int>(data.keyCount * 2 - 2, ShaderSizes.s_Int))
            using (var nodeCountersBuffer = new StructuredBuffer<int>(data.keyCount - 1, ShaderSizes.s_Int))
            using (var cb = new CommandBuffer())
            {
                constructProgram.Dispatch(cb, keysBuffer, leafBoundsBuffer, nodesBuffer, parentIndicesBuffer);
                fitProgram.Dispatch(cb, parentIndicesBuffer, nodeCountersBuffer, nodesBuffer);

                keysBuffer.data = keys;
                leafBoundsBuffer.data = leafBounds;
                Graphics.ExecuteCommandBuffer(cb);
                nodes = nodesBuffer.data;
                parentIndices = parentIndicesBuffer.data;
            }

            AssertVisitedOnce(nodes, keys, parentIndices);
            AssertLeafBounds(nodes, leafBounds);
            AssertUpTraversalPossible(nodes, keys, parentIndices);
            AssertLeafBounds(nodes, leafBounds);
        }

        private void AssertVisitedOnce(AlignedBvhNode[] nodes, int[] keys, int[] parentIndices)
        {
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
                var expected = i + 1;
                if (expected >= nodes.Length)
                    expected -= nodes.Length;
                Assert.IsTrue(parent.left == expected || parent.right == expected);
            }
        }

        private void AssertLeafBounds(AlignedBvhNode[] nodes, AlignedAabb[] leafBounds)
        {
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

        private void AssertUpTraversalPossible(AlignedBvhNode[] nodes, int[] keys, int[] parentIndices)
        {
            var maxLevels = nodes.Length;
            var levelMaxCount = 0;

            // Verify that it is possible to walk from all leafs to the top
            for (var i = 0; i < keys.Length; i++)
            {
                int levels = 0;
                var i1 = i;
                TraverseTreeUp(nodes, parentIndices, i, true, (nodeIndex, isLeaf) =>
                {
                    levels++;
                    Assert.LessOrEqual(levels, maxLevels, i1.ToString());
                });
                levelMaxCount = Math.Max(levels, levelMaxCount);
            }
        }

        private void AssertBounds(AlignedBvhNode[] nodes, AlignedAabb[] leafBounds)
        {
            TraverseTreeDepthFirst(nodes, 0, (index, isLeaf) =>
            {
                if (isLeaf) return;
                var node = nodes[index];

                var message = new DebugStringBuilder { { "index", index }, { "node", node.ToString(), "({1})" } }.ToString();
                if (node.isLeftLeaf)
                {
                    var expected = leafBounds[node.left];
                    Assert.AreEqual(expected, node.leftBounds, message + "\nLeft");
                }
                else
                {
                    var leftChild = nodes[node.left];
                    var expected = leftChild.leftBounds.Merge(leftChild.rightBounds);
                    Assert.AreEqual(expected, node.leftBounds, message + "\nLeft");
                }

                if (node.isRightLeaf)
                {
                    var expected = leafBounds[node.right];
                    Assert.AreEqual(expected, node.rightBounds, message + "\nRight");
                }
                else
                {
                    var rightChild = nodes[node.right];
                    var expected = rightChild.leftBounds.Merge(rightChild.rightBounds);
                    Assert.AreEqual(expected, node.rightBounds, message + "\nRight");
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

        public void TraverseTreeUp(AlignedBvhNode[] nodes, int[] parentIndices, int index, bool isLeaf, Action<int, bool> action)
        {
            while (true)
            {
                action(index, isLeaf);
                if (index == 0)
                    return;
                var parentIndex = parentIndices[(isLeaf ? nodes.Length : 0) + index - 1];
                index = parentIndex;
                isLeaf = false;
            }
        }
    }
}
