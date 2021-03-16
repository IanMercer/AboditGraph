# AboditGraph
Graph library for RDF-like in-memory graphs. Efficient either-direction traversals. Provides depth-first, 
breadth-first and random-first traversals, topological sort, path finding, and page rank algorithms. Supports 
Dotgraph for rendering output which is very helpful when debugging.

![image](https://user-images.githubusercontent.com/347540/111330804-7fc1f500-862d-11eb-9382-1d1bb768146c.png)

Create a graph using RDF-like predicates on nodes of any type.

````csharp
    var g = new Graph<string, Relation>();
    g.AddStatement("a", Relation.RDFSType, "b");
    g.AddStatement("a", Relation.RDFSType, "c");
    g.AddStatement("b", Relation.RDFSType, "d");
    g.AddStatement("c", Relation.RDFSType, "e");
    g.AddStatement("d", Relation.RDFSType, "f");
    g.AddStatement("e", Relation.RDFSType, "f");
````

And then traverse it, union it, intersect it, find paths, topological sorts, page rank, ...

````csharp
    var sorted = string.Join("", g.TopologicalSortApprox());
    sorted.Should().Be("abcdef");

    var path = g.ShortestPath(a, Relation.RDFSType, e, (r) => 1.0, (x, y) => x + y);
    string result = string.Join(";", path);
    result.Should().Be("a;c;e");
````

Why create another graph library?
----

1. Performance when traversing a graph in both directions.
2. Full control over Node and Edge type including the ability to use Nodes as Edges in true RDF 'turtles-all-the-way-down' fashion.
3. Compact in memory representation for very large in-memory graphs.
4. Immutable data structures wherever possible.
5. Complete set of algorithms including exploration, path finding, union, topological sort and page rank.
6. Support for Dotgraph rendering of graphs.


Where's the nuget package?
----
I haven't published one yet, but if you ping me on Twitter I will.

