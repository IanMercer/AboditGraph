using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;

namespace AboditUnitsTest
{
    /// <summary>
    /// Stress and performance sanity tests. These do not measure exact timing in CI but
    /// set generous upper bounds to catch catastrophically slow regressions (O(n²) etc.).
    /// </summary>
    [TestClass]
    public class GraphStressTests
    {
        private const int ChainLength = 10_000;

        [TestMethod]
        public void TopologicalSort_LargeChain_CompletesInReasonableTime()
        {
            var g = new Graph<int, Relation>();
            for (int i = 0; i < ChainLength - 1; i++)
                g.AddStatement(i, Relation.RDFSType, i + 1);

            var sw = Stopwatch.StartNew();
            var result = g.TopologicalSortApprox().ToList();
            sw.Stop();

            result.Should().HaveCount(ChainLength);
            sw.ElapsedMilliseconds.Should().BeLessThan(2_000,
                because: "topological sort of a 10 000-node chain should complete well under 2 s");
        }

        [TestMethod]
        public void DistanceToEverywhere_DenseGraph_CompletesInReasonableTime()
        {
            // 100 nodes, each connected to the next 50 (dense)
            const int nodeCount = 100;
            const int fanOut = 50;
            var g = new Graph<int, Relation>();
            for (int i = 0; i < nodeCount; i++)
                for (int j = 1; j <= fanOut && i + j < nodeCount; j++)
                    g.AddStatement(i, Relation.RDFSType, i + j);

            var sw = Stopwatch.StartNew();
            var result = g.DistanceToEverywhere(0, includeStartNode: false, Relation.RDFSType).ToList();
            sw.Stop();

            result.Should().HaveCount(nodeCount - 1);
            sw.ElapsedMilliseconds.Should().BeLessThan(1_000,
                because: "distance search on a dense 100-node graph should be well under 1 s");
        }

        [TestMethod]
        public void AddThenRemoveStatements_LargeCount_LeavesGraphEmpty()
        {
            const int count = 10_000;
            var g = new Graph<int, Relation>();

            for (int i = 0; i < count; i++)
                g.AddStatement(i, Relation.RDFSType, i + 1);

            g.Edges.Count().Should().Be(count);

            for (int i = 0; i < count; i++)
                g.RemoveStatement(i, Relation.RDFSType, i + 1).Should().BeTrue();

            g.Edges.Should().BeEmpty();
            g.BackEdges.Should().BeEmpty();
        }

        [TestMethod]
        public void PageRank_LargeGraph_CompletesInReasonableTime()
        {
            // Star graph: one hub with 1 000 spokes
            const int spokes = 1_000;
            var g = new Graph<int, Relation>();
            for (int i = 1; i <= spokes; i++)
                g.AddStatement(i, Relation.RDFSType, 0);

            var sw = Stopwatch.StartNew();
            var ranks = g.PageRank(20).ToList();
            sw.Stop();

            ranks.Should().HaveCount(spokes + 1);
            sw.ElapsedMilliseconds.Should().BeLessThan(2_000,
                because: "PageRank on a 1 001-node star should complete well under 2 s");
        }

        [TestMethod]
        public void StartNodesAndEndNodes_LargeGraph_CorrectCounts()
        {
            // Linear chain: only node 0 is a start node, only node N-1 is an end node
            const int n = 5_000;
            var g = new Graph<int, Relation>();
            for (int i = 0; i < n - 1; i++)
                g.AddStatement(i, Relation.RDFSType, i + 1);

            g.StartNodes.Should().ContainSingle().Which.Should().Be(0);
            g.EndNodes.Should().ContainSingle().Which.Should().Be(n - 1);
        }
    }
}
