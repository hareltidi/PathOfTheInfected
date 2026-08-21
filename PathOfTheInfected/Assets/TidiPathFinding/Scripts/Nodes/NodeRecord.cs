

namespace TidiPathFinding
{
    public class NodeRecord
    {
        // A* runtime data

        public NavNode Node { get; set; }

        /// <summary>
        /// The cost of the path from the start node to this node.
        /// </summary>
        public float GCost { get; set; }

        /// <summary>
        /// The estimated cost of the path from this node to the goal.
        /// </summary>
        public float HCost { get; set; }

        /// <summary>
        /// The node from which this node was reached.
        /// </summary>
        public NodeRecord CameFrom { get; set; }

        /// <summary>
        /// The total cost of the path from the start node to the goal.
        /// </summary>
        public float FCost => GCost + HCost;

    }
}