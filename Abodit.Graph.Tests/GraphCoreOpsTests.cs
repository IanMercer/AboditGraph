using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AboditUnitsTest
{
    [TestClass]
    public class GraphCoreOpsTests
    {
        // -------------------------------------------------------------------------
        // Self-loop rejection
        // -------------------------------------------------------------------------

        [TestMethod]
        public void AddStatement_SelfLoop_ReturnsFalseAndAddsNoEdge()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "A").Should().BeFalse();
            g.Edges.Should().BeEmpty();
        }

        // -------------------------------------------------------------------------
        // Reflexive (bidirectional) edges
        // -------------------------------------------------------------------------

        [TestMethod]
        public void AddStatement_ReflexiveRelation_CreatesBothDirections()
        {
            var g = new Graph<string, Relation>();
            // Synonym is reflexive
            g.AddStatement("cat", Relation.Synonym, "feline");

            // Forward: cat --synonym--> feline
            g.Follow("cat", Relation.Synonym).Select(e => e.End)
                .Should().ContainSingle("feline");

            // Backward index: feline <--synonym-- cat
            g.Back("feline", Relation.Synonym).Select(e => e.Start)
                .Should().ContainSingle("cat");

            // Reverse edge also queryable forward: feline --synonym--> cat
            g.Follow("feline", Relation.Synonym).Select(e => e.End)
                .Should().ContainSingle("cat");
        }

        [TestMethod]
        public void RemoveStatement_ReflexiveRelation_RemovesBothDirections()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("cat", Relation.Synonym, "feline");

            // Both directions exist
            g.Follow("cat", Relation.Synonym).Should().HaveCount(1);
            g.Follow("feline", Relation.Synonym).Should().HaveCount(1);

            g.RemoveStatement("cat", Relation.Synonym, "feline").Should().BeTrue();

            g.Follow("cat", Relation.Synonym).Should().BeEmpty();
            g.Follow("feline", Relation.Synonym).Should().BeEmpty();
        }

        // -------------------------------------------------------------------------
        // Node removal
        // -------------------------------------------------------------------------

        [TestMethod]
        public void Remove_NodeWithIncomingAndOutgoing_RemovesAllEdges()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");
            g.AddStatement("B", Relation.RDFSType, "C");
            g.AddStatement("D", Relation.RDFSType, "B");

            g.Remove("B").Should().BeTrue();

            g.Edges.Should().BeEmpty();
            g.BackEdges.Should().BeEmpty();
            g.Nodes.Should().NotContain("B");
        }

        [TestMethod]
        public void Remove_NodeThatDoesNotExist_ReturnsFalse()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            g.Remove("Z").Should().BeFalse();
            g.Edges.Should().HaveCount(1);
        }

        // -------------------------------------------------------------------------
        // Empty graph
        // -------------------------------------------------------------------------

        [TestMethod]
        public void EmptyGraph_AllQueryMethods_ReturnEmptyWithoutException()
        {
            var g = new Graph<string, Relation>();

            g.Nodes.Should().BeEmpty();
            g.Edges.Should().BeEmpty();
            g.BackEdges.Should().BeEmpty();
            g.StartNodes.Should().BeEmpty();
            g.EndNodes.Should().BeEmpty();
            g.Follow("X").Should().BeEmpty();
            g.Follow("X", Relation.RDFSType).Should().BeEmpty();
            g.Back("X").Should().BeEmpty();
            g.Back("X", Relation.RDFSType).Should().BeEmpty();
            g.Siblings("X", Relation.RDFSType).Should().BeEmpty();
            g.Contains("X").Should().BeFalse();
        }

        [TestMethod]
        public void EmptyGraph_TopologicalSort_ReturnsEmpty()
        {
            var g = new Graph<string, Relation>();
            g.TopologicalSortApprox().Should().BeEmpty();
        }

        [TestMethod]
        public void EmptyGraph_PageRank_ReturnsEmpty()
        {
            var g = new Graph<string, Relation>();
            g.PageRank(10).Should().BeEmpty();
        }

        // -------------------------------------------------------------------------
        // Single edge graph
        // -------------------------------------------------------------------------

        [TestMethod]
        public void SingleEdgeGraph_QueryMethods_ReturnCorrectResults()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            g.Nodes.Should().BeEquivalentTo(new[] { "A", "B" });
            g.StartNodes.Should().ContainSingle("A");
            g.EndNodes.Should().ContainSingle("B");
            g.Follow("A", Relation.RDFSType).Select(e => e.End).Should().ContainSingle("B");
            g.Back("B", Relation.RDFSType).Select(e => e.Start).Should().ContainSingle("A");
            g.Contains("A").Should().BeTrue();
            g.Contains("B").Should().BeTrue();
            g.Contains("Z").Should().BeFalse();
        }

        // -------------------------------------------------------------------------
        // RemoveStatement for non-existent edge
        // -------------------------------------------------------------------------

        [TestMethod]
        public void RemoveStatement_NonExistentEdge_ReturnsFalseAndGraphUnchanged()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B");

            g.RemoveStatement("A", Relation.RDFSType, "Z").Should().BeFalse();
            g.Edges.Should().HaveCount(1);
        }

        // -------------------------------------------------------------------------
        // Duplicate edge
        // -------------------------------------------------------------------------

        [TestMethod]
        public void AddStatement_DuplicateEdge_ReturnsFalseAndCountUnchanged()
        {
            var g = new Graph<string, Relation>();
            g.AddStatement("A", Relation.RDFSType, "B").Should().BeTrue();
            g.AddStatement("A", Relation.RDFSType, "B").Should().BeFalse();
            g.Edges.Should().HaveCount(1);
        }
    }
}
