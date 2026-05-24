using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class BatchPrefabCreatorWindow : EditorWindow
{
    private List<Sprite> _selectedSprites = new();
    private string _targetFolderPath = "Assets";
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Batch Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<BatchPrefabCreatorWindow>("Prefab Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Prefab Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. SELECT TARGET FOLDER
        GUILayout.Label("Target Folder: " + _targetFolderPath, EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Choose Location"))
        {
            string absolutePath = EditorUtility.OpenFolderPanel("Select Output Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(absolutePath))
            {
                // Convert absolute path to a relative Unity project path
                if (absolutePath.StartsWith(Application.dataPath))
                {
                    _targetFolderPath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a folder inside this Unity project.", "OK");
                }
            }
        }

        EditorGUILayout.Space();

        // 2. DRAG AND DROP AREA FOR SPRITES
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Sprites Here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition)) break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Sprite sprite)
                        {
                            if (!_selectedSprites.Contains(sprite)) _selectedSprites.Add(sprite);
                        }
                        else if (draggedObject is Texture2D texture)
                        {
                            // If a texture is dragged, try to load its underlying sprite
                            string path = AssetDatabase.GetAssetPath(texture);
                            Sprite loadedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                            if (loadedSprite && !_selectedSprites.Contains(loadedSprite))
                            {
                                _selectedSprites.Add(loadedSprite);
                            }
                        }
                    }
                }
                Event.current.Use();
                break;
        }

        // 3. DISPLAY ADDED SPRITES
        GUILayout.Label($"Selected Sprites ({_selectedSprites.Count}):", EditorStyles.boldLabel);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
        for (int i = _selectedSprites.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_selectedSprites[i] != null ? _selectedSprites[i].name : "Null");
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                _selectedSprites.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // 4. CLEAR & GENERATE BUTTONS
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear List"))
        {
            _selectedSprites.Clear();
        }

        GUI.enabled = _selectedSprites.Count > 0;
        if (GUILayout.Button("Generate Prefabs", GUILayout.Height(30)))
        {
            CreatePrefabsAndFolders();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void CreatePrefabsAndFolders()
    {
        // Define directory paths
        string parallaxFolder = Path.Combine(_targetFolderPath, "Paralax");
        string collisionFolder = Path.Combine(_targetFolderPath, "Collision");

        // Create folders if they do not exist
        if (!AssetDatabase.IsValidFolder(parallaxFolder)) Directory.CreateDirectory(parallaxFolder);
        if (!AssetDatabase.IsValidFolder(collisionFolder)) Directory.CreateDirectory(collisionFolder);

        AssetDatabase.Refresh();

        foreach (Sprite sprite in _selectedSprites)
        {
            if (!sprite) continue;

            // 1. CREATE PARALLAX PREFAB
            GameObject parallaxGo = new GameObject("Par_" + sprite.name);
            parallaxGo.AddComponent<SpriteRenderer>().sprite = sprite;

            System.Type parallaxType = System.Type.GetType("Parallax, PathOfTheInfected - Gameplay");
            if (parallaxType != null)
            {
                parallaxGo.AddComponent(parallaxType);
            }

            string path1 = Path.Combine(parallaxFolder, parallaxGo.name + ".prefab").Replace('\\', '/');
            PrefabUtility.SaveAsPrefabAsset(parallaxGo, path1);
            DestroyImmediate(parallaxGo);

            // 2. CREATE BOX COLLIDER PREFAB
            GameObject boxGo = new GameObject("Coll_" + sprite.name);
            boxGo.AddComponent<SpriteRenderer>().sprite = sprite;
            boxGo.AddComponent<BoxCollider2D>();
            boxGo.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            string path2 = Path.Combine(collisionFolder, boxGo.name + ".prefab").Replace('\\', '/');
            PrefabUtility.SaveAsPrefabAsset(boxGo, path2);
            DestroyImmediate(boxGo);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Created prefabs for {_selectedSprites.Count} sprites inside '{_targetFolderPath}'.", "OK");
        _selectedSprites.Clear();
    }
}
