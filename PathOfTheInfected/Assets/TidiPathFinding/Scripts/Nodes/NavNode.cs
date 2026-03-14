using System.Collections.Generic;
using UnityEngine;

namespace TidiPathFinding
{
    /// <summary>
    /// Represents a navigation node in the graph.
    /// Contains some pathfinding runtime data.
    /// </summary>
    public class NavNode
    {
        /// <summary>
        /// The position of this node in the graph.
        /// </summary>
        public Vector2 Position { get; }

        private readonly List<NavEdge> _edges = new();

        /// <summary>
        /// The collection of edges connected to this node in the navigation graph.
        /// Each edge represents a directional connection to another node with an associated traversal cost.
        /// </summary>
        public IReadOnlyList<NavEdge> Edges => _edges;

        /// <summary>
        /// Represents a navigation node in the graph that is used for pathfinding.
        /// </summary>
        /// <param name="position">The position of the node in the graph.</param>
        public NavNode(Vector2 position)
        {
            Position = position;
        }

        /// <summary>
        /// Adds a directional edge to another node.
        /// </summary>
        public void AddEdge(NavNode target, float cost)
        {
            if (target == null) return;

            _edges.Add(new NavEdge(this, target, cost)); // Add the edge to the list of edges
        }
    }
}
