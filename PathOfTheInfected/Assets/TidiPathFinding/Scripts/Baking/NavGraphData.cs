using System;
using System.Collections.Generic;
using UnityEngine;

namespace TidiPathFinding
{
    /// <summary>
    /// struct for storing baked data of a node
    /// </summary>
    [Serializable]
    public class NodeBakedData
    {
        public Vector2 position;
        public List<int> connections = new();
    }



    /// <summary>
    /// A scriptable object that stores all the baked data of the graph.
    /// </summary>
    [CreateAssetMenu(fileName = "NavGraphData", menuName = "TidiPathfinding/BakedData/NavGraphData", order = 0)]
    public class NavGraphData : ScriptableObject
    {
        /// <summary>
        /// List of baked data of all the nodes in the graph.
        /// </summary>
        public List<NodeBakedData> nodesBakedData;
    }
}