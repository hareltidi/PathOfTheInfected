using System.Collections.Generic;
using UnityEngine;

namespace TidiPathFinding
{
    /// <summary>
    /// Runtime A* Path Finder algorithm implementation.
    /// </summary>
    public static class AStarPathFinder
    {

        public static NavGraph CurrentGraph = new();

        #region Path finding methods

        /// <summary>
        /// Calculates the shortest path between two points in a navigation graph using the A* algorithm.
        /// </summary>
        /// <param name="graph">The navigation graph containing nodes and edges used for pathfinding.</param>
        /// <param name="startWorld">The world-space coordinate for the starting point of the path.</param>
        /// <param name="goalWorld">The world-space coordinate for the goal point of the path.</param>
        /// <returns>A list of Vector2 representing the path from the start point to the goal point.
        /// Returns null if no valid path is found or if the start or goal nodes are invalid.</returns>
        public static List<Vector2> FindPath(NavGraph graph, Vector2 startWorld, Vector2 goalWorld)
        {
            NavNode startNode = graph.FindClosestNode(startWorld); // find the closest node to the start point
            NavNode goalNode = graph.FindClosestNode(goalWorld); // find the closest node to the goal point
            if (startNode == null || goalNode == null) return null;
            var openList = new List<NodeRecord>(); // This list will contain all the nodes that are currently being evaluated
            var closedSet = new HashSet<NavNode>(); // This Hash Set will contain all the nodes that have already been evaluated
            var allRecords = new Dictionary<NavNode, NodeRecord>(); // This dictionary will contain all the node records

            NodeRecord startRec = new NodeRecord
            {
                Node = startNode,
                GCost = 0,
                HCost = Vector2.Distance(startNode.Position, goalNode.Position),
                CameFrom = null
            };

            openList.Add(startRec);
            allRecords[startNode] = startRec;

            while (openList.Count > 0) // while the open list is not empty (meaning we have not yet found a path)
            {
                NodeRecord current = GetLowestF(openList);

                if (current.Node == goalNode) // if we have reached the goal node
                {
                    return ReconstructPath(current); // return the path
                }

                openList.Remove(current); // remove the current node from the open list
                closedSet.Add(current.Node); // add the current node to the closed set

                foreach (var edge in current.Node.Edges)
                {
                    NavNode neighbor = edge.To;
                    if (closedSet.Contains(neighbor)) // if the neighbor has already been visited
                    {
                        continue;
                    }
                    float tentativeG = current.GCost + edge.Cost; // Set a new tentative G cost for the neighbor
                    if (!allRecords.TryGetValue(neighbor, out NodeRecord neighborRecord))
                    {
                        neighborRecord = new NodeRecord
                        {
                            Node = neighbor
                        };
                        allRecords[neighbor] = neighborRecord;
                    }

                    if (!openList.Contains(neighborRecord) || tentativeG < neighborRecord.GCost) // if the neighbor is not in the open list or has a lower G cost
                    {
                        neighborRecord.GCost = tentativeG;
                        neighborRecord.HCost = Vector2.Distance(neighbor.Position, goalNode.Position);
                        neighborRecord.CameFrom = current;

                        if (!openList.Contains(neighborRecord)) // if the neighbor is not already in the open list
                        {
                            openList.Add(neighborRecord); // add it to the open list
                        }
                    }
                }
            }
            return null;

        }


        /// <summary>
        /// Wrapper for FindPath that uses the current graph.
        /// </summary>
        /// <param name="startWorld"></param>
        /// <param name="goalWorld"></param>
        /// <returns></returns>
        public static List<Vector2> FindPath_CurrentGraph(Vector2 startWorld, Vector2 goalWorld)
        {
            return FindPath(CurrentGraph, startWorld, goalWorld);
        }

        #endregion


        /// <summary>
        /// Returns the node record with the lowest F cost from the given list.
        /// </summary>
        /// <param name="list">The list to check on</param>
        /// <returns></returns>
        private static NodeRecord GetLowestF(List<NodeRecord> list)
        {
            NodeRecord best = list[0]; // set the best node to the first node in the list

            for (int i = 1; i < list.Count; i++) // Loop from index 1 to the end of the list to find the lowest F cost
            {
                if (list[i].FCost < best.FCost) // if the current node has a lower F cost than the best node
                {
                    best = list[i]; // set the best node to the current node
                }
            }

            return best; // return the best node
        }

        /// <summary>
        /// Reconstructs the path from the start node to the goal node by tracing back through the node records.
        /// </summary>
        /// <param name="endRecord">The final node record representing the goal node, from which the path reconstruction begins.</param>
        /// <returns>A list of Vector2 points representing the reconstructed path in world-space coordinates,
        /// starting from the goal and ending at the start node.</returns>
        private static List<Vector2> ReconstructPath(NodeRecord endRecord)
        {
            List<Vector2> path = new List<Vector2>(); // The list that will contain the reconstructed path
            NodeRecord current = endRecord; // Start tracing back from the goal node

            while (current != null) // while we have not reached the start node (since the start node does not have a cameFrom property)
            {
                path.Add(current.Node.Position); // add the position of the current node to the path
                current = current.CameFrom; // move to the node that came before the current node (until a null meaning the start node was reached)
            }

            path.Reverse(); // reverse the path so that it starts at the start node
            return path;
        }
    }
}