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
        /// Returns all nodes reachable from <paramref name="subject"/> by following edges of type
        /// <paramref name="predicate"/>, ordered by ascending hop-count distance from the subject.
        /// Each result is a <c>(node, distance)</c> pair where distance is the number of hops.
        /// </summary>
        /// <remarks>
        /// Implements Dijkstra's algorithm with a <see cref="PriorityQueue{TElement,TPriority}"/> for
        /// O((n + e) log n) complexity. Handles circular graphs by tracking visited nodes.
        /// </remarks>
        /// <param name="graph">The graph to search.</param>
        /// <param name="subject">The starting node.</param>
        /// <param name="includeStartNode">When <see langword="true"/>, the first result is the subject itself at distance 0.</param>
        /// <param name="predicate">The edge type to follow.</param>
        public static IEnumerable<Tuple<TNode, int>> DistanceToEverywhere<TNode, TRelation>(
            this GraphBase<TNode, TRelation> graph,
            TNode subject,
            bool includeStartNode,
            TRelation predicate)
            where TNode : IEquatable<TNode>
            where TRelation : IEquatable<TRelation>
        {
            var visited = new HashSet<TNode>();
            // Min-heap keyed by distance — O(log n) enqueue and dequeue
            var queue = new PriorityQueue<TNode, int>();
            queue.Enqueue(subject, 0);

            // Track best-known distance to handle a node being enqueued multiple times
            var bestDistance = new Dictionary<TNode, int> { [subject] = 0 };

            while (queue.Count > 0)
            {
                queue.TryDequeue(out TNode? node, out int distance);

                if (!visited.Add(node!))
                    continue;   // already processed at a shorter distance

                foreach (var item in graph.Follow(node!, predicate))
                {
                    var end = item.End;
                    if (visited.Contains(end)) continue;

                    int newDist = distance + 1;
                    if (!bestDistance.TryGetValue(end, out int known) || newDist < known)
                    {
                        bestDistance[end] = newDist;
                        queue.Enqueue(end, newDist);
                    }
                }

                if (includeStartNode || !node!.Equals(subject))
                    yield return Tuple.Create(node!, distance);
            }
        }
    }
}
