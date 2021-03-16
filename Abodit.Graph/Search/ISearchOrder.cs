using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph
{
    /// <summary>
    /// Interface for a search strategy
    /// </summary>
    public interface ISearchOrder<T>
    {
        /// <summary>
        /// Count of how many items waiting
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Dequeue a value
        /// </summary>
        T Dequeue();

        /// <summary>
        /// Enque a value
        /// </summary>
        void Enqueue(T value);
    }
}