using System.Collections.Generic;
using UnityEngine;

namespace TidiPathFinding
{
    /// <summary>
    /// Represents a navigation graph as a pure data container.
    /// </summary>
    public class NavGraph
    {
        private readonly List<NavNode> _nodes = new();
        public IReadOnlyList<NavNode> Nodes => _nodes;

        /// <summary>
        /// Adds a new node to the graph at the specified position and returns it.
        /// </summary>
        /// <param name="position">Where the node should be placed.</param>
        /// <returns>The newly created node.</returns>
        public NavNode AddNode(Vector2 position)
        {
            var node = new NavNode(position);
            _nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Connects two nodes in the graph.
        /// </summary>
        /// <param name="from">The starting node we want to connect from.</param>
        /// <param name="to">The target node to connect to.</param>
        public void Connect(NavNode from, NavNode to)
        {
            if (!_nodes.Contains(from) || !_nodes.Contains(to))
            {
                Debug.LogError("Both nodes must be part of the graph to connect them.");
                return;
            }
            float cost = Vector2.Distance(from.Position, to.Position); // calculate the cost of the connection
            from.AddEdge(to, cost); // add the edge to the starting node
        }

        /// <summary>
        /// Creates a bidirectional connection between two navigation nodes, allowing traversal in both directions.
        /// </summary>
        /// <remarks>This method establishes connections from nodeA to nodeB and from nodeB to nodeA. Use
        /// this method when both nodes should be reachable from each other in the navigation graph.</remarks>
        /// <param name="nodeA">The first navigation node to connect. Cannot be null.</param>
        /// <param name="nodeB">The second navigation node to connect. Cannot be null.</param>
        public void ConnectBidirectional(NavNode nodeA, NavNode nodeB)
        {
            if (nodeA == null || nodeB == null)
            {
                Debug.LogError("Cannot connect null nodes. Both nodeA and nodeB must be valid NavNode instances.");
                return;
            }
            if (!ContainsNode(nodeA) || !ContainsNode(nodeB))
            {
                Debug.LogError("Both nodes must be part of the graph to connect them.");
                return;
            }

            // connect nodes in both directions
            Connect(nodeA, nodeB);
            Connect(nodeB, nodeA);
        }

        /// <summary>
        /// Finds the closest node in the graph to a given position. This is useful for determining which node an agent should start from or target when navigating to a specific point in the world.
        /// </summary>
        /// <param name="position">The position to check against the nodes in the graph.</param>
        /// <returns>The closest node to the specified position.</returns>

        public NavNode FindClosestNode(Vector2 position)
        {
            NavNode closestNode = null; // Initialize to null
            float closestDistance = float.MaxValue; // Initialize to a very large value
            foreach (var node in _nodes) // Loop through all nodes in the graph
            {
                float distance = Vector2.Distance(position, node.Position); // Calculate the distance between the position and the node
                if (distance < closestDistance) // Check if this distance is less than the previous closest distance
                {
                    closestDistance = distance; // Update the closest distance if true
                    closestNode = node; // Update closest node if true
                }
            }
            return closestNode;
        }

        /// <summary>
        /// Clears all nodes and edges from the graph, effectively resetting it to an empty state. Use this method when you want to completely rebuild the graph from scratch or when you no longer need the existing graph data.
        /// </summary>
        public void Clear()
        {
            _nodes.Clear(); // clear the list of nodes
        }

        /// <summary>
        /// Helper method to check if a given node is part of the graph. This can be useful for validating inputs before performing operations like connecting nodes or finding paths.
        /// </summary>
        /// <param name="node">The node to check if it is part of the graph.</param>
        /// <returns>True if the node is part of the graph, false otherwise.</returns>
        public bool ContainsNode(NavNode node)
        {
            return _nodes.Contains(node); // returns the result of the Contains method on the internal list of nodes
        }
    }
}
