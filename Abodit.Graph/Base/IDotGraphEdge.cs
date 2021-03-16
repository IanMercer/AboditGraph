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
}