﻿using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace AboditUnitsTest
{
    [TestClass]
    public class GraphTests
    {
        private Graph<string, Relation> A;
        private Graph<string, Relation> B;
        private Graph<string, Relation> C;

        private static readonly string a = "a";
        private static readonly string b = "b";
        private static readonly string c = "c";
        private static readonly string d = "d";
        private static readonly string e = "e";
        private static readonly string f = "f";

        public GraphTests()
        {
            A = new Graph<string, Relation>();
            A.AddStatement(a, Relation.RDFSType, b);
            A.AddStatement(a, Relation.RDFSType, c);
            A.AddStatement(b, Relation.RDFSType, d);
            A.AddStatement(c, Relation.RDFSType, e);
            A.AddStatement(d, Relation.RDFSType, f);
            A.AddStatement(e, Relation.RDFSType, f);

            A.Edges.Count().Should().Be(6);
            ////Console.WriteLine("A");
            //foreach (var edge in A.Edges)
            //{
            //    Console.WriteLine("  " + edge);
            //}

            B = new Graph<string, Relation>();
            B.AddStatement(a, Relation.RDFSType, b);
            B.AddStatement(a, Relation.RDFSType, c);
            B.AddStatement(b, Relation.RDFSType, d);

            B.Edges.Count().Should().Be(3);
            //Console.WriteLine("B");
            //foreach (var edge in B.Edges)
            //{
            //    Console.WriteLine("  " + edge);
            //}

            C = new Graph<string, Relation>();
            C.AddStatement(a, Relation.RDFSType, f);

            C.Edges.Count().Should().Be(1);
            //Console.WriteLine("C");
            //foreach (var edge in C.Edges)
            //{
            //    Console.WriteLine("  " + edge);
            //}
        }

        [TestMethod]
        public void CanIntersectGraphs()
        {
            var AwithA = A.Intersect(A);
            var AwithB = A.Intersect(B);
            var BwithA = B.Intersect(A);
            var BwithC = B.Intersect(C);
            var CwithB = C.Intersect(B);

            AwithA.Nodes.Should().HaveCount(A.Nodes.Count(), "Intersection with self should preserve graph nodes");
            AwithA.Edges.Should().HaveCount(A.Edges.Count(), "Intersection with self should preserve graph edges");

            AwithB.Nodes.Should().HaveCount(B.Nodes.Count(), "B is a subgraph of A");
            BwithA.Nodes.Should().HaveCount(B.Nodes.Count(), "A is a supergraph of B");

            BwithC.Nodes.Should().HaveCount(0, "B is disjoint from C");
            CwithB.Nodes.Should().HaveCount(0, "C is disjoint from B");
        }

        [TestMethod]
        public void CanFollowEdge()
        {
            var follow = A.Follow(a, Relation.RDFSType);
            follow.Count().Should().Be(2);
            follow.Select(e => e.End).Should().BeEquivalentTo(new[] { b, c });
        }

        [TestMethod]
        public void CanFindSiblings()
        {
            var siblings = A.Siblings(b, Relation.RDFSType);
            siblings.Count().Should().Be(1);
            siblings.Should().BeEquivalentTo(new[] { c });
        }

        [TestMethod]
        public void UnionWithSelfIsIdentity()
        {
            var AwithA = A.Union(A);
            AwithA.Nodes.Should().HaveCount(A.Nodes.Count(), "Union with self should preserve graph nodes");
            AwithA.Edges.Should().HaveCount(A.Edges.Count(), "Union with self should preserve graph edges");
        }

        [TestMethod]
        public void UnionWithSubgraph()
        {
            var AwithB = A.Union(B);
            var BwithA = B.Union(A);
            AwithB.Nodes.Should().HaveCount(A.Nodes.Count(), "B is a subgraph of A");
            BwithA.Nodes.Should().HaveCount(A.Nodes.Count(), "A is a supergraph of B");
        }

        [TestMethod]
        public void UnionWithDisjoint()
        {
            var BwithC = B.Union(C);
            var CwithB = C.Union(B);
            BwithC.Nodes.Should().HaveCount(B.Nodes.Count() + C.Nodes.Count() - 1, "B is disjoint from C but shares one node");
            CwithB.Nodes.Should().HaveCount(B.Nodes.Count() + C.Nodes.Count() - 1, "C is disjoint from B but shares one node");

            BwithC.Edges.Should().HaveCount(B.Edges.Count() + C.Edges.Count(), "B is disjoint from C");
            CwithB.Edges.Should().HaveCount(B.Edges.Count() + C.Edges.Count(), "B is disjoint from C");
        }

        [TestMethod]
        public void TopoSortSimple()
        {
            var aSorted = string.Join("", A.TopologicalSortApprox());
            aSorted.Should().Be("abcdef");
        }

        [TestMethod]
        public void DistanceToEverywhere()
        {
            var distances = A.DistanceToEverywhere(a, true, Relation.RDFSType);
            string result = string.Join(";", distances.Select(d => d.Item1 + "=" + d.Item2));

            result.Should().Be("a=0;b=1;c=1;d=2;e=2;f=3");
        }

        [TestMethod]
        public void DistanceToEverywhereExcludeSelf()
        {
            var distances = A.DistanceToEverywhere(a, false, Relation.RDFSType);
            string result = string.Join(";", distances.Select(d => d.Item1 + "=" + d.Item2));

            result.Should().Be("b=1;c=1;d=2;e=2;f=3");
        }

        [TestMethod]
        public void ShortestPath()
        {
            var path = A.ShortestPath(a, Relation.RDFSType, e, (r) => 1.0, (x, y) => x + y);
            string result = string.Join(";", path!);
            result.Should().Be("a;c;e");
        }

        [TestMethod]
        public void NoShortestPath()
        {
            var path = A.ShortestPath(e, Relation.RDFSType, a, (r) => 1.0, (x, y) => x + y);
            path.Should().BeNull();
        }

        [TestMethod]
        public void Successors()
        {
            var successors = A.Successors<string>(c, Relation.RDFSType);
            string result = string.Join(";", successors.Edges.Select(e => e.End).OrderBy(x => x));
            result.Should().Be("e;f");
        }

        [TestMethod]
        public void Predecessors()
        {
            var predecessors = A.Predecessors<string>(c, Relation.RDFSType);
            string result = string.Join(";", predecessors.Edges.Select(e => e.Start).OrderBy(x => x));

            result.Should().Be("a");
        }

        /// <summary>
        /// Simple in-memory predicates
        /// </summary>
        private class Pred : IRelation, IEquatable<Pred>
        {
            public string Id { get; set; }

            public bool IsReflexive { get; set; }

            private Pred(string id)
            {
                this.Id = id;
            }

            public static Pred InverseOf = new Pred("inverseOf");
            public static Pred IsA = new Pred("isA");
            public static Pred Class = new Pred("class");
            public static Pred Reflexive = new Pred("reflexive");
            public static Pred Transitive = new Pred("transitive");

            public static Pred ChildOf = new Pred("childOf");
            public static Pred ParentOf = new Pred("parentOf");

            public static Pred AdjacentTo = new Pred("adjacentTo");
            public static Pred OpenTo = new Pred("openTo");
            public static Pred ConnectsTo = new Pred("connectsTo");
            public static Pred Controls = new Pred("controls");

            public static Pred HasDoor = new Pred("hasDoor");
            public static Pred Above = new Pred("above");
            public static Pred Below = new Pred("below");
            public static Pred In = new Pred("in");
            public static Pred Contains = new Pred("contains");
            public static Pred Domain = new Pred("domain");
            public static Pred Range = new Pred("range");

            public static Pred PartOf = new Pred("part Of");
            public static Pred ContainsPart = new Pred("contains part");

            public bool Equals(Pred? other) => other is Pred p && this.Id == p.Id;
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod]
        public void AddTest()
        {
            var g = new Graph<string, Pred>();

            g.AddStatement("one", Pred.ChildOf, "two");

            foreach (var relationship in g.Edges)
            {
                Debug.WriteLine("Debug " + relationship);
            }

            g.Follow("one").Should().HaveCount(1);

            g.Back("two").Should().HaveCount(1);

            g.Follow("one", Pred.ChildOf).Should().HaveCount(1);
            g.Follow("one", Pred.Below).Should().HaveCount(0);

            g.AddStatement("one", Pred.ChildOf, "three");

            g.Follow("one").Should().HaveCount(2);
            g.Back("two").Should().HaveCount(1);
            g.Back("three").Should().HaveCount(1);
        }

        private Graph<string, Pred> SimpleGraph()
        {
            var g = new Graph<string, Pred>();
            g.AddStatement("two", Pred.ChildOf, "one");
            g.AddStatement("one", Pred.ParentOf, "two");
            g.AddStatement("three", Pred.ChildOf, "two");
            g.AddStatement("two", Pred.ParentOf, "three");
            g.AddStatement("four", Pred.ChildOf, "three");
            g.AddStatement("three", Pred.ParentOf, "four");

            foreach (var relationship in g.Edges)
            {
                Debug.WriteLine("Debug " + relationship);
            }

            return g;
        }

        /// <summary>
        ///A test for Ancestors
        ///</summary>
        [TestMethod]
        public void BackAndFollowTests()
        {
            var g = SimpleGraph();
            g.Back("one").Should().HaveCount(1, "One has a single child");
            g.Back("two").Should().HaveCount(2, "Two has a parent (one) and a child (three)");
            g.Follow("two").Should().HaveCount(2, "Two has a parent (one) and a child (three)");
        }

        /// <summary>
        ///A test for Ancestors
        ///</summary>
        [TestMethod]
        public void AncestorsTest()
        {
            var g = SimpleGraph();
            var fourAnc = g.Predecessors<string>("four", Pred.ParentOf)
                .Edges.Select(x => x.Start)
                .ToList();
            fourAnc.Should().HaveCount(3, "Four has three ancestors");
        }

        /// <summary>
        ///A test for Ancestors
        ///</summary>
        [TestMethod]
        public void DescendantsTest()
        {
            var g = SimpleGraph();
            var oneDesc = g.Successors<string>("one", Pred.ParentOf)
                .Edges.Select(x => x.End)
                .ToList();      // one is the parent of three others
            oneDesc.Should().HaveCount(3, "One has three descendants");
        }

        /// <summary>
        ///A test for Paths
        ///</summary>
        [TestMethod]
        public void PathFindingTest()
        {
            var g = new Graph<string, Pred>();

            g.AddStatement("one", Pred.AdjacentTo, "two");
            g.AddStatement("two", Pred.AdjacentTo, "one");
            g.AddStatement("two", Pred.AdjacentTo, "three");
            g.AddStatement("three", Pred.AdjacentTo, "two");
            g.AddStatement("three", Pred.AdjacentTo, "four");
            g.AddStatement("four", Pred.AdjacentTo, "three");
            g.AddStatement("four", Pred.AdjacentTo, "one");
            g.AddStatement("one", Pred.AdjacentTo, "four");
            g.AddStatement("five", Pred.AdjacentTo, "three");
            g.AddStatement("three", Pred.AdjacentTo, "five");

            var distances = g.DistanceToEverywhere("one", true, Pred.AdjacentTo);
            foreach (var distance in distances)
            {
                Debug.WriteLine(distance.Item1.ToString() + " " + distance.Item2);
            }

            Debug.WriteLine("");
            Debug.WriteLine("Calculating path");

            var path = g.ShortestPath("one", Pred.AdjacentTo, "five", (r) => 1.0, (x, y) => x + y);

            if (path is null)
                Debug.WriteLine("No path");
            else
            {
                Debug.WriteLine("Found a path");
                foreach (var step in path)
                {
                    Debug.WriteLine("..." + step);
                }
            }

            path.Should().HaveCount(4, "Should be 4 steps");
        }

        /// <summary>
        ///A test for PageRank
        ///</summary>
        [TestMethod]
        public void PageRankTests()
        {
            var g = new Graph<string, Pred>();

            g.AddStatement("five", Pred.Above, "four");
            g.AddStatement("five", Pred.Above, "three");
            g.AddStatement("four", Pred.Above, "two");
            g.AddStatement("four", Pred.AdjacentTo, "one");
            g.AddStatement("three", Pred.AdjacentTo, "one");
            g.AddStatement("five", Pred.AdjacentTo, "one");

            var pageRank = g.PageRank(20, 0.85).ToList();

            //foreach (var r in pageRank)
            //{
            //    Console.WriteLine($"// {r.Item1:0.00} {r.Item2}");
            //}
            // 0.12 five
            // 0.16 three
            // 0.16 four
            // 0.19 two
            // 0.36 one

            pageRank.Sum(x => x.rank).Should().BeApproximately(1.0, 0.01, "page rank should add to 1");
        }
    }
}