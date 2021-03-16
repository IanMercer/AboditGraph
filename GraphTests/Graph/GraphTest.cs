using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abodit.Immutable;
using FluentAssertions;
using Abodit.Graph;

namespace Abodit.Units.Tests.Graph
{
    [TestFixture]
    public class GraphTest
    {
        [Test]
        public void CanAddAndRemoveOnImmutableGraph()
        {
            var graph = new Abodit.Immutable.Graph<string, Relation>();

            graph = graph.AddStatement("A", Relation.Implies, "B");
            graph = graph.AddStatement("A", Relation.Implies, "C");
            graph = graph.AddStatement("B", Relation.Implies, "C");

            graph.Edges.Should().HaveCount(3);

            graph = graph.RemoveStatement("A", Relation.Implies, "A");
            graph.Edges.Should().HaveCount(3);
            graph = graph.RemoveStatement("A", Relation.Implies, "B");
            graph.Edges.Should().HaveCount(2);
            graph = graph.RemoveStatement("B", Relation.Implies, "C");
            graph.Edges.Should().HaveCount(1);
            graph = graph.RemoveStatement("A", Relation.Implies, "C");
            graph.Edges.Should().HaveCount(0);
        }

        [Test]
        public void CanAddAndRemoveOnMutableGraph()
        {
            var graph = new Abodit.Mutable.Graph<string, Relation>();

            graph.AddStatement("A", Relation.Implies, "B");
            graph.AddStatement("A", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Implies, "C");

            graph.Edges.Should().HaveCount(3);

            graph.RemoveStatement("A", Relation.Implies, "A");
            graph.Edges.Should().HaveCount(3);
            graph.RemoveStatement("A", Relation.Implies, "B");
            graph.Edges.Should().HaveCount(2);
            graph.RemoveStatement("B", Relation.Implies, "C");
            graph.Edges.Should().HaveCount(1);
            graph.RemoveStatement("A", Relation.Implies, "C");
            graph.Edges.Should().HaveCount(0);
        }

        [Test]
        public void CanRemovePairsFromMutableGraph()
        {
            var graph = new Mutable.Graph<string, Relation>();

            graph.AddStatement("A", Relation.Implies, "B");
            graph.AddStatement("A", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Related, "C");

            graph.Edges.Should().HaveCount(4);
            graph.BackEdges.Should().HaveCount(4);

            graph.RemoveEdgesBetween("A", "D").Should().Be(0);
            graph.Edges.Should().HaveCount(4);
            graph.BackEdges.Should().HaveCount(4);

            graph.RemoveEdgesBetween("A", "C").Should().Be(2);
            graph.Edges.Should().HaveCount(3);
            graph.BackEdges.Should().HaveCount(3);

            graph.RemoveEdgesBetween("B", "C").Should().Be(4);
            graph.Edges.Should().HaveCount(1);
            graph.BackEdges.Should().HaveCount(1);
        }

        [Test]
        public void CanRemoveStatementsFromMutableGraph()
        {
            var graph = new Mutable.Graph<string, Relation>();

            graph.AddStatement("A", Relation.Implies, "B");
            graph.AddStatement("A", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Related, "C");

            graph.Edges.Should().HaveCount(4);
            graph.BackEdges.Should().HaveCount(4);

            graph.RemoveStatement("A", Relation.Implies, "D").Should().Be(false);
            graph.Edges.Should().HaveCount(4);
            graph.BackEdges.Should().HaveCount(4);

            graph.RemoveStatement("A", Relation.Implies, "C").Should().Be(true);
            graph.Edges.Should().HaveCount(3);
            graph.BackEdges.Should().HaveCount(3);

            graph.RemoveStatement("B", Relation.Related, "C").Should().Be(true);
            graph.Edges.Should().HaveCount(2);
            graph.BackEdges.Should().HaveCount(2);

            graph.RemoveStatement("B", Relation.Implies, "C").Should().Be(true);
            graph.Edges.Should().HaveCount(1);
            graph.BackEdges.Should().HaveCount(1);

            graph.RemoveStatement("A", Relation.Implies, "B").Should().Be(true);
            graph.Edges.Should().HaveCount(0);
            graph.BackEdges.Should().HaveCount(0);
        }

        [Test]
        public void CanRemoveANodeFromMutableGraph()
        {
            var graph = new Mutable.Graph<string, Relation>();

            graph.AddStatement("A", Relation.Implies, "B");
            graph.AddStatement("A", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Related, "C");
            graph.AddStatement("D", Relation.Implies, "A");

            graph.Edges.Should().HaveCount(5);
            graph.BackEdges.Should().HaveCount(5);

            graph.Remove("A").Should().BeTrue();

            graph.Edges.Should().HaveCount(2);
            graph.BackEdges.Should().HaveCount(2);
            graph.Nodes.Should().HaveCount(2);
        }

        [Test]
        public void CanRemoveEdgeInMutableGraph()
        {
            var graph = new Mutable.Graph<string, Relation>();

            graph.AddStatement("A", Relation.Implies, "B");
            graph.AddStatement("A", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Related, "C");
            graph.AddStatement("D", Relation.Implies, "A");

            graph.Edges.Should().HaveCount(5);

            graph.RemoveStatement("A", Relation.Implies, "B").Should().BeTrue();
            graph.RemoveStatement("A", Relation.Implies, "D").Should().BeFalse();

            graph.Edges.Should().HaveCount(4);
            graph.Nodes.Should().HaveCount(4);
        }

        [Test]
        public void CanReplaceInMutableGraph()
        {
            var graph = new Mutable.Graph<string, Relation>();

            graph.AddStatement("A", Relation.Implies, "B");
            graph.AddStatement("A", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Implies, "C");
            graph.AddStatement("B", Relation.Related, "C");
            graph.AddStatement("D", Relation.Implies, "A");

            graph.Edges.Should().HaveCount(5);

            graph.ReplaceEdge("A", "B", Relation.Related).Should().Be(true);

            graph.Edges.Should().HaveCount(5);
            graph.Nodes.Should().HaveCount(4);
        }

        public class Relation : IRelation, IEquatable<Relation>
        {
            public static Relation Implies = new Relation("Implies");
            public static Relation Related = new Relation("Related");

            private Relation(string name)
            {
                Name = name;
            }

            public bool IsReflexive => false;

            public string Name { get; }

            public bool Equals(Relation other)
            {
                return this.Name.Equals(other.Name);
            }

            public override string ToString() => this.Name;
        }
    }
}