using Abodit.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Immutable
{
    /// <summary>
    /// An in-memory immutable graph of statements (subject, predicate, object)
    /// </summary>
    /// <remarks>
    /// Shares nodes with other graphs
    /// </remarks>
    public class Graph<TNode, TRelation> : Abodit.Graph.Base.GraphBase<TNode, TRelation>, IEnumerable<TNode>
        where TNode : class, IEquatable<TNode>
        where TRelation : notnull, IEquatable<TRelation>, IRelation
    {
        /// <summary>
        /// Create a new empty graph
        /// </summary>
        public Graph() : base()
        {
        }

        private Graph(ConcurrentDictionary<TNode, PredicateNext> startIndexedEdges, ConcurrentDictionary<TNode, PredicatePrevious> endIndexedEdges)
            : base(startIndexedEdges, endIndexedEdges)
        {
        }

        /// <summary>
        /// Add a statement, returns true if it was added, false if already there
        /// </summary>
        /// <remarks>
        /// Direct loops back to self are not allowed
        /// </remarks>
        public Graph<TNode, TRelation> AddStatement(TNode start, TRelation predicate, TNode end)
        {
            if (start is null || end is null) throw new Exception("Trying to relate a null");
            // If something is a synonym of itself, really don't care
            if (start == end) return this;

            if (predicate is TRelation && predicate.IsReflexive)
            {
                this.AddStatement(new Edge(end, predicate, start));
            }
            return this.AddStatement(new Edge(start, predicate, end));
        }

        /// <summary>
        /// Add a statement
        /// </summary>
        private Graph<TNode, TRelation> AddStatement(Edge statement)
        {
            var startIndexedEdges = new ConcurrentDictionary<TNode, PredicateNext>(StartIndexedEdges);
            var endIndexedEdges = new ConcurrentDictionary<TNode, PredicatePrevious>(EndIndexedEdges);

            var forward = new PredicateNext(statement.Predicate, statement.End);
            // If it doesn't exist start a new chain
            if (!startIndexedEdges.TryAdd(statement.Start, forward))
            {
                // otherwise add to an existing chain
                var start = startIndexedEdges[statement.Start];
                var current = start;
                int i = this.Limit;
                while (current.Next != null)
                {
                    // Skip if the statement is already there
                    if (current.Predicate.Equals(statement.Predicate) && current.End == statement.End)
                        return this;
                    if (i-- < 0) throw new Exception("Infinite loop possible");
                    current = current.Next;
                }
                // Add to front of list not back
                forward.Next = start;
                startIndexedEdges.AddOrUpdate(statement.Start, forward, (key, c) => forward); // will always update
            }

            var reverse = new PredicatePrevious(statement.Predicate, statement.Start);
            // If it doesn't exist start a new chain
            if (!endIndexedEdges.TryAdd(statement.End, reverse))
            {
                // otherwise add to an existing chain
                var start = endIndexedEdges[statement.End];
                var current = start;
                int i = this.Limit;
                while (current.Next != null)
                {
                    // Skip if the statement is already there
                    if (current.Predicate.Equals(statement.Predicate) && current.Start == statement.Start)
                        return this;
                    if (i-- < 0) throw new Exception("Infinite loop possible");
                    current = current.Next;
                }
                // Add it to the front of the list
                reverse.Next = start;
                endIndexedEdges.AddOrUpdate(statement.End, reverse, (key, c) => reverse); // will always update
            }

            return new Graph<TNode, TRelation>(startIndexedEdges, endIndexedEdges);
        }

        /// <summary>
        /// Remove a statement
        /// </summary>
        public Graph<TNode, TRelation> RemoveStatement(TNode start, TRelation predicate, TNode end)
        {
            if (!this.StartIndexedEdges.ContainsKey(start)) return this;
            if (!this.EndIndexedEdges.ContainsKey(end)) return this;      // shiould never happen

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

            var startIndexedEdges = new ConcurrentDictionary<TNode, PredicateNext>(StartIndexedEdges);
            var endIndexedEdges = new ConcurrentDictionary<TNode, PredicatePrevious>(EndIndexedEdges);

            if (startCopy is PredicateNext)
                startIndexedEdges.AddOrUpdate(start, startCopy, (key, c) => startCopy);   // will always update
            else
                startIndexedEdges.TryRemove(start, out startCopy);

            if (endCopy is PredicatePrevious)
                endIndexedEdges.AddOrUpdate(end, endCopy, (key, c) => endCopy);         // will always update
            else
                endIndexedEdges.TryRemove(end, out endCopy);

            return new Graph<TNode, TRelation>(startIndexedEdges, endIndexedEdges);
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
        /// Find all the outgoing edges from a vertex with a given predicate (or null) and keep following edges of that type
        /// match only nodes of type T. Return the results as a tree (can be flattened using SelectMany).
        /// </summary>
        public Graph<T, TRelation> Successors<T>(TNode start2, TRelation predicate)
            where T : class, TNode, IEquatable<T>
        {
            var result = new Graph<T, TRelation>();
            var stack = new Stack<TNode>();
            stack.Push(start2);

            return SuccessorsHelper<T>(result, predicate, stack);
        }

        /// <summary>
        /// Find all the outgoing edges from a list of node with a given predicate (or null) and keep following edges of that type
        /// match only nodes of type T. Return the results as a tree (can be flattened using SelectMany).
        /// </summary>
        public Graph<T, TRelation> Successors<T>(IEnumerable<TNode> starts, TRelation predicate)
            where T : class, TNode, IEquatable<T>
        {
            var result = new Graph<T, TRelation>();
            var stack = new Stack<TNode>();
            foreach (var start in starts)
            {
                stack.Push(start);
            }
            return SuccessorsHelper<T>(result, predicate, stack);
        }

        private Graph<T, TRelation> SuccessorsHelper<T>(Graph<T, TRelation> result, TRelation predicate, Stack<TNode> stack) where T : class, IEquatable<T>, TNode
        {
            var visited = new HashSet<TNode>();

            while (stack.Count > 0)
            {
                var start = stack.Pop();
                var outgoing = this.Follow(start, predicate);

                foreach (var edge in outgoing)
                {
                    if (!(edge.Start is T inEnd) || !(edge.End is T outEnd)) continue;

                    result = result.AddStatement(inEnd, edge.Predicate, outEnd);

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
        /// Find all the incoming edges from a vertex with a given predicate (or null) and keep following edges of that type
        /// match only nodes of type T. Return the results as a tree (can be flattened using SelectMany).
        /// </summary>
        public Graph<T, TRelation> Predecessors<T>(TNode start, TRelation predicate = default!)
            where T : class, TNode, IEquatable<T>
        {
            var stack = new Stack<TNode>();
            stack.Push(start);
            return PredecessorsHelper<T>(predicate!, stack);
        }

        /// <summary>
        /// Find all the incoming edges from a list of nodes with a given predicate (or null) and keep following edges of that type
        /// match only nodes of type T. Return the results as a tree (can be flattened using SelectMany).
        /// </summary>
        public Graph<T, TRelation> Predecessors<T>(IEnumerable<TNode> starts, TRelation predicate = default!)
            where T : class, TNode, IEquatable<T>
        {
            var stack = new Stack<TNode>();
            foreach (var start in starts)
            {
                stack.Push(start);
            }
            return PredecessorsHelper<T>(predicate!, stack);
        }

        private Graph<T, TRelation> PredecessorsHelper<T>(TRelation predicate, Stack<TNode> stack)
            where T : class, IEquatable<T>, TNode
        {
            var visited = new HashSet<TNode>();
            var result = new Graph<T, TRelation>();

            while (stack.Count > 0)
            {
                var start = stack.Pop();
                var incoming = this.Back(start, predicate);

                foreach (var edge in incoming)
                {
                    if (!(edge.Start is T inEnd) || !(edge.End is T outEnd)) continue;

                    result = result.AddStatement(inEnd, edge.Predicate, outEnd);

                    if (!visited.Contains(inEnd))
                    {
                        stack.Push(inEnd);
                        visited.Add(inEnd);
                    }
                }
            }
            return result;
        }
    }
}