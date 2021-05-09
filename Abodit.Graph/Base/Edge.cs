using System;
using System.Diagnostics;

namespace Abodit.Graph.Base
{
    public abstract partial class GraphBase<TNode, TRelation>
    {
        /// <summary>
        /// A relationship between two objects
        /// </summary>
        /// <remarks>
        /// These are not stored in the graph per se but are generated on the fly when the graph is queried
        /// so do not expect them to be reference equals for the same edge in the future.
        /// </remarks>
        [DebuggerDisplay("{Start} -- {Predicate} --> {End}")]
        public struct Edge : IEquatable<Edge>
        {
            /// <summary>
            /// The start node
            /// </summary>
            public readonly TNode Start;

            /// <summary>
            /// The edge predicate (relation)
            /// </summary>
            public readonly TRelation Predicate;

            /// <summary>
            /// The end node
            /// </summary>
            public readonly TNode End;

            /// <summary>
            /// Creates a new instance of the <see cref="Edge"/> class
            /// </summary>
            public Edge(TNode start, TRelation predicate, TNode end)
            {
                this.Start = start;
                this.Predicate = predicate;
                this.End = end;
            }

            /// <inheritdoc />
            public override bool Equals(object? obj) => obj is GraphBase<TNode, TRelation>.Edge e && this.Equals(e);

            ///<summary>
            ///Compare two edges for equality of their start, end and predicate values
            /// </summary>
            public bool Equals(GraphBase<TNode, TRelation>.Edge other)
            {
                return other is GraphBase<TNode, TRelation>.Edge e && this.Start.Equals(e.Start)
                    && this.Predicate.Equals(e.Predicate)
                    && this.End.Equals(e.End);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
#if NET50
                return HashCode.Combine(this.Start, this.Predicate, this.End);
#else
                return this.Start.GetHashCode() + this.Predicate.GetHashCode() + this.End.GetHashCode();
#endif
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"{Start} -- {Predicate} --> {End}";
            }

            /// <summary>
            /// Equals
            /// </summary>
            public static bool operator ==(GraphBase<TNode, TRelation>.Edge left, GraphBase<TNode, TRelation>.Edge right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Not equals
            /// </summary>
            public static bool operator !=(GraphBase<TNode, TRelation>.Edge left, GraphBase<TNode, TRelation>.Edge right)
            {
                return !(left == right);
            }
        }
    }
}