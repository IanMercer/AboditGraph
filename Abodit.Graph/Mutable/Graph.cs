using Abodit.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Mutable
{
    /// <summary>
    /// An in-memory, mutable graph of statements (subject, predicate, object)
    /// </summary>
    public partial class Graph<TNode, TRelation> : Abodit.Graph.Base.GraphBase<TNode, TRelation>, IEnumerable<TNode>
        where TNode : IEquatable<TNode>
        where TRelation : notnull, IEquatable<TRelation>, IRelation
    {
        /// <summary>
        /// Add a statement, returns true if it was added, false if already there
        /// </summary>
        /// <remarks>
        /// Direct loops back to self are not allowed
        /// </remarks>
        public bool AddStatement(TNode start, TRelation predicate, TNode end)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            if (start is null || end is null) throw new Exception("Trying to relate a null");
            // If something is a synonym of itself, really don't care
            if (start.Equals(end)) return false;

            if (predicate.IsReflexive)
            {
                this.AddStatement(new Edge(end, predicate, start));
            }
            return this.AddStatement(new Edge(start, predicate, end));
        }

        /// <summary>
        /// Add a statement, returns true if it was added, false if already there
        /// </summary>
        private bool AddStatement(Edge statement)
        {
            var forward = new PredicateNext(statement.Predicate, statement.End);
            if (!StartIndexedEdges.TryAdd(statement.Start, forward))
            {
                var current = StartIndexedEdges[statement.Start];
                // Skip if the statement is already there
                if (current.Predicate.Equals(statement.Predicate) && current.End.Equals(statement.End))
                    return false;
                int i = this.Limit;
                while (current.Next != null)
                {
                    current = current.Next;
                    // Skip if the statement is already there
                    if (current.Predicate.Equals(statement.Predicate) && current.End.Equals(statement.End))
                        return false;
                    if (i-- < 0) throw new Exception("Infinite loop possible");
                }
                current.Next = forward;
            }

            var reverse = new PredicatePrevious(statement.Predicate, statement.Start);
            if (!EndIndexedEdges.TryAdd(statement.End, reverse))
            {
                var current = EndIndexedEdges[statement.End];
                // Skip if the statement is already there
                if (current.Predicate.Equals(statement.Predicate) && current.Start.Equals(statement.Start))
                    return false;
                int i = this.Limit;
                while (current.Next != null)
                {
                    current = current.Next;
                    // Skip if the statement is already there
                    if (current.Predicate.Equals(statement.Predicate) && current.Start.Equals(statement.Start))
                        return false;
                    if (i-- < 0) throw new Exception("Infinite loop possible");
                }
                current.Next = reverse;
            }
            return true;
        }

        /// <summary>
        /// Remove a statement
        /// </summary>
        public bool RemoveStatement(TNode start, TRelation predicate, TNode end)
        {
            if (!this.StartIndexedEdges.ContainsKey(start)) return false;
            if (!this.EndIndexedEdges.ContainsKey(end)) return false;      // should never happen

            PredicateNext? startCopy = this.StartIndexedEdges[start];
            PredicatePrevious? endCopy = this.EndIndexedEdges[end];

            {
                PredicateNext? previousCopied = null;
                PredicateNext? current = startCopy;

                int i = this.Limit;
                while (current is PredicateNext)
                {
                    if (current.Predicate.Equals(predicate) && current.End.Equals(end))
                    {
                        if (previousCopied is PredicateNext)
                        {
                            // Remove current from the chain, no need to copy anything else (immutable)
                            previousCopied.Next = current.Next;
                        }
                        else
                        {
                            // Remove current from the head, no need for any copying
                            startCopy = current.Next;
                        }
                        break;
                    }

                    // Copy this to a new chain for immutability reasons, reversing the order as we go (easier)!
                    var copyNode = new PredicateNext(current.Predicate, current.End);

                    if (previousCopied is PredicateNext)
                    {
                        previousCopied.Next = copyNode;
                    }
                    else
                    {
                        // New head of chain
                        startCopy = copyNode;
                    }

                    previousCopied = copyNode;

                    current = current.Next;

                    if (i-- < 0) throw new Exception("Infinite loop possible");
                }
            }

            {
                PredicatePrevious? previousCopied = null;
                PredicatePrevious? current = endCopy;

                int i = this.Limit;
                while (current is PredicatePrevious)
                {
                    if (current.Predicate.Equals(predicate) && current.Start.Equals(start))
                    {
                        if (previousCopied is PredicatePrevious)
                        {
                            // Remove current from the chain, no need to copy anything else (immutable)
                            previousCopied.Next = current.Next;
                        }
                        else
                        {
                            // Remove current from the head, no need for any copying
                            endCopy = current.Next;
                        }
                        break;
                    }

                    // Copy this to a new chain for immutability reasons, reversing the order as we go (easier)!
                    var copyNode = new PredicatePrevious(current.Predicate, current.Start);

                    if (previousCopied is PredicatePrevious)
                    {
                        previousCopied.Next = copyNode;
                    }
                    else
                    {
                        // New head of chain
                        endCopy = copyNode;
                    }

                    previousCopied = copyNode;

                    current = current.Next;

                    if (i-- < 0) throw new Exception("Infinite loop possible");
                }
            }

            if (startCopy is PredicateNext)
                StartIndexedEdges.AddOrUpdate(start, startCopy, (key, c) => startCopy);   // will always update
            else
                StartIndexedEdges.TryRemove(start, out startCopy);

            if (endCopy is PredicatePrevious)
                EndIndexedEdges.AddOrUpdate(end, endCopy, (key, c) => endCopy);         // will always update
            else
                EndIndexedEdges.TryRemove(end, out endCopy);

            return true;
        }

        /// <summary>
        /// Remove a node from the graph
        /// </summary>
        public bool Remove(TNode node)
        {
            bool found = false;

            if (StartIndexedEdges.TryRemove(node, out var predicateNext))
            {
                found = true;
                // These are all going away
                while (predicateNext is PredicateNext)
                {
                    RemoveEdgesBetween(node, predicateNext.End);
                    predicateNext = predicateNext.Next;
                }
            }

            if (EndIndexedEdges.TryRemove(node, out var predicatePrevious))
            {
                found = true;
                // These are all going away
                while (predicatePrevious is PredicatePrevious)
                {
                    RemoveEdgesBetween(predicatePrevious.Start, node);
                    predicatePrevious = predicatePrevious.Next;
                }
            }

            return found;
        }

        /// <summary>
        /// Remove edges from the graph between a pair of nodes
        /// </summary>
        public int RemoveEdgesBetween(TNode a, TNode b)
        {
            int foundForward = 0;
            if (StartIndexedEdges.TryGetValue(a, out var forward))
            {
                // Chomp links off the front as long as they match
                int stupidLimit = 10000;
                while (forward.End.Equals(b))
                {
                    if (stupidLimit-- < 0) throw new Exception("Stupid limit hit, forward from start");
                    foundForward++;
                    if (forward.Next is null)
                    {
                        StartIndexedEdges.TryRemove(a, out _);
                        forward = null;
                        break;
                    }
                    else
                    {
                        StartIndexedEdges[a] = forward.Next;
                        forward = forward.Next;
                    }
                }

                // Now either empty of not a match and not first in chain
                var previous = forward;
                var current = forward?.Next;  // may be null, or may both be null if already at end
                stupidLimit = 10000;
                while (current is PredicateNext)
                {
                    if (stupidLimit-- < 0) throw new Exception("Stupid limit hit, forward from start middle");
                    if (current.End.Equals(b))
                    {
                        foundForward++;
                        // Remove current
                        previous!.Next = current.Next;
                    }

                    previous = current;
                    current = current.Next;
                }
            }
            // and reverse

            int foundReverse = 0;
            if (EndIndexedEdges.TryGetValue(b, out var reverse))
            {
                // Chomp links off the front as long as they match
                int stupidLimit = 10000;
                while (reverse.Start.Equals(a))
                {
                    if (stupidLimit-- < 0) throw new Exception("Stupid limit hit, reverse from end front");
                    foundReverse++;
                    if (reverse.Next is null)
                    {
                        EndIndexedEdges.TryRemove(b, out _);
                        reverse = null;
                        break;
                    }
                    else
                    {
                        EndIndexedEdges[b] = reverse.Next;
                        reverse = reverse.Next;
                    }
                }

                // Now either empty of not a match and not first in chain
                var previous = reverse;
                var current = reverse?.Next;  // may be null, or may both be null if already at end
                stupidLimit = 10000;
                while (current is PredicatePrevious)
                {
                    if (stupidLimit-- < 0) throw new Exception("Stupid limit hit, reverse from end more");
                    if (current.Start.Equals(a))
                    {
                        foundReverse++;
                        // Remove current
                        previous!.Next = current.Next;
                    }

                    previous = current;
                    current = current.Next;
                }
            }

            return foundForward + foundReverse;
        }

        /// <summary>
        /// Replace an edge in the graph
        /// </summary>
        public bool ReplaceEdge(TNode start, TNode end, TRelation newRelation)
        {
            // TODO: Make this more efficient?
            int found = this.RemoveEdgesBetween(start, end);
            bool added = this.AddStatement(start, newRelation, end);
            return found == 2 && added;
        }

        /// <summary>
        /// Union two graphs
        /// </summary>
        public Graph<TNode, TRelation> Union(Graph<TNode, TRelation> other)
        {
            var result = new Graph<TNode, TRelation>();
            foreach (var edge in this.Edges.Concat(other.Edges).ToList())       // ToList for debugging
            {
                result.AddStatement(edge);
            }
            return result;
        }

        /// <summary>
        /// Intersect two graphs
        /// </summary>
        public Graph<TNode, TRelation> Intersect(Graph<TNode, TRelation> other)
        {
            var result = new Graph<TNode, TRelation>();
            // As a struct, this should just work - if they have the same start, Predicate and end
            var commonEdges = this.Edges.Intersect(other.Edges);
            foreach (var edge in commonEdges)
            {
                result.AddStatement(edge);
            }
            return result;
        }

        /// <summary>
        /// Find all the outgoing edges from a node, optionally filtered to a set of predicates
        /// match only nodes of type T along the way.
        /// </summary>
        /// <returns>The results as a tree (can be flattened using SelectMany). Edges are the predicates
        /// that were used during the search which is only a subset of all edges between these nodes.</returns>
        public Graph<T, TRelation> Successors<T>(TNode start2, params TRelation[] predicates)
            where T : TNode, IEquatable<T>
        {
            var result = new Graph<T, TRelation>();
            var stack = new Stack<TNode>();
            stack.Push(start2);

            return SuccessorsHelper<T>(result, stack, predicates);
        }

        /// <summary>
        /// Find all the outgoing edges from a list of nodes, optionally filtered to a set of predicates.
        /// Match only nodes of type T along the way.
        /// </summary>
        /// <returns>The results as a tree (can be flattened using SelectMany). Edges are the predicates
        /// that were used during the search which is only a subset of all edges between these nodes.</returns>
        public Graph<T, TRelation> Successors<T>(IEnumerable<TNode> starts, params TRelation[] predicates)
            where T : TNode, IEquatable<T>
        {
            var result = new Graph<T, TRelation>();
            var stack = new Stack<TNode>();
            foreach (var start in starts)
            {
                stack.Push(start);
            }
            return SuccessorsHelper<T>(result, stack, predicates);
        }

        private Graph<T, TRelation> SuccessorsHelper<T>(Graph<T, TRelation> result, Stack<TNode> stack, params TRelation[] predicates) where T : IEquatable<T>, TNode
        {
            var visited = new HashSet<TNode>();

            while (stack.Count > 0)
            {
                var start = stack.Pop();
                var outgoing = this.Follow(start, predicates);

                foreach (var edge in outgoing)
                {
                    if (!(edge.Start is T inEnd) || !(edge.End is T outEnd)) continue;

                    result.AddStatement(inEnd, edge.Predicate, outEnd);

                    if (!visited.Contains(outEnd))
                    {
                        stack.Push(outEnd);
                        visited.Add(outEnd);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Find all the incoming edges from a vertex using a given set of predicates (or none)
        /// and keep following edges of that type match only nodes of type T.
        /// </summary>
        /// <returns>The results as a tree (can be flattened using SelectMany). Edges are the predicates
        /// that were used during the search which is only a subset of all edges between these nodes.</returns>
        public Graph<T, TRelation> Predecessors<T>(TNode start, params TRelation[] predicates)
            where T : TNode, IEquatable<T>
        {
            var stack = new Stack<TNode>();
            stack.Push(start);
            return PredecessorsHelper<T>(stack, predicates);
        }

        /// <summary>
        /// Find all the incoming edges from a list of nodes using a given set of predicates (or empty for all)
        /// and keep following edges of those types, match only nodes of type T.
        /// </summary>
        /// <returns>The results as a tree (can be flattened using SelectMany). Edges are the predicates
        /// that were used during the search which is only a subset of all edges between these nodes.</returns>
        public Graph<T, TRelation> Predecessors<T>(IEnumerable<TNode> starts, params TRelation[] predicates)
            where T : TNode, IEquatable<T>
        {
            var stack = new Stack<TNode>();
            foreach (var start in starts)
            {
                stack.Push(start);
            }
            return PredecessorsHelper<T>(stack, predicates);
        }

        private Graph<T, TRelation> PredecessorsHelper<T>(Stack<TNode> stack, params TRelation[] predicates) where T : IEquatable<T>, TNode
        {
            var visited = new HashSet<TNode>();
            var result = new Graph<T, TRelation>();

            while (stack.Count > 0)
            {
                var start = stack.Pop();
                var incoming = this.Back(start, predicates);

                foreach (var edge in incoming)
                {
                    if (!(edge.Start is T inEnd) || !(edge.End is T outEnd)) continue;

                    result.AddStatement(inEnd, edge.Predicate, outEnd);

                    if (!visited.Contains(inEnd))
                    {
                        stack.Push(inEnd);
                        visited.Add(inEnd);
                    }
                }
            }
            return result;
        }

        private class FuncComparer<T> : IComparer<T>
            where T : notnull
        {
            private readonly Comparison<T> comparison;

            public FuncComparer(Comparison<T> comparison)
            {
                this.comparison = comparison;
            }

#pragma warning disable CS8767 // Something something nullability

            public int Compare(T x, T y)
#pragma warning restore CS8767 // Something something nullability
            {
                return comparison(x, y);
            }
        }

        /// <summary>
        /// Shortest path with cumulative function
        /// </summary>
        public Graph<(TNode node, double probability), TRelation> Minimum(IEnumerable<TNode> starts,
            TRelation predicate, Func<TRelation, double> probabilityFunction,
            Func<TNode, TNode, bool> filter,   // To prevent going backwards by Id for example
            TRelation newRelation)
        {
            var visited = new HashSet<TNode>();
            //var active = new SortedSet<(TNode node, double probability)>(new FuncComparer((a, b) => a.probability.CompareTo(b.probability)); // TODO: Use SortedSet
            var active = new List<(TNode node, TNode succ, double prior, double posterior)>();
            foreach (var start in starts)
            {
                foreach (var succ in Follow(start, predicate))
                {
                    active.Add((node: start, succ: succ.End, prior: 1.0, posterior: 1.0 * probabilityFunction(succ.Predicate)));
                }
            }
            active = active.OrderByDescending(x => x.posterior).ToList();

            var result = new Graph<(TNode node, double probability), TRelation>();

            while (active.Count > 0)
            {
                // Pick the highest probability next
                var current = active[0];
                active.RemoveAt(0);

                if (visited.Contains(current.node)) continue;       // Prim/Kruskall
                visited.Add(current.node);

                if (!filter(current.node, current.succ)) continue;

                // The from node should only ever be added once, and the to node will get added more times but will be equal
                result.AddStatement((current.node, current.prior), newRelation, (current.succ, current.posterior));

                foreach (var succ in Follow(current.succ, predicate))
                {
                    active.Add((node: current.succ, succ: succ.End,
                        prior: current.posterior,
                        posterior: current.posterior * probabilityFunction(succ.Predicate)));
                }

                active = active.OrderByDescending(x => x.posterior).ToList();
            }
            return result;
        }

        /// <summary>
        /// Debug graph (use Graphviz)
        /// </summary>
        /// <remarks>
        /// This is mainly for debugging - expand the DotGraph property, copy the text, paste it into Graphviz
        /// </remarks>
        public string DotGraph
        {
            get
            {
                // Two layers of start nodes (root and start nodes)

                var rootNodes = this.Nodes
                    .Where(x => !(this.Back(x).Any()))
                    .ToList();

                var startNodes = this.Nodes
                    .Except(rootNodes)
                    .Where(x => (x is IDotGraphNode dgn && dgn.IsStartNode)) // || this.Back(x).All(y => !this.Back(y.Start).Any()))
                    .ToList();

                var endNodes = this.Nodes.Except(rootNodes).Except(startNodes).Where(x => !this.Follow(x).Any()).ToList();

                var middleNodes = this.Nodes.Except(rootNodes).Except(startNodes).Except(endNodes).ToList();

                return "\n\ndigraph tracks {\nrankdir=\"LR\"\n" +

                    "{ rank=\"min\"\n" +
                        string.Join(" ", startNodes.Select(x => NodeId(x) + " " + NodeProperties(x) + ";")) +
                    "}\n" +
                    "{ rank=\"same\"\n" +
                        string.Join(" ", startNodes.Select(x => NodeId(x) + " " + NodeProperties(x) + ";")) +
                    "}\n" +
                    "{\n" +
                        string.Join(" ", middleNodes.Select(x => NodeId(x) + " " + NodeProperties(x) + ";")) +
                    "}" +
                    "{ rank=\"max\"\n" +
                        string.Join(" ", endNodes.Select(x => NodeId(x) + " " + NodeProperties(x) + ";")) +
                    "}\n" +
                    string.Join(" ", this.Edges
                        .Select(x => DotEdge(x.Start, x.End, x.Predicate))) +
                    "\n" +
                    "}";
            }
        }

        private static string[] colors = new string[] { "violet", "indigo", "blue", "green", "orange", "red" };

        /// <summary>
        /// Cleans up tuple ToString and others for dotGraph to consume
        /// </summary>
        private string NodeId(TNode node)
        {
            if (node is IDotGraphNode dotNode) return dotNode.Id.ToString();
            else return new string(node.GetHashCode().ToString().Take(5).ToArray());
        }

        private string NodeProperties(TNode node)
        {
            if (node is IDotGraphNode dotNode) return dotNode.DotProperties;
            else return "";
        }

        private string EdgeLabel(IRelation edge)
        {
            if (edge is IDotGraphEdge dotEdge) return " ;label=\"" + dotEdge.DotLabel + "\"";
            else return "";
        }

        private const int idspercolor = 10;

        private string DotEdge(TNode node, TNode successor, TRelation relation)
        {
            //double hue = 1.0 / node.Id;

            //// Rainbow by generation
            string color = successor is IDotGraphNode dgn ? colors[(dgn.Id / idspercolor) % colors.Length] : colors[(node.GetHashCode() & 0xFFFFFF) % colors.Length];

            string dotted = (successor is IDotGraphNode dgn2 && dgn2.IsPruned) ? ";style=dashed" : "";

            string label = EdgeLabel(relation);

            int penWidth = relation is IEdgeProbability ep ? 1 + (int)(ep.Probability * 5) : 2;

            return $"{NodeId(node)}->{NodeId(successor)} [penwidth={penWidth};color={color}{dotted}{label}];";
        }

        /// <summary>
        /// Project to a new node and edge type
        /// </summary>
        public Graph<TNode2, TRelation2> Project<TNode2, TRelation2>(Func<TNode, TNode2> nodeProjection, Func<TRelation, TRelation2> edgeProjection)
            where TNode2 : IEquatable<TNode2>
            where TRelation2 : IEquatable<TRelation2>, IRelation
        {
            var graph = new Mutable.Graph<TNode2, TRelation2>();

            foreach (var edge in this.Edges)
            {
                graph.AddStatement(nodeProjection(edge.Start), edgeProjection(edge.Predicate), nodeProjection(edge.End));
            }
            return graph;
        }
    }
}