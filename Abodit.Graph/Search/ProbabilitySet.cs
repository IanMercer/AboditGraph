using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph
{
    /// <summary>
    /// A set of items and their probabilities
    /// </summary>
    /// <remarks>
    /// This isn't really part of the Graph library, it's here for some tests that use it, please ignore.
    /// </remarks>
    public class ProbabilitySet<TItem> : IProbabilitySet<TItem>, IEnumerable<KeyValuePair<TItem, double>>
        where TItem : IEquatable<TItem>
    {
        private IDictionary<TItem, double> nodeProbabilities;

        /// <inheritdoc />
        public IDictionary<TItem, double> NodeProbabilities => nodeProbabilities;

        /// <summary>
        /// Creates a new <see cref="ProbabilitySet{TPerson}"/>
        /// </summary>
        public ProbabilitySet(IDictionary<TItem, double> nodeProbabilities)
        {
            if (nodeProbabilities.Any(x => double.IsNaN(x.Value))) throw new ArgumentException("One of values is NaN", nameof(nodeProbabilities));
            this.nodeProbabilities = nodeProbabilities;
        }

        /// <summary>
        /// Creates a new <see cref="ProbabilitySet{TItem}"/>
        /// </summary>
        public ProbabilitySet()
        {
            this.nodeProbabilities = new Dictionary<TItem, double>();
        }

        /// <summary>
        /// An empty probability set
        /// </summary>
        public static readonly ProbabilitySet<TItem> Empty = new ProbabilitySet<TItem>();

        /// <summary>
        /// Creates a new <see cref="ProbabilitySet{TItem}"/>
        /// </summary>
        public static ProbabilitySet<TItem> Create(IEnumerable<(ProbabilitySet<TItem> set, double prob)> list)
        {
            return list.Aggregate(Empty, (x, y) => x.Add(y.set.Multiply(y.prob)));
        }

        /// <summary>
        /// Creates a new <see cref="ProbabilitySet{TItem}"/>
        /// </summary>
        public ProbabilitySet(TItem item)
        {
            this.nodeProbabilities = new Dictionary<TItem, double>() { [item] = 1.0 };
        }

        /// <summary>
        /// Adds two independent sets
        /// </summary>
        public static ProbabilitySet<TItem> operator +(ProbabilitySet<TItem> one, ProbabilitySet<TItem> two)
            => one.Add(two);

        /// <summary>
        /// Subtracts two independent sets
        /// </summary>
        public static ProbabilitySet<TItem> operator -(ProbabilitySet<TItem> one, ProbabilitySet<TItem> two)
            => one.Minus(two);

        /// <summary>
        /// Divide by a constant
        /// </summary>
        public static ProbabilitySet<TItem> operator /(ProbabilitySet<TItem> one, double value)
            => one.Multiply(1.0 / value);

        /// <summary>
        /// Multiply by a constant
        /// </summary>
        public static ProbabilitySet<TItem> operator *(ProbabilitySet<TItem> one, double value)
            => one.Multiply(value);

        /// <summary>
        /// Multiply by a constant
        /// </summary>
        public static ProbabilitySet<TItem> operator *(double value, ProbabilitySet<TItem> one)
            => one.Multiply(value);

        ///<summary>
        /// Unary minus
        /// </summary>
        public static ProbabilitySet<TItem> operator -(ProbabilitySet<TItem> one)
            => one.Multiply(-1);

        /// <summary>
        /// Adds two node probability sets
        /// </summary>
        public ProbabilitySet<TItem> Add(ProbabilitySet<TItem> two)
        {
            var newList = this.nodeProbabilities.ToDictionary(x => x.Key, x => x.Value);  // clone

            foreach (var p in two.NodeProbabilities)
            {
                if (newList.ContainsKey(p.Key))
                {
                    newList[p.Key] += p.Value;
                }
                else
                {
                    newList[p.Key] = p.Value;
                }
            }

            return new ProbabilitySet<TItem>(newList);
        }

        /// <summary>
        /// Choose N of this probability set using fact that each key value pair is independent but cannot be chosen more than once
        /// </summary>
        public ProbabilitySet<TItem> ChooseN(int n)
        {
            return (n == 1) ? this : this * n;
        }

        /// <summary>
        /// Or probability sets together using p(a|b) = p(a) + p(b) - p(a).p(b)
        /// </summary>
        public static ProbabilitySet<TItem> Or(IEnumerable<ProbabilitySet<TItem>> input)
        {
            var first = input.First();
            foreach (var item in input.Skip(1))
            {
                first = first.Or(item);
            }
            return first;
        }

        /// <summary>
        /// Combines two node probability sets p(A|B) = p(A) + p(B) - p(A)p(B)
        /// </summary>
        public ProbabilitySet<TItem> Combine(ProbabilitySet<TItem> two)
        {
            var newList = this.nodeProbabilities.ToDictionary(x => x.Key, x => x.Value);  // clone

            foreach (var p in two.NodeProbabilities)
            {
                if (newList.ContainsKey(p.Key))
                {
                    newList[p.Key] += p.Value - p.Value * newList[p.Key];
                }
                else
                {
                    newList[p.Key] = p.Value;
                }
            }

            return new ProbabilitySet<TItem>(newList);
        }

        /// <summary>
        /// Or two node probability sets p(A|B) = p(A) + p(B) - p(A)p(B)
        /// </summary>
        /// <remarks>
        /// Synonym for Combine
        /// </remarks>
        public ProbabilitySet<TItem> Or(ProbabilitySet<TItem> two)
        {
            var newList = this.nodeProbabilities.ToDictionary(x => x.Key, x => x.Value);  // clone

            foreach (var p in two.NodeProbabilities)
            {
                if (newList.ContainsKey(p.Key))
                {
                    newList[p.Key] += p.Value - p.Value * newList[p.Key];
                }
                else
                {
                    newList[p.Key] = p.Value;
                }
            }

            return new ProbabilitySet<TItem>(newList);
        }

        /// <summary>
        /// Combine a series of probability sets using p(A|B) = p(A) + p(B) - p(A)*p(B)
        /// </summary>
        public static ProbabilitySet<TItem> Aggregate(IEnumerable<ProbabilitySet<TItem>> input)
        {
            return input.Aggregate(new ProbabilitySet<TItem>(), (x, y) => (x.Add(y)).Minus(x.SameProduct(y)));
        }

        /// <summary>
        /// Ensures that this set has one member for each of the other set
        /// </summary>
        public ProbabilitySet<TItem> Harmonize(ProbabilitySet<TItem> two)
        {
            var newList = this.nodeProbabilities.ToDictionary(x => x.Key, x => x.Value);  // clone

            bool changed = false;
            foreach (var p in two.NodeProbabilities)
            {
                if (!newList.ContainsKey(p.Key))
                {
                    changed = true;
                    newList[p.Key] = 0.0;
                }
            }

            if (!changed) return this;
            return new ProbabilitySet<TItem>(newList);
        }

        /// <summary>
        /// Replace the probability set with updated values
        /// </summary>
        public void Replace(ProbabilitySet<TItem> probabilitySet)
        {
            if (probabilitySet.NodeProbabilities.Any(x => double.IsNaN(x.Value))) throw new ArgumentException("One of values is NaN", nameof(probabilitySet));
            this.nodeProbabilities = probabilitySet.nodeProbabilities;
        }

        /// <summary>
        /// Dot product of two node probability sets (Ai x Bi + ... An x Bn)
        /// </summary>
        public double DotProduct(ProbabilitySet<TItem> two)
        {
            double result = 0.0;
            foreach (var p in two.NodeProbabilities)
            {
                if (this.nodeProbabilities.ContainsKey(p.Key))
                {
                    result += this.nodeProbabilities[p.Key] * p.Value;
                }
            }
            // All mismatched values go to zero
            return result;
        }

        /// <summary>
        /// Same element product of two node probability sets (Ai x Bi, ... An x Bn)
        /// </summary>
        public ProbabilitySet<TItem> SameProduct(ProbabilitySet<TItem> two)
        {
            var newList = new Dictionary<TItem, double>();
            foreach (var p in two.NodeProbabilities)
            {
                if (this.nodeProbabilities.ContainsKey(p.Key))
                {
                    newList[p.Key] = this.nodeProbabilities[p.Key] * p.Value;
                }
            }
            return new ProbabilitySet<TItem>(newList);
        }

        /// <summary>
        /// Sum squares difference (Ai - Bi) ^ 2 + ... (An x Bn) ^ 2
        /// </summary>
        public double SumSquaredDifference(ProbabilitySet<TItem> two)
        {
            double result = 0.0;
            foreach (var key in this.nodeProbabilities.Keys.Union(two.NodeProbabilities.Keys))
            {
                if (this.nodeProbabilities.ContainsKey(key) && two.NodeProbabilities.ContainsKey(key))
                {
                    result += this.nodeProbabilities[key] * two.NodeProbabilities[key];
                }
                else if (this.nodeProbabilities.ContainsKey(key))
                {
                    result += this.nodeProbabilities[key] * this.nodeProbabilities[key];
                }
                else if (two.NodeProbabilities.ContainsKey(key))
                {
                    result += two.NodeProbabilities[key] * two.NodeProbabilities[key];
                }
            }
            return result;
        }

        /// <summary>
        /// Subtracts two node probability sets
        /// </summary>
        public ProbabilitySet<TItem> Minus(ProbabilitySet<TItem> two)
        {
            var newList = this.nodeProbabilities.ToDictionary(x => x.Key, x => x.Value);  // clone

            foreach (var p in two.NodeProbabilities)
            {
                if (newList.ContainsKey(p.Key))
                {
                    newList[p.Key] -= p.Value;
                }
                else
                {
                    newList[p.Key] = -p.Value;
                }
            }

            return new ProbabilitySet<TItem>(newList);
        }

        /// <summary>
        /// Magnitude
        /// </summary>
        public double Magnitude => this.nodeProbabilities.Sum(x => Math.Abs(x.Value));

        /// <summary>
        /// Are there any probabilities in the set?
        /// </summary>
        /// <returns></returns>
        public bool Any() => this.nodeProbabilities.Any();

        /// <inheritdoc />
        public ProbabilitySet<TItem> Multiply(double value)
        {
            var newList = this.nodeProbabilities.ToDictionary(x => x.Key, x => x.Value * value);
            return new ProbabilitySet<TItem>(newList);
        }

        /// <inheritdoc />
        public string DotLabel => string.Join("\\n", this.nodeProbabilities.OrderByDescending(x => x.Value).Select(n => $"{n.Key} : {n.Value:0.000}"));

        /// <inheritdoc />
        public override string ToString() => string.Join("|", this.nodeProbabilities.Select(n => $"{n.Key}").OrderBy(x => x));

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TItem, double>> GetEnumerator()
        {
            return this.nodeProbabilities.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.nodeProbabilities.GetEnumerator();
        }
    }
}