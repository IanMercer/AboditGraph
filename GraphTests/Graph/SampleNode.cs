using Abodit.Graph;
using System;

namespace Abodit.Units.Tests.Graph
{
    public class SampleNode : INodeProbability<Person>, IEquatable<SampleNode>, IDotGraphNode
    {
        private static int sequence = 0;

        public int Id { get; }

        public int N => 1;

        public bool IsPruned => false;

        public bool IsStartNode { get; }

        /// <summary>
        /// Creates an intermediate node
        /// </summary>
        public SampleNode()
        {
            this.IsStartNode = true;
            this.Id = sequence++;
            this.ProbabilitySet = new ProbabilitySet<Person>();
        }

        /// <summary>
        /// Creates a start node
        /// </summary>
        public SampleNode(Person person)
        {
            this.Id = sequence++;
            this.ProbabilitySet = new ProbabilitySet<Person>(person);
        }

        private SampleNode(ProbabilitySet<Person> newSet, int id)
        {
            this.Id = id;
            this.ProbabilitySet = newSet;
        }

        public SampleNode WithNewProbabilities(ProbabilitySet<Person> newSet)
        {
            return new SampleNode(newSet, this.Id);
        }

        public ProbabilitySet<Person> ProbabilitySet { get; }

        public string DotProperties => $"[label=\"{this.Id}\\n{this.ProbabilitySet.DotLabel}\"]";

        public override string ToString() => $"{this.Id} {this.ProbabilitySet.DotLabel}";

        public bool Equals(SampleNode other)
        {
            return this.Id == other.Id;
        }
    }
}