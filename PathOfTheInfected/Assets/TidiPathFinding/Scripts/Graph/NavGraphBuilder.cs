using System.Collections.Generic;

namespace TidiPathFinding
{
    public static class NavGraphBuilder
    {
        public static NavGraph BuildGraph(NavGraphData data)
        {
            NavGraph graph = new NavGraph(); // Create the graph

            // Create the runtime nodes
            var runtimeNodes = new List<NavNode>();

            foreach (var nodeData in data.nodesBakedData) // Loop through all the nodes
            {
                runtimeNodes.Add(graph.AddNode(nodeData.position)); // Add the node to the graph
            }

            // Connect the nodes
            for (int i = 0; i < data.nodesBakedData.Count; i++) // Loop through all the nodes
            {
                var nodeData = data.nodesBakedData[i]; // Get the node data
                var fromNode = runtimeNodes[i]; // Get the node

                foreach (var connectionIndex in nodeData.connections) // Loop through all the connections
                {
                    NavNode toNode = runtimeNodes[connectionIndex]; // Get the connected node
                    graph.Connect(fromNode, toNode); // Connect the nodes
                }
            }
            return graph;
        }
    }
}