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

            public bool Equals(Impedance? other)
            {
                return other is Impedance imp && Math.Abs(this.Resistance - imp.Resistance) < 1E-9;
            }

            public override string ToString() => $"{this.Resistance}Ω";

            public override bool Equals(object? obj)
            {
                return Equals(obj as Impedance);
            }

            public override int GetHashCode()
            {
                return this.Resistance.GetHashCode();
            }
        }
    }
}