using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph
{
    /// <summary>
    /// Best first strategy for queuing returns nodes in best order according to a comparison function
    /// </summary>
    public class BestFirstSearch<T> : ISearchOrder<T>
        where T : notnull
    {
        private readonly SortedSet<T> sortedSet = new SortedSet<T>();

        private class Comparer : IComparer<T>
        {
            private readonly Func<T, T, int> comparison;

            public Comparer(Func<T, T, int> comparison)
            {
                this.comparison = comparison;
            }

#pragma warning disable CS8767 // nullability class vs struct C#9 issue

            public int Compare(T x, T y)
#pragma warning restore CS8767 // nullability
            {
                return comparison(x, y);
            }
        }

        /// <summary>
        /// Creates a new <see cref="BestFirstSearch{T}"/>
        /// </summary>
        public BestFirstSearch(Func<T, T, int> comparer)
        {
            this.sortedSet = new SortedSet<T>(new Comparer(comparer));
        }

        /// <summary>
        /// Count of items queued
        /// </summary>
        public int Count => sortedSet.Count;

        /// <summary>
        /// Dequeue an item
        /// </summary>
        public T Dequeue()
        {
            T result = sortedSet.First();
            sortedSet.Remove(result);
            return result;
        }

        /// <summary>
        /// Enqueue an item
        /// </summary>
        public void Enqueue(T value)
        {
            sortedSet.Add(value);
        }
    }
}