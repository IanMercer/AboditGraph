using System.Collections.Generic;

namespace Abodit.Graph
{
    /// <summary>
    /// Depth first strategy for queuing (i.e. use a Stack)
    /// </summary>
    public class DepthFirstSearch<T> : ISearchOrder<T>
    {
        private readonly Stack<T> stack = new Stack<T>();

        /// <summary>
        /// Count of items queued
        /// </summary>
        public int Count => stack.Count;

        /// <summary>
        /// Dequeue an item
        /// </summary>
        public T Dequeue()
        {
            return stack.Pop();
        }

        /// <summary>
        /// Enqueue an item
        /// </summary>
        public void Enqueue(T value)
        {
            stack.Push(value);
        }
    }
}