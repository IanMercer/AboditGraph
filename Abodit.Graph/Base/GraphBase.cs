using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph.Base
{
    /// <summary>
    /// Base class for <see cref="Abodit.Immutable.Graph{TNode, TRelation}"/> and <see cref="Abodit.Mutable.Graph{TNode, TRelation}"/>
    /// </summary>
    /// <remarks>
    /// Shares nodes with other graphs. Edges are stored as linked lists indexed by both start node and end node.
    /// </remarks>
    public abstract partial class GraphBase<TNode, TRelation> : IEnumerable<TNode>
        where TNode : IEquatable<TNode>
        where TRelation : notnull, IEquatable<TRelation>
    {
        /// <summary>
        /// Limit the number of edges that can be attached to any node
        /// </summary>
        public int Limit { get; set; } = 100000;

        /// <summary>
        /// Create a new empty graph
        /// </summary>
        protected GraphBase()
        {
        }

        /// <summary>
        /// Create a new <see cref="GraphBase{TNode, TRelation}"/>
        /// </summary>
        protected GraphBase(ConcurrentDictionary<TNode, PredicateNext> startIndexedEdges, ConcurrentDictionary<TNode, PredicatePrevious> endIndexedEdges)
        {
            this.StartIndexedEdges = startIndexedEdges;
            this.EndIndexedEdges = endIndexedEdges;
        }

        /// <summary>
        /// Edges arranged by start node
        /// </summary>
        protected readonly ConcurrentDictionary<TNode, PredicateNext> StartIndexedEdges = new ConcurrentDictionary<TNode, PredicateNext>();

        /// <summary>
        /// Edges arrange by end node
        /// </summary>
        protected readonly ConcurrentDictionary<TNode, PredicatePrevious> EndIndexedEdges = new ConcurrentDictionary<TNode, PredicatePrevious>();

        /// <summary>
        /// Enumerate all the nodes
        /// </summary>
        public IEnumerable<TNode> Nodes
        {
            get
            {
                // Every node in the graph must be either a start node or an end node or it wouldn't be here
                return this.StartIndexedEdges.Select(x => x.Key).Concat(this.EndIndexedEdges.Select(x => x.Key)).Distinct();
            }
        }

        /// <summary>
        /// Enumerate all the nodes with no incoming edges (roots of the graph).
        /// </summary>
        /// <remarks>
        /// Computed in O(n) using a <see cref="HashSet{T}"/> rather than LINQ <c>Except</c>,
        /// which would be O(n²) for large graphs.
        /// </remarks>
        public IEnumerable<TNode> StartNodes
        {
            get
            {
                var endSet = new HashSet<TNode>(this.EndIndexedEdges.Keys);
                return this.StartIndexedEdges.Keys.Where(k => !endSet.Contains(k));
            }
        }

        /// <summary>
        /// Enumerate all the nodes with no outgoing edges (leaves of the graph).
        /// </summary>
        /// <remarks>
        /// Computed in O(n) using a <see cref="HashSet{T}"/> rather than LINQ <c>Except</c>,
        /// which would be O(n²) for large graphs.
        /// </remarks>
        public IEnumerable<TNode> EndNodes
        {
            get
            {
                var startSet = new HashSet<TNode>(this.StartIndexedEdges.Keys);
                return this.EndIndexedEdges.Keys.Where(k => !startSet.Contains(k));
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the graph contains <paramref name="node"/> as either
        /// the start or end of at least one edge.
        /// </summary>
        public bool Contains(TNode node) => this.StartIndexedEdges.Any(x => x.Key.Equals(node)) || this.EndIndexedEdges.Any(x => x.Key.Equals(node));

        /// <summary>
        /// Does the graph contain a matching node?
        /// </summary>
        public bool Any(TNode node, Func<TNode, bool> predicate) =>
            this.StartIndexedEdges.Any(x => predicate(x.Key)) || this.EndIndexedEdges.Any(x => predicate(x.Key));

        /// <summary>
        /// Examine every node in the graph for ones of type T
        /// </summary>
        public IEnumerable<T> GetNodes<T>() where T : TNode => this.Nodes.OfType<T>();

        /// <summary>
        /// Get all the edges of the graph
        /// </summary>
        public IEnumerable<Edge> Edges
        {
            get
            {
                return this.StartIndexedEdges
                    .SelectMany(e => e.Value.Select(pn => new Edge(e.Key, pn.Predicate, pn.End)));
            }
        }

        /// <summary>
        /// Get edges going forward grouped by node
        /// </summary>
        public IEnumerable<IGrouping<TNode, Edge>> ForwardEdgesByNode
        {
            get
            {
                return this.StartIndexedEdges.Select(pn =>
                    new GroupedEdge(pn.Key, pn.Value.Select(e => new Edge(pn.Key, e.Predicate, e.End))));
            }
        }

        /// <summary>
        /// Get edges going backward grouped by node
        /// </summary>
        public IEnumerable<IGrouping<TNode, Edge>> BackwardEdgesByNode
        {
            get
            {
                return this.EndIndexedEdges.Select(pn =>
                    new GroupedEdge(pn.Key, pn.Value.Select(e => new Edge(e.Start, e.Predicate, pn.Key))));
            }
        }

        private class GroupedEdge : IGrouping<TNode, Edge>
        {
            private readonly TNode node;
            private readonly IEnumerable<Edge> edge;

            public GroupedEdge(TNode node, IEnumerable<Edge> edge)
            {
                this.node = node;
                this.edge = edge;
            }

            public TNode Key => this.node;

            public IEnumerator<Edge> GetEnumerator() => this.edge.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.edge.GetEnumerator();
        }

        /// <summary>
        /// Get all the back edges of the graph (for testing)
        /// </summary>
        public IEnumerable<Edge> BackEdges
        {
            get
            {
                return this.EndIndexedEdges
                    .SelectMany(e => e.Value.Select(pn => new Edge(pn.Start, pn.Predicate, e.Key)));
            }
        }

        /// <summary>
        /// Find all the outgoing edges from a vertex
        /// </summary>
        /// <remarks>
        /// A single step in the graph away from a node
        /// </remarks>
        public IEnumerable<Edge> Follow(TNode start)
        {
            if (StartIndexedEdges.TryGetValue(start, out PredicateNext? startLink))
            {
                foreach (var pn in startLink!)
                {
                    yield return new Edge(start, pn.Predicate, pn.End);
                }
            }
        }

        /// <summary>
        /// Find all the outgoing edges from a vertex that match at least one of the listed predicates
        /// </summary>
        /// <remarks>
        /// A single step in the graph away from a node
        /// </remarks>
        public IEnumerable<Edge> Follow(TNode start, params TRelation[] predicates)
        {
            int safetyLimit = Limit;
            if (StartIndexedEdges.TryGetValue(start, out PredicateNext? startLink))
            {
                foreach (var pn in startLink!.Where(pn => (!predicates.Any()) || predicates.Contains(pn.Predicate)))
                {
                    if (safetyLimit-- < 0) throw new Exception($"Too many edges, limit was {safetyLimit}");
                    yield return new Edge(start, pn.Predicate, pn.End);
                }
            }
        }

        /// <summary>
        /// Find all the outgoing edges from a vertex that match a condition on the edge
        /// </summary>
        /// <remarks>
        /// A single step in the graph away from a node
        /// </remarks>
        public IEnumerable<Edge> Follow(TNode start, Func<TNode, TRelation, TNode, bool> condition)
        {
            if (StartIndexedEdges.TryGetValue(start, out PredicateNext? startLink))
            {
                foreach (var pn in startLink!.Where(pn => condition(start, pn.Predicate, pn.End)))
                {
                    yield return new Edge(start, pn.Predicate, pn.End);
                }
            }
        }

        /// <summary>
        /// Returns all siblings of <paramref name="child"/>: nodes that share at least one parent
        /// reachable by following the given <paramref name="predicates"/> backward from
        /// <paramref name="child"/> and then forward to the parent's other children.
        /// </summary>
        /// <remarks>
        /// A "sibling" is any node <c>S ≠ child</c> such that there exists a node <c>P</c> with
        /// edges <c>P → child</c> and <c>P → S</c> (both via one of the supplied predicates).
        /// If no predicates are supplied all edge types are followed.
        /// The same sibling may be returned more than once if it shares multiple parents with
        /// <paramref name="child"/>.
        /// </remarks>
        public IEnumerable<TNode> Siblings(TNode child, params TRelation[] predicates)
        {
            HashSet<TNode> seen = new HashSet<TNode>();
            if (EndIndexedEdges.TryGetValue(child, out PredicatePrevious? endLink))
            {
                int safetyLimit = Limit;
                foreach (var priorEdge in endLink!)
                {
                    if (StartIndexedEdges.TryGetValue(priorEdge.Start, out PredicateNext? startLink))  // always
                    {
                        foreach (var pn2 in startLink!.Where(pn => (!predicates.Any()) || predicates.Contains(pn.Predicate)))
                        {
                            if (safetyLimit-- < 0) throw new Exception($"Too many edges, limit was {safetyLimit}");
                            if (pn2.End.Equals(child)) continue;  // not a sibling of self

                            yield return pn2.End;
                        }

                        if (safetyLimit-- < 0) throw new Exception($"Too many edges, limit was {safetyLimit}");
                    }
                }
            }
        }

        /// <summary>
        /// Find all the outgoing edges from a vertex
        /// </summary>
        public IEnumerable<Edge> Back(TNode end)
        {
            if (EndIndexedEdges.TryGetValue(end, out PredicatePrevious? endLink))
            {
                foreach (var pn in endLink!)
                {
                    yield return new Edge(pn.Start, pn.Predicate, end);
                }
            }
        }

        /// <summary>
        /// Find all the outgoing edges from a vertex with a given predicate (or null)
        /// </summary>
        public IEnumerable<Edge> Back(TNode end, params TRelation[] predicates)
        {
            if (EndIndexedEdges.TryGetValue(end, out PredicatePrevious? endLink))
            {
                int safetyLimit = Limit;
                foreach (var pn in endLink!.Where(pn => (!predicates.Any()) || predicates.Contains(pn.Predicate)))
                {
                    if (safetyLimit-- < 0) throw new Exception($"Too many edges, limit was {safetyLimit}");
                    yield return new Edge(pn.Start, pn.Predicate, end);
                }
            }
        }

        /// <summary>
        /// Find all the outgoing edges from a vertex that match a condition on the edge
        /// </summary>
        public IEnumerable<Edge> Back(TNode end, Func<TNode, TRelation, TNode, bool> condition)
        {
            if (EndIndexedEdges.TryGetValue(end, out PredicatePrevious? endLink))
            {
                foreach (var pn in endLink!.Where(pn => condition(pn.Start, pn.Predicate, end)))
                {
                    yield return new Edge(pn.Start, pn.Predicate, end);
                }
            }
        }

        /// <summary>
        /// Perform a search of the graph following a given predicate over nodes of a given type
        /// </summary>
        public IEnumerable<T> Search<T>(TNode start, ISearchOrder<TNode> stack, TRelation predicate = default!) where T : class, TNode
        {
            var visited = new HashSet<TNode>();
            stack.Enqueue(start);

            while (stack.Count > 0)
            {
                start = stack.Dequeue();

                if (start is T starttt)
                    yield return starttt;

                var outgoing = this.Follow(start, predicate!);

                foreach (var edge in outgoing)
                {
                    if (!(edge.End is T outEnd)) continue;

                    if (!visited.Contains(outEnd))
                    {
                        stack.Enqueue(outEnd);
                        visited.Add(outEnd);
                        yield return outEnd;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an approximate topological ordering of all reachable nodes using Kahn's algorithm.
        /// </summary>
        /// <remarks>
        /// The "Approx" qualifier means that cycles are silently tolerated: nodes involved in a cycle
        /// are simply not visited (they never become start-nodes) and are omitted from the result.
        /// For a strict DAG the result is a valid topological order.  For a graph with cycles it is a
        /// best-effort partial order that excludes the cyclic nodes.
        /// </remarks>
        public IEnumerable<TNode> TopologicalSortApprox()
        {
            var nodesWithNoIncomingLinks = this.StartNodes.ToList();

            // L ← Empty list that will contain the sorted elements
            var l = new List<TNode>();
            //S ← Set of all nodes with no incoming edges
            var s = new Queue<TNode>(nodesWithNoIncomingLinks);

            var visited = new HashSet<TNode>(nodesWithNoIncomingLinks);

            //while S is non - empty do
            while (s.Count > 0)
            {
                //                    remove a node n from S
                var n = s.Dequeue();
                //                    add n to tail of L
                l.Add(n);
                //    for each node m with an edge e from n to m do
                foreach (var e in this.Follow(n))
                {
                    if (visited.Contains(e.End))
                        continue;
                    //                remove edge e from the graph
                    //                if m has no other incoming edges then
                    //                insert m into S
                    s.Enqueue(e.End);
                    visited.Add(e.End);
                }
            }
            //if graph has edges then
            //    return error(graph has at least one cycle)
            //else
            //    return L(a topologically sorted order)
            return l;
        }

        /// <summary>
        /// Computes PageRank for every node in the graph, assuming equal weighting on each edge type.
        /// Returns results in ascending rank order.
        /// </summary>
        /// <param name="iterations">
        /// Number of power-iteration steps to run.  More iterations improve accuracy;
        /// 20 is typically sufficient for graphs that are not extremely large or sparse.
        /// </param>
        /// <param name="d">
        /// Damping factor (default 0.85).  Represents the probability that a random walker
        /// follows an outgoing edge rather than teleporting to a random node.
        /// The teleportation component distributes <c>(1 - d) / N</c> rank to every node each iteration.
        /// Dangling nodes (no outgoing edges) distribute their entire rank equally to all nodes.
        /// </param>
        public IEnumerable<(double rank, TNode node)> PageRank(int iterations, double d = 0.85)
        {
            var allNodes = this.Nodes.ToList();
            int N = allNodes.Count;
            double oneOverN = 1.0 / N;
            double startValue = (1.0 - d) / N;

            Dictionary<TNode, double> pageRanks = allNodes.ToDictionary(x => x, _ => oneOverN);

            // calculate page rank

            for (int i = 0; i < iterations; i++)
            {
                Dictionary<TNode, double> pageRanksNext = allNodes.ToDictionary(x => x, _ => startValue);

                foreach (var node in allNodes)
                {
                    var edges = Follow(node);
                    int L = edges.Count();          // number of outbound edges
                    double current = pageRanks[node];

                    if (L == 0)
                    {
                        // page rank goes equally to all other nodes
                        foreach (var key in pageRanks.Keys)
                        {
                            pageRanksNext[key] += d * (current / N);
                        }
                    }
                    else
                    {
                        foreach (var edge in edges)
                        {
                            // We share our page rank equally across all our outbound edges
                            pageRanksNext[edge.End] += d * current / L;
                        }
                    }
                }

                pageRanks = pageRanksNext;
            }
            return pageRanks.OrderBy(x => x.Value).Select(x => (x.Value, x.Key));
        }

        /// <summary>
        /// IEnumerable implementation
        /// </summary>
        public IEnumerator<TNode> GetEnumerator()
        {
            return this.GetNodes<TNode>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}