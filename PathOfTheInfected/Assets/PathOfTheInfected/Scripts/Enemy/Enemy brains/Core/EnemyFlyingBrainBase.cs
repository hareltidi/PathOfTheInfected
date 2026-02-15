using System.Collections.Generic;
using PathOfTheInfected.Damagable;
using TidiPathFinding;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class EnemyFlyingBrainBase : EnemyBrainBase
    {
        #region Detection Methods

        protected override void DetectVisibleSpottables()
        {
            VisibleSpottables.Clear();

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                maxSpotRange,
                SpottableMask
            );

            ClosestTarget = null;
            BestDistSq = float.MaxValue;
            foreach (Collider2D hit in hits)
            {
                if (requiresLos)
                {
                    bool hasLineOfSight = HasLineOfSight(hit.transform);
                    UpdateLineOfSightFlag(hasLineOfSight);
                    if (hit.TryGetComponent(out ISpottable spottable) && hasLineOfSight)
                    {
                        VisibleSpottables.Add(spottable);
                    }
                }
                else
                {
                    if (hit.TryGetComponent(out ISpottable spottable))
                    {
                        VisibleSpottables.Add(spottable);
                    }
                }
                FindClosestTarget();
            }

            isSpottableDetected = VisibleSpottables.Count > 0;
        }

        protected override void CheckForSpottablesInAttackRange()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                attack.MaxAttackRange,
                SpottableMask
            );
            bool test = false;
            ISpottable testTarget = null;
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<ISpottable>(out var spottable) &&
                    hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    if (VisibleSpottables.Contains(spottable) && damageable != null)
                    {
                        testTarget = spottable;
                        test = true;
                        break;
                    }
                }
            }

            isSpottableInAttackRange = test;
            AttackTarget = testTarget;
        }

        protected override void DrawingSpottingRange()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxSpotRange);
        }

        protected override void DrawAttackRange()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attack.MaxAttackRange);
        }

        #endregion

        #region Movement overrides

        public override void MoveEnemy(Vector2 dir, bool instant = false)
        {
            if (!RB) return;

            Vector2 targetVelocity = dir * moveSpeed;

            float t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);

            Vector2 newVelocity;
            if (!instant)
            {
                 newVelocity = Vector2.Lerp(
                    RB.linearVelocity,
                    targetVelocity,
                    t
                );
            }
            else
            {
                 newVelocity = targetVelocity;
            }
            CheckForLeftOrRightFacing(newVelocity);
            RB.linearVelocity = newVelocity;
        }

        public override void MoveTo(Vector2 target)
        {
            if (AStarPathFinder.CurrentGraph == null || !RB) return;

            // --- Recalculate path on timer ---
            if (Time.timeSinceLevelLoad >= nextRepath)
            {
                List<Vector2> newPath = AStarPathFinder.FindPath_CurrentGraph(
                    transform.position, target);

                nextRepath = Time.timeSinceLevelLoad + repathInterval; // Reset timer

                if (newPath != null && newPath.Count > 0) // If a path was found
                {
                    currentPath = newPath;

                    float bestDistance = float.MaxValue;
                    int bestIndex = 0;

                    for (int i = 0; i < currentPath.Count; i++)
                    {
                        float dist = Vector2.Distance(transform.position, currentPath[i]);
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            bestIndex = i;
                        }
                    }

                    currentIndex = bestIndex;
                }
            }

            if (currentPath == null || currentIndex >= currentPath.Count) // If no path found or reached te end
            {
                MoveEnemy(Vector2.zero); // Stop moving
                return; // Exit method early
            }

            // --- Look-ahead ---
            int targetIndex = Mathf.Min(currentIndex + 1, currentPath.Count - 1); // Next waypoint
            Vector2 waypoint = currentPath[targetIndex];

            Vector2 toWaypoint = waypoint - (Vector2)transform.position; // Distance to waypoint

            // Ground enemy moves only horizontally
            Vector2 dir = new Vector2(Mathf.Sign(toWaypoint.x), Mathf.Sign(toWaypoint.y)); // Direction to waypoint

            MoveEnemy(dir); // Move the enemy in that direction

            // Advance waypoint if close enough
            if (Mathf.Abs(toWaypoint.x) <= waypointTolerance || Mathf.Abs(toWaypoint.y) <= waypointTolerance)
            {
                currentIndex++;
            }
        }

        public override void MoveTo(GameObject target)
        {
            if (!target || !target.transform) return;
            MoveTo(target.transform.position);
        }

        public override void MoveTo(Transform target)
        {
            if (!target) return;
            MoveTo(target.position);
        }

        #endregion
    }
}