using System.Collections.Generic;
using TidiPathFinding;
using UnityEngine;

public class PathFindingTesting : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waypointTolerance = 0.1f;
    [SerializeField] private float repathInterval = 1f;

    private List<Vector2> _currentPath;
    private int _currentIndex;
    private float _nextRepath;
    private Vector2 _lastTargetPosition;
    private void Update()
    {
        FollowCurrentPath();
    }


    /// <summary>
    /// Updates the object's position along a calculated path toward the target.
    /// The method recalculates the path at regular intervals using the A* pathfinding algorithm
    /// and progresses through the current path's waypoints until the destination is reached.
    /// </summary>
    /// <remarks>
    /// This method requires a valid target Transform and a configured A* graph to operate.
    /// If a valid path is available, the object moves toward the next waypoint using a
    /// specified movement speed. The path is recomputed based on a repath interval timer
    /// to account for dynamic changes in the environment or the target's position.
    /// </remarks>
    private void FollowCurrentPath()
    {
        if (!target|| AStarPathFinder.CurrentGraph == null) return;


        // --- Recalculate path on timer ---
        if (Time.timeSinceLevelLoad >= _nextRepath)
        {
            List<Vector2> newPath = AStarPathFinder.FindPath_CurrentGraph(transform.position, target.position);
            _nextRepath = Time.timeSinceLevelLoad + repathInterval;

            if (newPath != null && newPath.Count > 0)
            {
                _currentPath = newPath;

                // Find the closest waypoint in the new path
                float bestDistance = float.MaxValue;
                int bestIndex = 0;

                for (int i = 0; i < _currentPath.Count; i++)
                {
                    float dist = Vector2.Distance(transform.position, _currentPath[i]);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestIndex = i;
                    }
                }

                _currentIndex = bestIndex;
            }
        }

        // --- If no valid path, stop ---
        if (_currentPath == null || _currentIndex >= _currentPath.Count) return;


        // ---Optional look-ahead---
        int targetIndex = Mathf.Min(_currentIndex + 1, _currentPath.Count - 1);
        Vector2 waypoint = _currentPath[targetIndex];

        Vector2 currentPos = transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, waypoint, moveSpeed * Time.deltaTime);

        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

        // Advance index if close enough
        if (Vector2.Distance(newPos, waypoint) <= waypointTolerance)
        {
            _currentIndex = Mathf.Min(_currentIndex + 1, _currentPath.Count - 1);
        }
    }



    private void OnDrawGizmos()
    {
        if (_currentPath == null || _currentPath.Count == 0) return;
        Gizmos.color = Color.yellow;
        Vector3 prev = transform.position;
        for (int i = _currentIndex; i < _currentPath.Count; i++)
        {
            Vector3 p = _currentPath[i];
            Gizmos.DrawLine(prev, p);
            Gizmos.DrawSphere(p, 0.07f);
            prev = p;
        }
    }
}
