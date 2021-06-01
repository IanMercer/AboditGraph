namespace Abodit.Graph
{
    /// <summary>
    /// An edge that can render nicely as a DotGraph
    /// </summary>
    public interface IDotGraphEdge
    {
        /// <summary>
        /// A label for the edge
        /// </summary>
        string DotLabel { get; }
    }

    /// <summary>
    /// An edge can be styled with a dot graph style, e.g. dotted
    /// </summary>
    public interface IDotGraphEdgeStyle
    {
        /// <summary>
        /// Gets the style for the edge
        /// </summary>
        DotGraphEdgeStyle Style { get; }
    }

    /// <summary>
    /// An edge can be colored, e.g. "red" or "#E04040"
    /// </summary>
    public interface IDotGraphEdgeColor
    {
        /// <summary>
        /// Gets the color for the edge
        /// </summary>
        string Color { get; }
    }

    /// <summary>
    /// An edge can have a thickness
    /// </summary>
    public interface IDotGraphEdgeThickness
    {
        /// <summary>
        /// Gets the thickness for the edge
        /// </summary>
        int Thickness { get; }
    }
}