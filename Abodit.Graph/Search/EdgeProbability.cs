using System;

namespace Abodit.Graph
{
    /// <summary>
    /// An edge with a probability on it
    /// </summary>
    /// <remarks>
    /// This is an example of the kind of relationship you can create and use on edges in a graph.
    /// </remarks>
    public class EdgeProbability : IRelation, IEdgeProbability, IEquatable<EdgeProbability>, IDotGraphEdge
    {
        private double probability;

        /// <inheritdoc />
        public double Probability
        {
            get { return this.probability; }
            set
            {
                if (double.IsNaN(value)) throw new ArgumentException("Assigning NaN to a probability");
                this.probability = value;
            }
        }

        /// <inheritdoc />
        public bool IsReflexive => false;

        /// <summary>
        /// Get a label for a dot graph
        /// </summary>
        public string DotLabel => $"{this.probability:0.000}";

        /// <summary>
        /// Creates a new <see cref="Probability"/>
        /// </summary>
        /// <param name="value"></param>
        public EdgeProbability(double value)
        {
            if (double.IsNaN(value)) throw new ArgumentException("EdgeProbability(NaN)");
            this.probability = value;
        }

        /// <inheritdoc />
        public override string ToString() => $"{this.probability:0.00}";

        /// <inheritdoc />
        public bool Equals(EdgeProbability? other) => other is EdgeProbability && this.probability == other.probability;

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as EdgeProbability);

        /// <inheritdoc />
        public override int GetHashCode() => this.probability.GetHashCode();
    }
}