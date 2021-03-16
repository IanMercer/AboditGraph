namespace Abodit.Graph
{
    /// <summary>
    /// An edge with a probability assigned to it
    /// </summary>
    public interface IEdgeProbability
    {
        /// <summary>
        /// The value
        /// </summary>
        double Probability { get; }
    }
}