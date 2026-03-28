using Abodit.Graph.Base;
using System;
using System.Collections.Generic;

namespace Abodit.Graph
{
    /// <summary>
    /// Graph extensions for searching, filtering, projecting, ...
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the nodes on the lowest-cost path from <paramref name="start"/> to <paramref name="endNode"/>
        /// by following edges of type <paramref name="predicate"/>, or <see langword="null"/> if no path exists.
        /// </summary>
        /// <remarks>
        /// Implements Dijkstra's algorithm with a <see cref="PriorityQueue{TElement,TPriority}"/> for
        /// O((n + e) log n) complexity. Handles circular graphs by tracking visited nodes.
        /// </remarks>
        /// <param name="graph">The graph to search.</param>
        /// <param name="start">The node to start from.</param>
        /// <param name="predicate">The edge type to follow.</param>
        /// <param name="endNode">The destination node.</param>
        /// <param name="edgeWeight">Returns the cost of traversing a single edge.</param>
        /// <param name="accumulator">Combines accumulated cost with an edge cost (e.g. addition or multiplication).</param>
        public static IEnumerable<TNode>? ShortestPath<TNode, TRelation>(
            this GraphBase<TNode, TRelation> graph,
            TNode start,
            TRelation predicate,
            TNode endNode,
            Func<TRelation, double> edgeWeight,
            Func<double, double, double> accumulator)
            where TNode : class, IEquatable<TNode>
            where TRelation : IEquatable<TRelation>
        {
            // Maps each visited node to the node we arrived from
            var visited = new Dictionary<TNode, TNode?>();
            // Maps each node to the best predecessor at the time it was enqueued
            var predecessor = new Dictionary<TNode, TNode?>();
            // Min-heap keyed by cumulative cost — O(log n) operations
            var queue = new PriorityQueue<TNode, double>();
            var bestCost = new Dictionary<TNode, double> { [start] = 0.0 };

            predecessor[start] = null;
            queue.Enqueue(start, 0.0);

            while (queue.Count > 0)
            {
                queue.TryDequeue(out TNode? node, out double distance);

                if (visited.ContainsKey(node!))
                    continue;   // already settled at a lower cost

                var previous = predecessor.GetValueOrDefault(node!);
                visited[node!] = previous;

                foreach (var item in graph.Follow(node!, predicate))
                {
                    var end = item.End;

                    if (end.Equals(endNode))
                    {
                        // Reconstruct path in reverse then flip it
                        var reversePath = new List<TNode> { end, node! };
                        var cur = previous;
                        while (cur != null)
                        {
                            reversePath.Add(cur);
                            cur = visited.GetValueOrDefault(cur);
                        }
                        reversePath.Reverse();
                        return reversePath;
                    }

                    if (visited.ContainsKey(end)) continue;

                    double nextCost = accumulator(distance, edgeWeight(item.Predicate));
                    if (!bestCost.TryGetValue(end, out double known) || nextCost < known)
                    {
                        bestCost[end] = nextCost;
                        predecessor[end] = node;
                        queue.Enqueue(end, nextCost);
                    }
                }
            }
            return null;
        }
    }
}
