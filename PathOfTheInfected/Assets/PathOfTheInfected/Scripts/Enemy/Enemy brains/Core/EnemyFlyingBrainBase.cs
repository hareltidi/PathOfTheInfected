using PathOfTheInfected.Damagable;
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
                if (requiresLOS)
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

        public override void MoveEnemy(Vector2 dir)
        {
            if (!RB) return;

            Vector2 targetVelocity = dir * moveSpeed;

            float t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);

            Vector2 newVelocity = Vector2.Lerp(
                RB.linearVelocity,
                targetVelocity,
                t
            );
            CheckForLeftOrRightFacing(newVelocity);
            RB.linearVelocity = newVelocity;
        }

        public override void MoveTo(Vector2 target)
        {
            Vector2 dir = target - (Vector2)transform.position;
            dir.Normalize();
            MoveEnemy(dir);
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