using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Abodit.Graph.Base
{
    public abstract partial class GraphBase<TNode, TRelation>
    {
        /// <summary>
        /// A predicate, object pair pointing forwards (stored in a linked list)
        /// </summary>
        [DebuggerVisualizer("--{Predicate}-->{End}")]
        protected class PredicateNext : IEnumerable<PredicateNext>
        {
            private const int limit = 10000;

            /// <summary>
            /// The predicate on the edge
            /// </summary>
            public TRelation Predicate;

            /// <summary>
            /// The node at the far end
            /// </summary>
            public TNode End;

            /// <summary>
            /// Linked next object
            /// </summary>
            public PredicateNext? Next { get; set; }

            /// <summary>
            /// Create a new instance of the <see cref="PredicateNext"/> class
            /// </summary>
            public PredicateNext(TRelation predicate, TNode end)
            {
                this.Predicate = predicate;
                this.End = end;
                this.Next = null;
            }

            /// <summary>
            /// Enumerates a chain of PredicateNext objects
            /// </summary>
            /// <returns></returns>
            public IEnumerable<PredicateNext> Chain()
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
            public IEnumerator<PredicateNext> GetEnumerator()
            {
                return this.Chain().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc />
            public override string ToString() => $"--{Predicate}-->{End}";
        }
    }
}