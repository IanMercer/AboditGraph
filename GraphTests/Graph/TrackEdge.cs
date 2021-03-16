//using System;
//using Abodit.Graph;

//namespace Abodit.Units.Tests.Graph
//{
//    public partial class GraphTestPathFinding
//    {
//        /// <summary>
//        /// An edge between two <see cref="ClusterTrack"/> nodes
//        /// </summary>
//        public class TrackEdge : IRelation, IEquatable<TrackEdge>, IEdgeProbability, IDotGraphEdge
//        {
//            public double Probability { get; }

//            /// <summary>
//            /// Edge is one-way
//            /// </summary>
//            public bool IsReflexive => false;

//            public string DotLabel => $"{this.Probability:0.000}";

//            public TrackEdge(double probability)
//            {
//                Probability = probability;
//            }

//            public bool Equals(TrackEdge other)
//            {
//                return this.Probability.Equals(other.Probability);
//            }

//            public override string ToString()
//            {
//                return $"{this.Probability:0.000}";
//            }
//        }
//    }
//}