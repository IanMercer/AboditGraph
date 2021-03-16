namespace Abodit.Graph
{
    /// <summary>
    /// A node that can render nicely as a DotGraph
    /// </summary>
    public interface IDotGraphNode
    {
        /// <summary>
        /// A unique ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// A label for the node
        /// </summary>
        string DotProperties { get; }

        /// <summary>
        /// Have priors been pruned?
        /// </summary>
        bool IsPruned { get; }

        /// <summary>
        /// Displays in first column in dot graph visualization
        /// </summary>
        bool IsStartNode { get; }
    }
}