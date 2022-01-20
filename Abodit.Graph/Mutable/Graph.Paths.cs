using Abodit.Graph;
using Gapotchenko.FX.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Abodit.Mutable
{
    public partial class Graph<TNode, TRelation> where TNode : IEquatable<TNode>
        where TRelation : notnull, IEquatable<TRelation>
    {
        /// <summary>
        /// Shortest paths to everywhere with cumulative function from Source nodes
        /// </summary>
        public IEnumerable<IEnumerable<Path<TNode>>> EveryPossibleNonOverlappingPath(
            Func<TRelation, double> probabilityFunction,
            Func<TNode, int> order,   // To prevent going backwards by Id for example
            Func<double, double, double> accumulator,
            ILogger log
            )
        {
            return this.EveryPossibleNonOverlappingPath(this.Nodes.Where(x => !this.Back(x).Any()),
                probabilityFunction, order, accumulator, log);
        }

        /// <summary>
        /// Shortest paths to everywhere with cumulative function from Source nodes
        /// </summary>
        public IEnumerable<IEnumerable<Path<TNode>>> EveryPossibleNonOverlappingPath(
            IEnumerable<TNode> startNodes,
            Func<TRelation, double> probabilityFunction,
            Func<TNode, int> order,   // To prevent going backwards by Id for example
            Func<double, double, double> accumulator,
            ILogger log
            )
        {
            var nodesToVisit = this.Successors<TNode>(startNodes).Except(startNodes).OrderBy(n => order(n));

            var result = EveryPossibleNonOverlappingPath(nodesToVisit, startNodes, probabilityFunction, order, accumulator, log)
                .Memoize();

            if (result.Any()) return result;

            log.LogWarning($"Failed to find any paths");
            return Enumerable.Empty<IEnumerable<Path<TNode>>>();
        }

        /// <summary>
        /// Shortest paths to everywhere with cumulative function
        /// </summary>
        /// <remarks>
        /// Use the other method and let Graph figure out the start nodes
        /// </remarks>
        private IEnumerable<IEnumerable<Path<TNode>>> EveryPossibleNonOverlappingPath(
            IEnumerable<TNode> nodesToVisit,
            IEnumerable<TNode> startNodes,
            Func<TRelation, double> probabilityFunction,
            Func<TNode, int> order,   // To prevent going backwards by Id for example
            Func<double, double, double> accumulator,
            ILogger log
            )
        {
            log = log ?? NullLogger.Instance;
            // Default accumulator is to multiply probabilities, but could do adder on log probabiities
            accumulator = accumulator ?? new Func<double, double, double>((x, y) => x * y);

            var paths = startNodes.Select(s => new Path<PathNode>(new PathNode(s, Follow(s), probabilityFunction), 1.0)).ToList();

            var pathResults = PossiblePathsHelper(nodesToVisit, paths, probabilityFunction, order, accumulator, log, 0, null);

            return pathResults
                // Rip off the extra Edges stuff
                .Select(y => y.Select(z => z.Convert(q => q.Node)))!;
        }

        private static IEnumerable<Path<T>> Replace<T>(IEnumerable<Path<T>> source, Path<T> from, Path<T> to)
            where T : IEquatable<T>
        {
            foreach (var path in source)
            {
                if (object.ReferenceEquals(path, from))
                    yield return to;
                else
                    yield return path;
            }
        }

        /// <summary>
        /// Min item in sequence by value function
        /// </summary>
        private static T Min<T>(IEnumerable<T> source, Func<T, int> order)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            int value = int.MinValue;
            T result = default;
            bool hasValue = false;
            foreach (var x in source)
            {
                int o = order(x);
                if (hasValue)
                {
                    if (o < value) { value = o; result = x; }
                }
                else
                {
                    value = o;
                    result = x;
                    hasValue = true;
                }
            }
            if (hasValue) return result!;
            throw new Exception("No results for min");
        }

        /// <summary>
        /// Shortest paths to everywhere with cumulative function
        /// </summary>
        private IEnumerable<IEnumerable<Path<PathNode>>> PossiblePathsHelper(
            IEnumerable<TNode> nodesToVisit,
            List<Path<PathNode>> currentPaths,
            Func<TRelation, double> probabilityFunction,
            Func<TNode, int> order,
            Func<double, double, double> accumulator,
            ILogger log,
            int level = 0,
            Stopwatch? sw = null
            )
        {
            if (probabilityFunction is null) throw new ArgumentNullException(nameof(probabilityFunction));
            if (order is null) throw new ArgumentNullException(nameof(order));
            if (accumulator is null) throw new ArgumentNullException(nameof(accumulator));
            sw ??= Stopwatch.StartNew();

            var nextNode = nodesToVisit.First();
            var priors = Back(nextNode).ToList();
            var sumPriors = priors.Sum(x => probabilityFunction(x.Predicate));

            var seen = currentPaths.SelectMany(cp => cp.Ancestors.Select(a => a.Current.Node)).ToList();

            // Find all possible paths to next node that we can go to from here ranked by cumuative next score
            var nexts = currentPaths
                .SelectMany(cp => cp.Current.Edges
                    .Where(e => !seen.Any(s => order(s) == order(e.End)))
                    .Select(e => (cp: cp, edge: e, cumProb: accumulator(cp.Score, probabilityFunction(e.Predicate) / sumPriors))))
               .OrderByDescending(cpx => cpx.cumProb)
               .Where(x => order(x.edge.End) == order(nextNode))
               //.Where(cpx => cpx.cumProb > 0.0005)
               .ToList();

            if (!nexts.Any())
            {
                yield break;
            }

            // If we choose highest prio first and it finds a successor on one node
            // and then we choose next highest prio and it extends a different node
            // but then we come back and extend the first node again ... we create duplicates
            // backtracking is hard!

            double best = double.NaN;
            double cutoff = 0.5;  // percentage of best

            foreach (var next in nexts)
            {
                if (best == double.NaN)
                {
                    best = next.cumProb;
                }
                else if (best * cutoff > next.cumProb)
                {
                    continue;
                }

                var newPathNode = new PathNode(next.edge.End, Follow(next.edge.End), probabilityFunction);
                var nextPaths = Replace(currentPaths, next.cp, next.cp.Extend(newPathNode, next.cumProb)).ToList();

                if (sw.ElapsedTicks > TimeSpan.TicksPerSecond * 1)
                {
                    log.LogDebug($"Taking too long {sw.ElapsedMilliseconds}ms");
                }

                string spaces = new string('>', level);

                if (nextPaths.All(x => !x.Current.Edges.Any()))
                {
                    // Succcess, all paths are allocated! And they all go to the end
                    // Save one recursion step
                    //log.Debug($"\n{spaces} Shortcut:\n{spaces}  " + string.Join($"\n{spaces}  ", nextPaths.Select(z => z.Convert(q => q.Node))));
                    yield return nextPaths;
                    yield break;
                }

                //log.Debug($"\n{spaces} Recursion:\n{spaces}  " + string.Join($"\n{spaces}  ", nextPaths.Select(z => z.Convert(q => q.Node))));

                var seen2 = new List<Path<PathNode>>();

                foreach (var result in this.PossiblePathsHelper(nodesToVisit.Skip(1), nextPaths, probabilityFunction, order, accumulator, log, level + 1, sw))
                {
                    if (result.All(r => seen2.Contains(r)))
                    {
                        //log.Debug($"All of these paths have already been seen");
                        continue;
                    }

                    if (level == 0)
                    {
                        // otherwise it's just unwinding the stack
                        //log.Debug($"\n{spaces} Result:\n{spaces}  " + string.Join($"\n{spaces}  ", result.Select(z => z.Convert(q => q.Node))));
                    }
                    yield return result;
                    seen2.AddRange(result);
                }

                if (sw.ElapsedTicks > TimeSpan.TicksPerSecond * 2)
                {
                    log.LogDebug($"Giving up {sw.ElapsedMilliseconds}ms");
                    sw.Reset();
                    break;
                }
            }

            // Could not find a path to extend all the paths to the end, backtrack
            yield break;
        }
    }
}