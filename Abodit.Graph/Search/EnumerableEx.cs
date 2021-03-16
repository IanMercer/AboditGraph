using System;
using System.Collections.Generic;
using System.Linq;

namespace Abodit.Graph
{
    /// <summary>
    /// Enumerable extensions
    /// </summary>
    public static class EnumerableEx
    {
        /// <summary>
        /// Concat a value
        /// </summary>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> input, T value)
        {
            foreach (var x in input)
            {
                yield return x;
            }
            yield return value;
        }

        /// <summary>
        /// Choose n elements from input in all possible ways without duplicates
        /// </summary>

        public static IEnumerable<(T chosen, IEnumerable<T> notChosen)> ChooseOne<T>(this IEnumerable<T> inputEnumerable)
        {
            return inputEnumerable.Choose(1).Select(x => (x.chosen.First(), x.notChosen));
        }

        /// <summary>
        /// Choose n elements from input in all possible ways without duplicates
        /// </summary>

        public static IEnumerable<(IEnumerable<T> chosen, IEnumerable<T> notChosen)> Choose<T>(this IEnumerable<T> inputEnumerable, int n)
        {
            IList<T> input = inputEnumerable.ToList(); // prevent multiple enumeration of enumerable

            if (n == 0)
            {
                yield break;
            }
            else if (n > input.Count)
            {
                throw new ArgumentException($"Can't choose {n} from {input.Count}");
            }
            else if (n == input.Count)
            {
                yield return (input, Enumerable.Empty<T>());
                yield break;
            }
            else if (n == 1)
            {
                int c = 0;
                foreach (var x in input)
                {
                    int cc = c++; // capture loop variable
                    yield return (new[] { x }, input.Where((x, i) => i != cc));
                }
            }
            else
            {
                int c = 0;
                foreach (var x in input)
                {
                    int cc = c++; // capture loop variable
                    var permuteOthers = input.Where((x, i) => i != cc).Choose(n - 1);
                    foreach (var other in permuteOthers)
                    {
                        yield return (new[] { x }.Concat(other.chosen), other.notChosen);
                    }
                }
            }
        }

        /// <summary>
        /// Get current and previous from a sequence
        /// </summary>
        public static IEnumerable<(T current, T previous)> CurrentAndPrevious<T>(this IEnumerable<T> input)
            where T : notnull
        {
            bool first = true;
            T previous = default;
            using (var enumerator = input.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!first)
                    {
                        yield return (enumerator.Current!, previous!);
                    }
                    previous = enumerator.Current;
                    first = false;
                }

                // And the final element on the end with no previous
                if (!first)
                {
                    yield return (previous!, default!);
                }
            }
        }
    }
}