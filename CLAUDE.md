# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AboditGraph is a high-performance, RDF-like in-memory graph library for .NET. It provides efficient bidirectional graph traversals, multiple search algorithms, path finding, topological sorting, and PageRank. Available on NuGet as `Abodit.Graph`.

## Commands

```bash
# Build
dotnet build Abodit.Graph.sln

# Run all tests
dotnet test Abodit.Graph.sln

# Run a specific test class
dotnet test Abodit.Graph.sln --filter ClassName=AboditUnitsTest.GraphTests

# Pack NuGet (also happens automatically on build)
dotnet pack Abodit.Graph/Abodit.Graph.csproj
```

## Architecture

The library has two parallel implementations under `Abodit.Graph/`:

- **`Mutable/Graph<TNode, TRelation>`** — mutations return `bool` and modify in place
- **`Immutable/Graph<TNode, TRelation>`** — mutations return new graph instances (functional API); requires `TNode : class`

Both extend **`Base/GraphBase<TNode, TRelation>`**, which contains all traversal and search logic. Constraints on the generics: `TNode : IEquatable<TNode>`, `TRelation : notnull, IEquatable<TRelation>`.

**Reflexive edges:** when a predicate implements `IRelation` and `IsReflexive == true`, both `AddStatement` and `RemoveStatement` automatically add/remove the reverse edge. Do not call them twice manually.

### Data storage (dual-indexed)

Edges are stored in two indices inside `GraphBase`:
- **Forward index** keyed by start node → `PredicateNext` linked list
- **Backward index** keyed by end node → `PredicatePrevious` linked list

This gives O(1) start-node and end-node lookups for bidirectional traversal without separate passes.

### Key abstractions

| Type | Purpose |
|------|---------|
| `Edge` (struct) | An (start, predicate, end) triple |
| `Path<TNode>` | Linked-list path with a `Score`; extended via `Extend()` |
| `Relation` | Concrete edge type; singleton/identity-mapped; supports reflexive (bidirectional) edges via `IsReflexive` |
| `IRelation` | Optional interface for edge types; only property is `bool IsReflexive` |
| `ISearchOrder<T>` | Strategy for traversal order (DFS/BFS/random/best-first) in `Search/` |
| `IDotGraphNode` | Implement on `TNode` to control Dotgraph visualization output |

### Search algorithms (`Search/`)

`GraphBase.Search<T>()` accepts any `ISearchOrder<T>`:
- `DepthFirstSearch<T>`
- `BreadthFirstSearch<T>`
- `RandomFirstSearch<T>`
- `BestFirstSearch<T>`

Extension methods on `GraphBase` add: topological sort (`TopologicalSortApprox` — cycles are silently skipped, not thrown), PageRank, path finding (`ShortestPath`), and distance calculations (`DistanceToEverywhere`). Both path-finding extensions use `PriorityQueue<T,T>` (net6+ only) for O((n+e) log n) Dijkstra.

### Visualization

Call `.DotGraph` (string property) on either graph type to get a Graphviz dot-format string. Implement `IDotGraphNode` / `IDotGraphEdge*` interfaces on node/edge types for custom styling.

## Test Project

Tests are in `Abodit.Graph.Tests/` (targets net9.0) using MSTest + FluentAssertions. Key test files:
- `GraphTests.cs` — core operations (union, intersect, follow, siblings)
- `GraphTest.cs` — additional mutable/immutable graph tests
- `GraphTestPathFinding.cs` — path-finding scenarios
- `GraphCoreOpsTests.cs` — edge cases (reflexive edges, self-loops, node removal, empty graphs)
- `GraphAlgorithmTests.cs` — algorithm correctness (TopologicalSort, PageRank, ShortestPath, DistanceToEverywhere)
- `GraphStressTests.cs` — performance sanity tests with generous time bounds

Helper domain types used in tests: `Person`, `SampleNode`, `Impedance`, `ProbabilitySet`.
