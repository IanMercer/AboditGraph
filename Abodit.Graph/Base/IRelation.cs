namespace Abodit.Graph
{
    /// <summary>
    /// An optional interface that Relations in Graphs may implement to indicate reflexive relationships
    /// </summary>
    /// <remarks>
    /// This is a convenience that allows a statement to be added one way and to automatically be added
    /// in the other direction too.
    /// </remarks>
    public interface IRelation
    {
        /// <summary>
        /// Is this relationship two-way?
        /// </summary>
        bool IsReflexive { get; }
    }
}