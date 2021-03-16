using System;
using System.Collections.Generic;

namespace Abodit.Graph
{
    /// <summary>
    /// Random first search strategy (neither a queue nor a stack)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RandomFirstSearch<T> : ISearchOrder<T>
    {
        private readonly Random r = new Random();
        private readonly List<T> queue = new List<T>();

        /// <summary>
        /// Count of items queued
        /// </summary>
        public int Count => queue.Count;

        /// <summary>
        /// Dequeue an item
        /// </summary>
        public T Dequeue()
        {
            int index = r.Next(queue.Count);
            var result = queue[index];
            queue.RemoveAt(index);
            return result;
        }

        /// <summary>
        /// Enqueue an item
        /// </summary>
        public void Enqueue(T value)
        {
            queue.Add(value);
        }
    }
}