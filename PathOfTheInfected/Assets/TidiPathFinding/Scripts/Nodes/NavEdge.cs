namespace TidiPathFinding
{
    /// <summary>
    /// Represents a directional connection between two nodes.
    /// </summary>
    public class NavEdge
    {
        /// <summary>
        /// The node this edge originates from.
        /// </summary>
        public NavNode From { get; }
        /// <summary>
        /// The node this edge points to.
        /// </summary>
        public NavNode To { get; }
        /// <summary>
        /// The traversal cost of this edge.
        /// </summary>
        public float Cost { get; }

        /// <summary>
        /// Represents a directional connection between two nodes in a navigation graph.
        /// </summary>
        /// <param name="from">The node this edge originates from.</param>
        /// <param name="to">The node this edge points to.</param>
        /// <param name="cost">The traversal cost of this edge.</param>
        public NavEdge(NavNode from, NavNode to, float cost)
        {
            From = from;
            To = to;
            Cost = cost;
        }
    }
}
