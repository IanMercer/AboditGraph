using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abodit.Mutable;
using FluentAssertions;
using Abodit.Graph;

namespace Abodit.Units.Tests.Graph
{
    [TestFixture]
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

            public bool Equals(Node other) => this.Id.Equals(other.Id);

            public override string ToString() => this.Name;
        }

        //[Test]
        //public void EqualProbabilitiesPeopleTotals()
        //{
        //    var graph = new Mutable.Graph<Node, EdgeProbability>();
        //    Node a = "A";
        //    Node b = "B";
        //    Node c = "C";
        //    Node d = "D";
        //    Node e = "E";
        //    Node f = "F";
        //    Node g = "G";
        //    Node h = "H";

        //    graph.AddStatement(a, new EdgeProbability(0.2), c);
        //    graph.AddStatement(b, new EdgeProbability(1.0), d);

        //    graph.ForwardPropagate<Node, Person>();

        //    graph.AddStatement(c, new EdgeProbability(0.2), e);
        //    // graph.AddStatement(d, new EdgeProbability(1.0), e);
        //    graph.AddStatement(c, new EdgeProbability(0.2), f);
        //    graph.AddStatement(d, new EdgeProbability(1.0), f);

        //    graph.ForwardPropagate<Node, Person>();

        //    graph.AddStatement(e, new EdgeProbability(0.7), g);
        //    graph.AddStatement(f, new EdgeProbability(0.5), g);
        //    graph.AddStatement(e, new EdgeProbability(0.3), h);
        //    graph.AddStatement(f, new EdgeProbability(0.5), h);

        //    graph.ForwardPropagate<Node, Person>();

        //    Console.WriteLine(graph.DotGraph);
        //}

        [Test]
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
            var pathSets = graph.EveryPossibleNonOverlappingPath(x => x.Resistance, x => x[0], (x, y) => x * y, new ConsoleLogger("test"))
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

        [Test]
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

            var pathSets = graph.EveryPossibleNonOverlappingPath(x => x.Probability, x => x, (x, y) => (x * y), new ConsoleLogger("test"))
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

        private Random r = new Random(123);

        //[Test]
        //public void CanPropagateForwardSimplest()
        //{
        //    var graph = new Mutable.Graph<SampleNode, EdgeProbability>();
        //    var a = new SampleNode(new Person("A"));
        //    var b = new SampleNode(new Person("B"));
        //    var c = new SampleNode();

        //    graph.AddStatement(a, new EdgeProbability(0.5), c);
        //    graph.AddStatement(b, new EdgeProbability(0.5), c);

        //    graph.ForwardPropagate<SampleNode, Person>(new ConsoleLogger("null"));

        //    c.ProbabilitySet.ToString().Should().Be("A|B");
        //    c.ProbabilitySet.NodeProbabilities.Sum(x => x.Value).Should().Be(1);
        //}

        //[Test]
        //public void CanFindTracksOnALargeGraphNewAlgorithm()
        //{
        //    int pcount = 5;

        //    var graph = CreateLargeGraph(pcount, 20);

        //    Console.WriteLine(graph.DotGraph);

        //    graph.ForwardPropagate<SampleNode, Person>();

        //    Console.WriteLine(graph.DotGraph);

        //    var summed = graph.EndNodes.Aggregate(new ProbabilitySet<Person>(), (x, a) => x + a.ProbabilitySet);

        //    foreach (var s in summed.NodeProbabilities)
        //    {
        //        Console.WriteLine($"  {s.Key} : {s.Value}");
        //    }

        //    // yeah, ignore that for the moment
        //    //foreach (var s in summed.NodeProbabilities)
        //    //{
        //    //    s.Value.Should().BeApproximately(1.0, 0.00001, $"{s.Key} should sum back up to 1.0");
        //    //}

        //    Console.WriteLine(graph.DotGraph);

        //    // graph.CleanLowProbabilityLinks<SampleNode, Person>();

        //    //Console.WriteLine(graph.DotGraph);

        //    //summed = graph.EndNodes.Aggregate(new ProbabilitySet<Person>(), (x, a) => x + a.ProbabilitySet);

        //    //foreach (var s in summed.NodeProbabilities)
        //    //{
        //    //    Console.WriteLine($"  {s.Key} : {s.Value}");
        //    //}

        //    // TODO: NOT WORKING - NEED BACK PROPAGATION!

        //    //foreach (var s in summed.NodeProbabilities)
        //    //{
        //    //    s.Value.Should().BeApproximately(1.0, 0.00001, $"{s.Key} should sum back up to 1.0");
        //    //}

        //    //var result = new Graph<(int node, double cumulative), TrackEdge>();

        //    //for (int p = 0; p < pcount; p++)
        //    //{
        //    //    graph.PathToEverywhere(result, p, x => 1, x => x.Probability, (x, y) => (x * y), tolerance: 0.99);
        //    //}

        //    //var cleaned = result.Project(x => x.node, e => e);

        //    //cleaned.Edges.Count().Should().BeLessThan(graph.Edges.Count());

        //    ////    .Take(takeLimit)  // must limit before sort
        //    ////    .OrderByDescending(x => x.Aggregate(1.0, (a, b) => a * b.Score));

        //    ////int c = 0;
        //    ////foreach (var pathSet in pathSets.Take(10))
        //    ////{
        //    ////    Console.WriteLine($"\n\n{++c}. path average = {pathSet.Average(x => x.Score):0.000}");
        //    ////    foreach (var item in pathSet)
        //    ////    {
        //    ////        Console.WriteLine(item);
        //    ////    }
        //    ////}

        //    Console.WriteLine(graph.DotGraph);
        //}

        //[Test]
        //public void CanFindTracksOnALargeGraph()
        //{
        //    var graph = CreateLargeGraph(5, 50);

        //    Console.WriteLine(graph.DotGraph);

        //    int takeLimit = 50;

        //    var pathSets = graph.EveryPossibleNonOverlappingPath(x => x.Probability, x => x, (x, y) => (x * y), new ConsoleLogger("paths"))
        //        .Take(takeLimit)  // must limit before sort
        //        .OrderByDescending(x => x.Aggregate(1.0, (a, b) => a * b.Score));

        //    int c = 0;
        //    foreach (var pathSet in pathSets.Take(10))
        //    {
        //        Console.WriteLine($"\n\n{++c}. path average = {pathSet.Average(x => x.Score):0.000}");
        //        foreach (var item in pathSet)
        //        {
        //            Console.WriteLine(item);
        //        }
        //    }

        //    // And now prune the graph and dump it
        //    var allowedEdges = pathSets.SelectMany(x => x.SelectMany(y => y.Pairs))
        //        .Distinct().ToList();

        //    var allNodes = graph.Nodes;
        //    var allEdges = graph.Edges;

        //    foreach (var edge in allEdges)
        //    {
        //        if (allowedEdges.Any(e => e.previous == edge.Start && e.successor == edge.End))
        //        {
        //            // OK
        //        }
        //        else
        //        {
        //            graph.RemoveEdgesBetween(edge.Start, edge.End);
        //        }
        //    }

        //    Console.WriteLine(graph.DotGraph);

        //    pathSets.Count().Should().BeGreaterThan(4);
        //}

        private Mutable.Graph<SampleNode, EdgeProbability> CreateLargeGraph(int pCount, int levels)
        {
            var graph = new Mutable.Graph<SampleNode, EdgeProbability>();

            int loop = levels;
            var seen = new HashSet<(SampleNode from, SampleNode to)>();

            var people = Person.Instances.Take(pCount).ToList();
            var startNodes = people.Select(p => new SampleNode(p)).ToList();

            var middleNodes = Enumerable.Range(0, loop).Select(i => new SampleNode()).ToList();

            var endNodes = Enumerable.Range(0, pCount).Select(i => new SampleNode()).ToList();

            var allNodes = startNodes.Concat(middleNodes).Concat(endNodes).ToList();

            bool running = true;

            while (running)
            {
                var shuffle = middleNodes.OrderBy(x => r.NextDouble())
                    .Select((middle, i) => (a: middle, g: r.Next(pCount)))
                    .ToList();

                var split = shuffle.GroupBy(x => x.g);

                foreach (var run in startNodes.Zip(endNodes, (s, e) => (s, e)).Zip(split, (p, s) => (start: p.s, end: p.e, items: s)))
                {
                    var pos = run.start;
                    var items = run.items.OrderBy(x => x.a.Id);

                    foreach (var item in items)
                    {
                        if (seen.Contains((pos, item.a))) continue;
                        seen.Add((pos, item.a));
                        graph.AddStatement(pos, new EdgeProbability(r.NextDouble()), item.a);
                        pos = item.a;
                    }
                    var next = run.end;
                    if (seen.Contains((pos, next))) continue;
                    seen.Add((pos, next));
                    graph.AddStatement(pos, new EdgeProbability(r.NextDouble() * r.NextDouble()), next);
                }

                // If any node in the middle doesn't have a 2+ successors we are not done
                running = middleNodes
                    .Select(x => seen.Where(s => s.from.Id == x.Id).Count())
                    .Any(x => x < 2);
            }

            return graph;
        }
    }
}