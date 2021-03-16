using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abodit.Graph.Base
{
    public abstract partial class GraphBase<TNode, TRelation>
    {
        /// <summary>
        /// A predicate, object pair pointing backwards (stored in a linked list)
        /// </summary>
        [DebuggerVisualizer("{Start}<--{Predicate}--")]
        protected class PredicatePrevious : IEnumerable<PredicatePrevious>
        {
            private const int limit = 10000;

            /// <summary>
            /// The predicate (Edge type)
            /// </summary>
            public TRelation Predicate;

            /// <summary>
            /// The start node
            /// </summary>
            public TNode Start;

            /// <summary>
            /// A linked list of further items
            /// </summary>
            public PredicatePrevious? Next;

            /// <summary>
            /// Creates a new instance of the <see cref="PredicatePrevious"/> class
            /// </summary>
            public PredicatePrevious(TRelation predicate, TNode start)
            {
                this.Predicate = predicate;
                this.Start = start;
                this.Next = null;
            }

            /// <summary>
            /// Enumerates the chain
            /// </summary>
            public IEnumerable<PredicatePrevious> Chain()
            {
                var current = this;
                int i = limit;
                while (current != null)
                {
                    yield return current;
                    current = current.Next;
                    if (i-- < 0) throw new Exception("Infinite loop possible");
                }
            }

            /// <summary>
            /// IEnumerable implementation
            /// </summary>
            public IEnumerator<PredicatePrevious> GetEnumerator()
            {
                return this.Chain().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc />
            public override string ToString() => $"{Start}<--{Predicate}--";
        }
    }
}