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
        /// Get all the nodes that can be reached by following relationships of a given type
        /// returns them in sorted order according to how close they are
        /// so shortest path is returned first.  Tuple includes the distance.
        /// </summary>
        /// <remarks>
        /// Handles circular graphs too ...
        /// </remarks>
        public static IEnumerable<Tuple<TNode, int>> DistanceToEverywhere<TNode, TRelation>(this GraphBase<TNode, TRelation> graph, TNode subject,
            bool includeStartNode, TRelation predicate)
            where TNode : IEquatable<TNode>
            where TRelation : IEquatable<TRelation>
        {
            HashSet<TNode> visited = new HashSet<TNode>();
            Dictionary<TNode, int> heap = new Dictionary<TNode, int>();

            // Breadth first search, ensures we search in distance order
            heap.Add(subject, 0);

            while (heap.Any())
            {
                var first = heap.OrderBy(h => h.Value).First();     // Next closest, TODO: an ordered heap would help

                var node = first.Key;
                var distance = first.Value;

                heap.Remove(node);
                visited.Add(node);

                // Where can we reach from this node? - add them all to the heap according to distance
                foreach (var item in graph.Follow(node, predicate))
                {
                    var end = item.End;

                    // If not already visited, go visit it ...
                    if (!visited.Contains(end))
                    {
                        if (heap.ContainsKey(end)) heap[end] = Math.Min(heap[end], distance + 1);
                        else heap[end] = distance + 1;
                    }
                }

                if (includeStartNode || !node.Equals(subject))
                    yield return Tuple.Create(node, distance);
            }
        }
    }
}