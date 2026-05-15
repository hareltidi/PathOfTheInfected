using System.Collections.Generic;
using TidiPathFinding;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Represents the aggressive state for a grounded enemy, managing target acquisition and movement toward detected
    /// targets.
    /// </summary>
    /// <remarks>This state is typically used when an enemy has detected a target and is actively pursuing it.
    /// The state handles updating the target and transitioning to other states based on detection and attack range
    /// conditions. Use this state within an enemy state machine to enable aggressive pursuit behavior.</remarks>
    /// <remarks>Pseudo state: Spottable detected</remarks>
    [CreateAssetMenu(menuName = "Enemy/States/Grounded/EnemyAggroSO", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : EnemyBaseState
    {
        public Transform Target { get; private set; }

        [SerializeField] private float aggroDelay = 0.5f;
        private float _aggroTimer;

        [Header("Personality")]
        [SerializeField] private bool useOffsetPursuit = true;
        [SerializeField] private float offsetRadiusMin = 2f;
        [SerializeField] private float offsetRadiusMax = 4f;
        [SerializeField] private float offsetRefreshInterval = 0.6f;

        private float _offsetTimer;
        private Vector2 _currentOffsetTarget;

        public override void StateEnter()
        {
            base.StateEnter();
            _aggroTimer = 0f;
            _offsetTimer = offsetRefreshInterval; // force an immediate offset pick
            Target = null;
            _currentOffsetTarget = (Vector2)CurrentEnemyBrain.transform.position; // safe fallback
        }

        public override void StateFixedUpdate()
        {
            if (_aggroTimer < aggroDelay)
            {
                _aggroTimer += Time.fixedDeltaTime;
                return;
            }

            if (!Target)
            {
                Target = CurrentEnemyBrain.ClosestTarget;
            }

            if (!Target)
            {
                if (CurrentEnemyBrain.HasLastKnownTarget)
                    CurrentEnemyBrain.MoveTo(CurrentEnemyBrain.LastKnownTargetPosition);
                return;
            }

            if (!useOffsetPursuit)
            {
                CurrentEnemyBrain.MoveTo(Target);
                return;
            }

            _offsetTimer += Time.fixedDeltaTime;
            if (_offsetTimer >= offsetRefreshInterval)
            {
                _offsetTimer = 0f;

                // Robust offset sampling: pick several candidate offsets biased toward the
                // direction from the enemy to the target and validate each candidate with
                // the pathfinder. Choose the candidate with the shortest valid path.
                int candidates = 5;
                float maxAngleDeg = 90f; // sample within +/- 90 degrees around forward

                Vector2 enemyPos = CurrentEnemyBrain.transform.position;
                Vector2 toTarget = ((Vector2)Target.position - enemyPos).normalized;

                float bestPathLength = float.MaxValue;
                Vector2 bestCandidate = (Vector2)Target.position;
                bool foundValid = false;

                for (int i = 0; i < candidates; i++)
                {
                    float r = Random.Range(offsetRadiusMin, offsetRadiusMax);
                    float angle = Random.Range(-maxAngleDeg, maxAngleDeg);
                    Vector2 dir = Quaternion.Euler(0f, 0f, angle) * toTarget;
                    if (dir.sqrMagnitude < 0.0001f) dir = toTarget;
                    dir.Normalize();

                    Vector2 candidate = (Vector2)Target.position + dir * r;

                    // Validate with pathfinder
                    List<Vector2> path = AStarPathFinder.FindPath_CurrentGraph(CurrentEnemyBrain.transform.position, candidate);
                    if (path != null && path.Count > 0)
                    {
                        // compute total length from start of path
                        float len = 0f;
                        Vector2 prev = CurrentEnemyBrain.transform.position;
                        foreach (var p in path)
                        {
                            len += Vector2.Distance(prev, p);
                            prev = p;
                        }

                        if (len < bestPathLength)
                        {
                            bestPathLength = len;
                            bestCandidate = candidate;
                            foundValid = true;
                        }
                    }
                }

                if (foundValid)
                {
                    _currentOffsetTarget = bestCandidate;
                }
                else
                {
                    // fallback to direct target position if no valid offset found
                    _currentOffsetTarget = Target.position;
                }
            }

            CurrentEnemyBrain.MoveTo(_currentOffsetTarget);
        }

        public override void DrawGizmosOnSelected(EnemyBrainBase en)
        {
            base.DrawGizmosOnSelected(en);
            if (useOffsetPursuit)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_currentOffsetTarget, 0.12f);
                Gizmos.DrawLine(en.transform.position, _currentOffsetTarget);
            }

            if (Target)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(Target.position, 0.12f);
            }
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();

            if (CurrentEnemyBrain.isSpottableInAttackRange)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.spottableInAttackRangeState);
            }

            if (!CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.noSpottableDetectedState);
            }

            if (CurrentEnemyBrain.isEnemyDamaged)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.damagedState);
            }
        }
    }
}