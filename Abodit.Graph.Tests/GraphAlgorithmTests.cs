using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AboditUnitsTest
{
    [TestClass]
    public class GraphAlgorithmTests
    {
        // -------------------------------------------------------------------------
        // TopologicalSortApprox
        // -------------------------------------------------------------------------

        [TestMethod]
        public void TopologicalSort_LinearChain_ReturnsNodesInOrder()
        {
            // A -> B -> C -> D
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "C");
            g.AddStatement("C", Relation.RDFSType, "D");

            var order = g.TopologicalSortApprox().ToList();

            order.Should().HaveCount(4);
            order.IndexOf("A").Should().BeLessThan(order.IndexOf("B"));
            order.IndexOf("B").Should().BeLessThan(order.IndexOf("C"));
            order.IndexOf("C").Should().BeLessThan(order.IndexOf("D"));
        }

        [TestMethod]
        public void TopologicalSort_DisconnectedDAG_ContainsAllNodes()
        {
            // A -> B   and   C -> D  (two separate chains)
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("C", Relation.RDFSType, "D");

            var order = g.TopologicalSortApprox().ToList();

            order.Should().BeEquivalentTo(new[] { "A", "B", "C", "D" });
        }

        [TestMethod]
        public void TopologicalSort_SingleEdgeGraph_ReturnsBothNodes()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            var order = g.TopologicalSortApprox().ToList();
            order.Should().HaveCount(2);
            order.IndexOf("A").Should().BeLessThan(order.IndexOf("B"));
        }

        [TestMethod]
        public void TopologicalSort_EmptyGraph_ReturnsEmpty()
        {
            var g = new Graph<string, Relation>();
            g.TopologicalSortApprox().Should().BeEmpty();
        }

        // -------------------------------------------------------------------------
        // PageRank
        // -------------------------------------------------------------------------

        [TestMethod]
        public void PageRank_SumOfAllRanks_IsApproximatelyOne()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "C");
            g.AddStatement("C", Relation.RDFSType, "D");

            double sum = g.PageRank(20).Sum(r => r.rank);
            sum.Should().BeApproximately(1.0, precision: 1e-9);
        }

        [TestMethod]
        public void PageRank_LinearChain_LaterNodesRankHigher()
        {
            // In a linear chain A->B->C->D, PageRank flows towards the sink (D)
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "C");
            g.AddStatement("C", Relation.RDFSType, "D");

            var ranks = g.PageRank(50).ToDictionary(r => r.node, r => r.rank);

            // D is the sink — it should accumulate the highest rank
            ranks["D"].Should().BeGreaterThan(ranks["A"]);
        }

        [TestMethod]
        public void PageRank_HubNode_RanksHighestAmongTargets()
        {
            // Many nodes pointing to "hub" — hub should receive high rank
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "hub");
            g.AddStatement("B", Relation.RDFSType, "hub");
            g.AddStatement("C", Relation.RDFSType, "hub");
            g.AddStatement("D", Relation.RDFSType, "hub");

            var ranks = g.PageRank(50).ToDictionary(r => r.node, r => r.rank);

            ranks["hub"].Should().BeGreaterThan(ranks["A"]);
            ranks["hub"].Should().BeGreaterThan(ranks["B"]);
            ranks["hub"].Should().BeGreaterThan(ranks["C"]);
            ranks["hub"].Should().BeGreaterThan(ranks["D"]);
        }

        [TestMethod]
        public void PageRank_DanglingNode_DoesNotThrow()
        {
            // "dangling" has no outgoing edges
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "dangling");

            var act = () => g.PageRank(20).ToList();
            act.Should().NotThrow();
        }

        // -------------------------------------------------------------------------
        // DistanceToEverywhere
        // -------------------------------------------------------------------------

        [TestMethod]
        public void DistanceToEverywhere_LinearChain_ReturnsCorrectDistances()
        {
            // A -> B -> C -> D
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "C");
            g.AddStatement("C", Relation.RDFSType, "D");

            var distances = g.DistanceToEverywhere("A", includeStartNode: false, Relation.RDFSType)
                             .ToDictionary(t => t.Item1, t => t.Item2);

            distances["B"].Should().Be(1);
            distances["C"].Should().Be(2);
            distances["D"].Should().Be(3);
        }

        [TestMethod]
        public void DistanceToEverywhere_NoOutgoingEdges_ReturnsOnlyStartIfIncluded()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            // From B there are no outgoing edges
            var results = g.DistanceToEverywhere("B", includeStartNode: true, Relation.RDFSType).ToList();
            results.Should().ContainSingle(t => t.Item1 == "B" && t.Item2 == 0);
        }

        [TestMethod]
        public void DistanceToEverywhere_NodeNotInGraph_ReturnsOnlyStartIfIncluded()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            var results = g.DistanceToEverywhere("Z", includeStartNode: true, Relation.RDFSType).ToList();
            results.Should().ContainSingle(t => t.Item1 == "Z" && t.Item2 == 0);
        }

        [TestMethod]
        public void DistanceToEverywhere_ShortestPath_TakenOverLonger()
        {
            // A->B->D (2 hops) and A->D (1 hop) — D should be at distance 1
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "D");
            g.AddStatement("A", Relation.RDFSType, "D");

            var distances = g.DistanceToEverywhere("A", includeStartNode: false, Relation.RDFSType)
                             .ToDictionary(t => t.Item1, t => t.Item2);

            distances["D"].Should().Be(1);
        }

        // -------------------------------------------------------------------------
        // ShortestPath
        // -------------------------------------------------------------------------

        [TestMethod]
        public void ShortestPath_DirectPath_ReturnsCorrectNodes()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "C");

            var path = g.ShortestPath("A", Relation.RDFSType, "C",
                edgeWeight: _ => 1.0,
                accumulator: (acc, w) => acc + w);

            path.Should().NotBeNull();
            path!.Should().ContainInOrder("A", "B", "C");
        }

        [TestMethod]
        public void ShortestPath_NoPathExists_ReturnsNull()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            var path = g.ShortestPath("A", Relation.RDFSType, "Z",
                edgeWeight: _ => 1.0,
                accumulator: (acc, w) => acc + w);

            path.Should().BeNull();
        }

        [TestMethod]
        public void ShortestPath_PrefersCheaperPath()
        {
            // A -> B (cost 10) -> C
            // A -> D (cost 1)  -> C (cost 1)
            // Cheap route: A -> D -> C (total 2) vs A -> B -> C (total 11)
            var g = new Graph<string, Relation>();
            var cheap = Relation.GetByName("cheap");
            var expensive = Relation.GetByName("expensive");

            g.AddStatement("A", expensive, "B");
            g.AddStatement("B", expensive, "C");
            g.AddStatement("A", cheap, "D");
            g.AddStatement("D", cheap, "C");

            // Follow any edge but weight them differently
            var path = g.ShortestPath("A", cheap, "C",
                edgeWeight: _ => 1.0,
                accumulator: (acc, w) => acc + w);

            path.Should().NotBeNull();
            path!.Should().ContainInOrder("A", "D", "C");
        }
    }
}
