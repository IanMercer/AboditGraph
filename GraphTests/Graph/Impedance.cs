using System;
using Abodit.Graph;

namespace Abodit.Units.Tests.Graph
{
    public partial class GraphTestPathFinding
    {
        public class Impedance : IRelation, IEquatable<Impedance>
        {
            public double Resistance { get; }

            public Impedance(double resistance)
            {
                this.Resistance = resistance;
            }

            public bool IsReflexive => false;

            public bool Equals(Impedance other)
            {
                return Math.Abs(this.Resistance - other.Resistance) < 1E-9;
            }

            public override string ToString() => $"{this.Resistance}Ω";
        }
    }
}