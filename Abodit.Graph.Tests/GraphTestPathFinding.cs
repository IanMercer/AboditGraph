using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Abodit.Units.Tests.Graph
{
    [TestClass]
    public partial class GraphTestPathFinding
    {
        private class Node : IEquatable<Node>, INodeProbability<Person>, IDotGraphNode
        {
            private ProbabilitySet<Person> probabilitySet;

            public string Name { get; }

            public int N => 1;

            public bool IsPruned => false;

            public bool IsStartNode => false;

            public ProbabilitySet<Person> ProbabilitySet => this.probabilitySet;

            private static int id = 0;

            public int Id { get; }

            public string DotProperties => $"[label=\"{this.Id}\\n{this.ProbabilitySet.DotLabel}\"]";

            public Node(string name)
            {
                this.Id = ++id;
                this.Name = name;
                this.probabilitySet = new ProbabilitySet<Person>(new Person(name));
            }

            public static implicit operator Node(string v) => new Node(v);

            public bool Equals(Node? other) => this.Id.Equals(other?.Id);

            public override string ToString() => this.Name;
        }

        [TestMethod]
        public void CanFindNonOverlappingPaths()
        {
            var graph = new Graph<string, Impedance>();

            graph.AddStatement("A", new Impedance(20), "B");
            graph.AddStatement("A", new Impedance(10), "D");
            graph.AddStatement("A", new Impedance(30), "F");
            graph.AddStatement("B", new Impedance(7), "C");
            graph.AddStatement("B", new Impedance(15), "E");
            graph.AddStatement("D", new Impedance(40), "E");
            graph.AddStatement("D", new Impedance(42), "G");
            graph.AddStatement("F", new Impedance(19), "G");
            graph.AddStatement("F", new Impedance(5), "E");

            graph.AddStatement("X", new Impedance(12), "B");
            graph.AddStatement("X", new Impedance(9), "D");

            graph.AddStatement("Y", new Impedance(17), "D");
            graph.AddStatement("Y", new Impedance(3), "F");

            // THREE INPUTS, THREE OUTPUTS

            // use first character for order
            var pathSets = graph.EveryPossibleNonOverlappingPath(x => x.Resistance, x => x[0], (x, y) => x * y, NullLogger.Instance)
                .OrderBy(x => x.Aggregate(1.0, (a, b) => a * b.Score));

            int c = 0;
            foreach (var pathSet in pathSets)
            {
                Console.WriteLine($"\n\n{++c}. path average = {pathSet.Aggregate(1.0, (a, b) => a + b.Score):0.000}");
                foreach (var item in pathSet)
                {
                    Console.WriteLine(item);
                }
            }

            pathSets.Count().Should().BeGreaterThan(2);
        }

        [TestMethod]
        public void CanFindTracksOnHASample()
        {
            var graph = new Graph<int, EdgeProbability>();

            graph.AddStatement(1, new EdgeProbability(0.8), 2);
            graph.AddStatement(1, new EdgeProbability(0.2), 3);
            graph.AddStatement(2, new EdgeProbability(0.7), 3);
            graph.AddStatement(2, new EdgeProbability(0.3), 4);
            graph.AddStatement(3, new EdgeProbability(0.5), 4);
            graph.AddStatement(3, new EdgeProbability(0.5), 5);
            graph.AddStatement(4, new EdgeProbability(1.0), 5);

            var pathSets = graph.EveryPossibleNonOverlappingPath(x => x.Probability, x => x, (x, y) => (x * y), NullLogger.Instance)
                    .OrderBy(x => x.Aggregate(1.0, (a, b) => a * b.Score));

            int c = 0;
            foreach (var pathSet in pathSets)
            {
                Console.WriteLine($"\n\n{++c}. path average = {pathSet.Aggregate(1.0, (a, b) => a * b.Score):0.000}");
                foreach (var item in pathSet)
                {
                    Console.WriteLine(item);
                }
            }

            pathSets.Count().Should().BeGreaterOrEqualTo(1);
        }
    }
}