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
        where TRelation : notnull, IEquatable<TRelation>, IRelation
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
        /// Enumerate all the nodes with no priors
        /// </summary>
        public IEnumerable<TNode> StartNodes
        {
            get
            {
                return this.StartIndexedEdges.Select(x => x.Key).Except(this.EndIndexedEdges.Select(x => x.Key));
            }
        }

        /// <summary>
        /// Enumerate all the nodes with no successors
        /// </summary>
        public IEnumerable<TNode> EndNodes
        {
            get
            {
                return this.EndIndexedEdges.Select(x => x.Key).Except(this.StartIndexedEdges.Select(x => x.Key));
            }
        }

        /// <summary>
        /// Does the graph contain this node?
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
        /// Find all the siblings of a node by following one or more predicates backwards
        /// to find parent nodes and then forwards to find children at the same level
        /// </summary>
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
        /// Topological sort approx
        /// </summary>
        public IEnumerable<TNode> TopologicalSortApprox()
        {
            var nodesWithNoIncomingLinks = this.StartNodes;

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
        /// PageRank algorithm on Graph assuming equal weighting on each edge type
        /// </summary>
        public IEnumerable<(double rank, TNode node)> PageRank(int iterations, double d = 0.85)
        {
            double oneOverN = 1.0 / this.Nodes.Count();
            int N = this.Nodes.Count();
            double startValue = (1.0 - d) / N;

            Dictionary<TNode, double> pageRanks = this.Nodes.Select(n => (n, oneOverN)).ToDictionary(x => x.n, x => x.oneOverN);

            // calculate page rank

            for (int i = 0; i < iterations; i++)
            {
                Dictionary<TNode, double> pageRanksNext = this.Nodes.ToDictionary(x => x, x => startValue);

                foreach (var node in this.Nodes)
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