using System.Collections.Generic;

namespace Abodit.Graph
{
    /// <summary>
    /// Breadth first strategy for queuing (i.e. use a Queue)
    /// </summary>
    public class BreadthFirstSearch<T> : ISearchOrder<T>
    {
        private readonly Queue<T> queue = new Queue<T>();

        /// <summary>
        /// Count of items queued
        /// </summary>
        public int Count => queue.Count;

        /// <summary>
        /// Dequeue an item
        /// </summary>
        public T Dequeue()
        {
            return queue.Dequeue();
        }

        /// <summary>
        /// Enqueue an item
        /// </summary>
        public void Enqueue(T value)
        {
            queue.Enqueue(value);
        }
    }
}