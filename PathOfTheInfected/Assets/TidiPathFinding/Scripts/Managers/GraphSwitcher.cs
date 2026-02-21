using System;
using UnityEngine;

namespace TidiPathFinding
{
    public class GraphSwitcher : MonoBehaviour
    {
        [SerializeField] private NavGraphData nextGraphBakedData;
        [SerializeField] private bool drawNodes = true;
        [SerializeField] private bool drawConnections = true;
        [SerializeField] private float nodeRadius = 0.25f;
        [SerializeField] private int maxNodesToDraw = 1000;
        private void Awake()
        {
            SwitchGraph();
        }


        void SwitchGraph()
        {
            NavGraph newGraph = NavGraphBuilder.BuildGraph(nextGraphBakedData);
            AStarPathFinder.CurrentGraph = newGraph;
        }

        private void OnDrawGizmosSelected()
        {
            if (nextGraphBakedData == null) return;

            int count = Mathf.Min(nextGraphBakedData.nodesBakedData.Count, maxNodesToDraw);

            for (int i = 0; i < count; i++)
            {
                var node = nextGraphBakedData.nodesBakedData[i];
                if (drawNodes)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(node.position, nodeRadius);

                }
                if (drawConnections)
                {
                    Gizmos.color = Color.yellow;
                    foreach (int connection in node.connections)
                    {
                        Gizmos.DrawLine(
                            node.position,
                            nextGraphBakedData.nodesBakedData[connection].position
                        );
                    }
                }
            }
        }
    }
}