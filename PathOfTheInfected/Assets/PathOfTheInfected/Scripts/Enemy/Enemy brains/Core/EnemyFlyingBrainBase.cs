using System.Collections.Generic;
using PathOfTheInfected.Damagable;
using TidiPathFinding;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class EnemyFlyingBrainBase : EnemyBrainBase
    {
        #region Member Variables
       [SerializeField] protected float obstacleCheckDistance = 5f;
       #endregion


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
                        if (attack && attack.RequireDistanceFromEnemyToSpottable)
                        {
                            test = (Mathf.Abs(Vector2.Distance(transform.position, testTarget.Transform.position))
                                    < attack.DistanceThreshold);
                        }
                        else if (attack)
                        {
                            test = true;
                        }
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

            Vector2 targetVelocity = dir * movementPersonality.maxSpeed;

            float t = Mathf.Clamp01(movementPersonality.acceleration * Time.fixedDeltaTime);
            float easedT = TidiEasing.Ease(movementPersonality.movementEase, t);

            Vector2 newVelocity;
            if (!instant)
            {
                 newVelocity = Vector2.Lerp(
                    RB.linearVelocity,
                    targetVelocity,
                    easedT
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

            Vector2 toWaypoint = waypoint - (Vector2)transform.position;
            Vector2 dir = toWaypoint.normalized;

            Vector2 baseCenter = (min.position + max.position) * 0.5f;
            Vector2 baseSize = boxCollider.size;

            float range = attack.MaxAttackRange;
            int facingDirection = IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);

            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0,
                LayerMask.GetMask("ground")); // Check for obstacles

            bool pathWantsUp = hits.Length > 0 || waypoint.y > transform.position.y + 0.1f; // Check if we need to go up or we hit something that we need to climb

            if (pathWantsUp) // If there are obstacles and we want to go up
            {
                MoveEnemy(Vector2.up); // Move the enemy upward
            }
            else
            {
                MoveEnemy(dir); // Move the enemy in the normal direction we calculated
            }


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

        private void OnDrawGizmos()
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                Gizmos.color = Color.yellow;
                Vector3 prev = transform.position;
                for (int i = currentIndex; i < currentPath.Count; i++)
                {
                    Vector3 p = currentPath[i];
                    Gizmos.DrawLine(prev, p);
                    Gizmos.DrawSphere(p, 0.07f);
                    prev = p;
                }
            }

            if (boxCollider)
            {
                Gizmos.color = Color.magenta;
                Vector2 baseCenter = (min.position + max.position) * 0.5f;
                Vector2 baseSize = new Vector2(
                    Mathf.Abs(max.position.x - min.position.x),
                    Mathf.Abs(max.position.y - min.position.y)
                );

                float range = attack.MaxAttackRange;
                int facingDirection = IsFacingRight ? 1 : -1;
                float forwardOffset = range * 0.5f * facingDirection;

                Vector2 center = baseCenter + Vector2.right * forwardOffset;
                Vector2 size = new Vector2(baseSize.x + range, baseSize.y);
                Gizmos.DrawWireCube(center, size);
            }

        }
    }
}