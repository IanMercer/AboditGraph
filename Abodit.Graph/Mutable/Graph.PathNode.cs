using Abodit.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Mutable
{
    public partial class Graph<TNode, TRelation> where TNode : IEquatable<TNode>
        where TRelation : notnull, IEquatable<TRelation>
    {
        /// <summary>
        /// A node in the path (with edges)
        /// </summary>
        public class PathNode : IEquatable<PathNode>
        {
            /// <summary>
            /// The node
            /// </summary>
            public TNode Node { get; }

            /// <summary>
            /// Cached edges so they are only fetched once
            /// </summary>
            public List<Edge> Edges { get; }

            /// <summary>
            /// Sum of outbound scores
            /// </summary>
            public double SumScore { get; }

            /// <summary>
            /// Creates a new <see cref="PathNode"/>
            /// </summary>
            public PathNode(TNode node, IEnumerable<Edge> edges, Func<TRelation, double> probabilityFunction)
            {
                this.Node = node;
                this.Edges = edges.ToList();
                this.SumScore = edges.Sum(x => probabilityFunction(x.Predicate));
            }

            ///<inheritdoc />
            public override string ToString() => $"{this.Node} and {this.Edges.Count} edges";

            ///<inheritdoc />
            public bool Equals(PathNode? other)
            {
                return other is PathNode && this.Node.Equals(other.Node) && this.SumScore.Equals(other.SumScore);
            }
        }
    }
}