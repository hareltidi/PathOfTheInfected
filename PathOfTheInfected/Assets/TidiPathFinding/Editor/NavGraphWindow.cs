using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TidiPathFinding.Editor
{
    public class NavGraphWindow : EditorWindow
    {
        private Vector2 _areaSize = new(20, 20);
        private float _nodeSpacing = 1.5f;
        private int _obstacleLayer; // Store the index of the layer
        private Vector2 _centerPoint = Vector2.zero;
        [ColorUsage(true, true)]
        private Color _areaColor = new(0, 1, 0, 0.3f);

        [MenuItem("Tools/Tidi Pathfinding/Navigation Graph Baker tool")]
        public static void ShowWindow() => GetWindow<NavGraphWindow>("Tidi Pathfinding Navigation Graph Baker Tool");

        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnGUI()
        {
            GUILayout.Label("Baking Settings", EditorStyles.boldLabel);
            _centerPoint = EditorGUILayout.Vector2Field("Center Position", _centerPoint);
            _areaSize = EditorGUILayout.Vector2Field("Area Size", _areaSize);
            _nodeSpacing = EditorGUILayout.FloatField("Node Spacing", _nodeSpacing);
            _obstacleLayer = EditorGUILayout.LayerField("Obstacle Layer", _obstacleLayer);
            _areaColor = EditorGUILayout.ColorField("Area Color", _areaColor);

            if (GUILayout.Button("Bake and Save"))
            {
                Bake();
            }

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Handles.color = _areaColor;
            Handles.DrawWireCube(_centerPoint, _areaSize);
        }

        public void Bake()
        {
            NavGraphData data = ScriptableObject.CreateInstance<NavGraphData>();

            // CHANGE 1: Use centerPoint instead of transform.position
            Vector2 origin = _centerPoint - (_areaSize / 2);

            // CHANGE 2: Convert layer index to a Bitmask
            LayerMask mask = 1 << _obstacleLayer;

            Dictionary<Vector2, int> positionToIndex = new Dictionary<Vector2, int>();
            List<NodeBakedData> nodes = new List<NodeBakedData>();

            // Generate the nodes
            for (float x = 0; x <= _areaSize.x; x += _nodeSpacing)
            {
                for (float y = 0; y <= _areaSize.y; y += _nodeSpacing)
                {
                    Vector2 worldPos = origin + new Vector2(x, y);
                    worldPos = new Vector2(
                        Mathf.Round(worldPos.x * 1000f) / 1000f,
                        Mathf.Round(worldPos.y * 1000f) / 1000f
                    );

                    if (Physics2D.OverlapCircle(worldPos, 0.2f, mask))
                        continue;

                    // CHANGE 3: Initialize the connections list to avoid NullReferenceException
                    nodes.Add(new NodeBakedData {
                        position = worldPos,
                        connections = new List<int>()
                    });
                    positionToIndex[worldPos] = nodes.Count - 1;
                }
            }

            // Connect neighbors (Your original logic)
            Vector2[] directions = {
                new(_nodeSpacing, 0), new(-_nodeSpacing, 0), new(0, _nodeSpacing), new(0, -_nodeSpacing),
                new(_nodeSpacing, _nodeSpacing), new(-_nodeSpacing, _nodeSpacing),
                new(_nodeSpacing, -_nodeSpacing), new(-_nodeSpacing, -_nodeSpacing)
            };

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector2 pos = nodes[i].position;
                foreach (var dir in directions)
                {
                    Vector2 neighborPos = pos + dir;
                    neighborPos = new Vector2(Mathf.Round(neighborPos.x * 1000f) / 1000f, Mathf.Round(neighborPos.y * 1000f) / 1000f);

                    if (positionToIndex.TryGetValue(neighborPos, out int neighborIndex))
                    {
                        if (!Physics2D.Linecast(pos, neighborPos, mask))
                        {
                            nodes[i].connections.Add(neighborIndex);
                        }
                    }
                }
            }

            data.nodesBakedData = nodes;
            SaveAsset(data);
        }

        private void SaveAsset(NavGraphData data)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Nav Data", "NavGraphBakedData", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Nav Graph Baked", $"Created successfully!\nNodes: {data.nodesBakedData.Count}", "OK");
        }
    }
}