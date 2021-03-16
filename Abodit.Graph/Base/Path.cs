using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph
{
    /// <summary>
    /// A path through the graph
    /// </summary>
    public class Path<TNode> : IEquatable<Path<TNode>>
        where TNode : IEquatable<TNode>
    {
        /// <summary>
        /// Current in chain
        /// </summary>
        public TNode Current { get; }

        /// <summary>
        /// Score for link to previous in path
        /// </summary>
        public double Score { get; }

        /// <summary>
        /// Previous in chain
        /// </summary>
        public Path<TNode>? Previous { get; }

        /// <summary>
        /// Root of chain
        /// </summary>
        public TNode Root => this.Previous is null ? this.Current : this.Previous.Root;

        /// <summary>
        /// Get current pair
        /// </summary>
        public (TNode previous, TNode successor) Pair => (this.Previous!.Current, this.Current);

        /// <summary>
        /// Get pairs
        /// </summary>
        public IEnumerable<(TNode previous, TNode successor)> Pairs
        {
            get
            {
                if (this.Previous is null) yield break;
                foreach (var pair in this.Previous.Pairs)
                {
                    yield return pair;
                }
                yield return this.Pair;
            }
        }

        /// <summary>
        /// Gets all ancestors of a path (including self)
        /// </summary>
        public IEnumerable<Path<TNode>> Ancestors
        {
            get
            {
                var current = this;
                while (current is Path<TNode>)
                {
                    yield return current;
                    current = current.Previous;
                }
            }
        }

        /// <summary>
        /// Starts a new path
        /// </summary>
        public Path(TNode start, double score)
        {
            this.Current = start;
            this.Previous = null;
            this.Score = score;
        }

        /// <summary>
        /// Create a new path from an existing one
        /// </summary>
        private Path(TNode observation, double score, Path<TNode> previous)
        {
            this.Current = observation;
            this.Previous = previous;
            this.Score = score;
        }

        /// <summary>
        /// Extend a path
        /// </summary>
        public Path<TNode> Extend(TNode observation, double score)
        {
            if (object.ReferenceEquals(this.Current, observation)) throw new ArgumentException("Cannot extend to same - no loops allowed");
            return new Path<TNode>(observation, score, this);
        }

        /// <inheritdoc/>>
        public override string ToString() => string.Join("<-", this.Ancestors.Select(x => $"{x.Current} ({x.Score:0.000})"));

        /// <summary>
        /// Convert a path of one type to a path of another type
        /// </summary>
        public Path<TOther>? Convert<TOther>(Func<TNode, TOther> convert)
            where TOther : IEquatable<TOther>
        {
            var path = default(Path<TOther>);
            foreach (var next in this.Ancestors.Reverse())
            {
                path = (path is default(Path<TOther>)) ?
                    new Path<TOther>(convert(next.Current), next.Score) :
                    path.Extend(convert(next.Current), next.Score);
            }
            return path;
        }

        /// <inheritdoc/>>
        public bool Equals(Path<TNode>? other)
        {
            if (other is null) return false;
            var current = this;

            // Always protect while loops and reursion against getting stuck in a thread pool environment
            int stupidLimit = 1000000;
            while (true)
            {
                if (!current.Current.Equals(other.Current)) return false;
                if (stupidLimit-- < 0) throw new Exception("Infinite loop");
                if (current.Previous is null & other.Previous is null) return true;
                if (current.Previous is null) return false;
                if (other.Previous is null) return false;

                current = current.Previous;
                other = other.Previous;
            }
        }
    }
}