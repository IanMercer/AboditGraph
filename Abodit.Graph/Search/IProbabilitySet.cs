using System;
using System.Collections.Generic;

namespace Abodit.Graph
{
    /// <summary>
    /// A node value listing a set of possibilities
    /// </summary>
    public interface IProbabilitySet<TItem>
        where TItem : IEquatable<TItem>
    {
        /// <summary>
        /// Gets the node probabilities
        /// </summary>
        IDictionary<TItem, double> NodeProbabilities { get; }

        /// <summary>
        /// Label for a dot graph
        /// </summary>
        string DotLabel { get; }

        /// <summary>
        /// Magnitude of vector
        /// </summary>
        double Magnitude { get; }

        /// <summary>
        /// Add two node probability sets
        /// </summary>
        ProbabilitySet<TItem> Add(ProbabilitySet<TItem> two);

        /// <summary>
        /// Multiply two node probability sets
        /// </summary>
        /// <param name="value"></param>
        ProbabilitySet<TItem> Multiply(double value);

        /// <summary>
        /// Subtract
        /// </summary>
        ProbabilitySet<TItem> Minus(ProbabilitySet<TItem> nodeProbabilitySet);
    }
}