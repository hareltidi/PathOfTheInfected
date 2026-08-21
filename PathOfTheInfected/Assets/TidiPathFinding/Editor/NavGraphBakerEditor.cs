using UnityEditor;
using UnityEngine;
using TidiPathFinding;

namespace TidiPathFinding.Editor
{
    [CustomEditor(typeof(NavGraphBaker))]
    public class NavGraphBakerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            NavGraphBaker baker = (NavGraphBaker)target;

            if (GUILayout.Button("Bake Nav Graph"))
            {
                baker?.Bake();
            }
        }
    }
}