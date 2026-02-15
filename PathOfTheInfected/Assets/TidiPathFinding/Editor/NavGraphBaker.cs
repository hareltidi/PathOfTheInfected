using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TidiPathFinding
{
    public class NavGraphBaker : MonoBehaviour
    {
        [ColorUsage(true, true)]
        [SerializeField] private Color areaColor;
        public Vector2 areaSize = new (20, 20);
        public float nodeSpacing = 1.5f;
        public LayerMask obstacleMask;


        /// <summary>
        /// Bakes the nav graph and saves the data to a scriptable object of type NodesBakedData.
        /// </summary>
        public void Bake()
        {
            NavGraphData data = ScriptableObject.CreateInstance<NavGraphData>(); // Create a new scriptable object
            Vector2 origin = (Vector2)transform.position - (areaSize / 2); // Calculate the origin of the graph

            Dictionary<Vector2, int>
                positionToIndex =
                    new Dictionary<Vector2, int>(); // This dictionary will map world positions to node indices
            List<NodeBakedData> nodes = new List<NodeBakedData>(); // This list will contain all the nodes in the graph

            // Generate the nodes

            for (float x = 0; x <= areaSize.x; x += nodeSpacing) // Loop through all the x positions
            {
                for (float y = 0; y <= areaSize.y; y += nodeSpacing) // Loop through all the y positions
                {
                    Vector2 worldPos = origin + new Vector2(x, y); // Calculate the world position
                    worldPos =  new Vector2(
                        Mathf.Round(worldPos.x * 1000f) / 1000f,
                        Mathf.Round(worldPos.y * 1000f) / 1000f
                    ); // Round the world position to avoid floating point errors

                    if (Physics2D.OverlapCircle(worldPos, 0.2f,
                            obstacleMask)) // Check if there is an obstacle at this position
                        continue; // Skip this node if there is an obstacle

                    var nodeData = new NodeBakedData
                    {
                        position = worldPos
                    };

                    positionToIndex[worldPos] = nodes.Count; // Add the node to the dictionary
                    nodes.Add(nodeData); // Add the node to the list
                }
            }

            data.nodesBakedData = nodes; // Set the nodes on the data

            // Connect neighbors

            Vector2[] directions =
            {
                new( nodeSpacing, 0),
                new(-nodeSpacing, 0),
                new(0,  nodeSpacing),
                new(0, -nodeSpacing),

                new( nodeSpacing,  nodeSpacing),
                new(-nodeSpacing,  nodeSpacing),
                new( nodeSpacing, -nodeSpacing),
                new(-nodeSpacing, -nodeSpacing),
            };

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector2 pos = nodes[i].position;

                foreach (var dir in directions)
                {
                    Vector2 neighborPos = pos + dir;
                    neighborPos = new Vector2(Mathf.Round(neighborPos.x * 1000f) / 1000f,
                        Mathf.Round(neighborPos.y * 1000f) / 1000f); // Round the position to avoid floating point errors

                    if (positionToIndex.TryGetValue(neighborPos, out int neighborIndex))
                    {
                        if (!Physics2D.Linecast(pos, neighborPos, obstacleMask))
                        {
                            nodes[i].connections.Add(neighborIndex);
                        }
                    }
                }
            }


            SaveAsset(data); // Save the data
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = areaColor;

            Gizmos.DrawWireCube(
                transform.position,
                areaSize
            );
        }


#if UNITY_EDITOR
        /// <summary>
        /// Saves the provided navigation graph data as a Unity asset in the project.
        /// </summary>
        /// <param name="data">The navigation graph data to be saved as an asset.</param>
        private void SaveAsset(NavGraphData data)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save The Baked Nav Graph Data",
                "NavGraphBakedData",
                "asset",
                "Choose location to save the Baked data for the NavGraph."
            ); // Prompt the user to select a location to save the asset

            if (string.IsNullOrEmpty(path)) return; // Check if the user canceled the save operation if so, return


            AssetDatabase.CreateAsset(data, path); // Save the asset
            AssetDatabase.SaveAssets(); // Save all assets
            AssetDatabase.Refresh(); // Refresh the project view

            EditorUtility.DisplayDialog(
                "Nav Graph Baked",
                $"Nav Graph created successfully!\nNodes: {data.nodesBakedData.Count}",
                "OK"
            );
        }
    }
#endif
}
