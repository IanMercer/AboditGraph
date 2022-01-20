using Abodit.Graph.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph
{
    /// <summary>
    /// Graph extensions for searching, filtering, projecting, ...
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Get all the nodes on the shortest path from A to B by following relationships of type T
        /// </summary>
        /// <remarks>
        /// Handles circular graphs too ...
        /// </remarks>
        public static IEnumerable<TNode>? ShortestPath<TNode, TRelation>(this GraphBase<TNode, TRelation> graph,
            TNode start,
            TRelation predicate,
            TNode endNode,
            Func<TRelation, double> edgeWeight,
            Func<double, double, double> accumulator
            )
            where TNode : class, IEquatable<TNode>
            where TRelation : IEquatable<TRelation>
        {
            var visited = new Dictionary<TNode, TNode?>();                 // visited node and where we came from ...
            var heap = new Dictionary<TNode, (TNode? node, double cumulative)>();        // can reach A from B in distance C

            // Breadth first search, ensures we search in distance order
            heap.Add(start, (default(TNode), 0.0));

            while (heap.Any())
            {
                var first = heap.OrderBy(h => h.Value.Item2).First();     // Next closest

                var node = first.Key;
                var previous = first.Value.node;
                var distance = first.Value.cumulative;

                heap.Remove(node);
                visited.Add(node, previous);

                // Where can we reach from this node? - add them all to the heap according to distance
                foreach (var item in graph.Follow(node, predicate))
                {
                    var end = item.End;

                    if (end.Equals(endNode))
                    {
                        var reversePath = new List<TNode>();
                        reversePath.Add(end);

                        // Success, follow backwards, return results ...
                        while (node != null)
                        {
                            reversePath.Add(node);
                            node = visited[node];
                        }
                        reversePath.Reverse();
                        return reversePath;
                    }

                    // If not already visited, go visit it ...
                    if (!visited.ContainsKey(end))
                    {
                        double nextDistance = accumulator(distance, edgeWeight(item.Predicate));
                        if (heap.ContainsKey(end))
                        {
                            if (heap[end].cumulative > nextDistance)
                                heap[end] = (node, nextDistance);
                        }
                        else
                        {
                            heap[end] = (node, nextDistance);
                        }
                    }
                }
            }
            return null;
        }
    }
}