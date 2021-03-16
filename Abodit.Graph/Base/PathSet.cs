using System;
using System.Collections.Generic;

namespace Abodit.Graph
{
    /// <summary>
    /// A set of paths
    /// </summary>
    /// <remarks>
    /// Paths are created by path finding algorithms operating on the graph
    /// </remarks>
    public class PathSet<TNode> where TNode : IEquatable<TNode>
    {
        /// <summary>
        /// Get the paths
        /// </summary>
        public List<Path<TNode>> Paths { get; }

        /// <summary>
        /// Creates a new <see cref="PathSet{TNode}"/>
        /// </summary>
        public PathSet(List<Path<TNode>> paths)
        {
            this.Paths = paths;
        }
    }
}