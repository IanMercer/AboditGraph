using System;

namespace Abodit.Graph
{
    /// <summary>
    /// A node with a probability set assigned to it
    /// </summary>
    public interface INodeProbability<TItem>
        where TItem : IEquatable<TItem>
    {
        /// <summary>
        /// The total number of people expected here (set should add to this)
        /// </summary>
        int N { get; }

        /// <summary>
        /// The value
        /// </summary>
        ProbabilitySet<TItem> ProbabilitySet { get; }
    }
}