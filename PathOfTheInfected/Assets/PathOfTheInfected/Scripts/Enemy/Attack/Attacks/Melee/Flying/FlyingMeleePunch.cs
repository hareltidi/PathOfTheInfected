using PathOfTheInfected.Damagable;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "FlyingMeleePunchSO", menuName = "Enemy/Attack/Melee/Flying/FlyingMeleePunchSO", order = 0)]
    public class FlyingMeleePunch : AttackSOBase
    {
        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);
            EnemyBrainBase enemyBrainBase = ctx.Owner;
            // Box cast and if we hit an IDamageable, damage his ass up
            Vector2 baseCenter = (enemyBrainBase.min.position + enemyBrainBase.max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(enemyBrainBase.max.position.x - enemyBrainBase.min.position.x),
                Mathf.Abs(enemyBrainBase.max.position.y - enemyBrainBase.min.position.y)
            );

            float range = MaxAttackRange;
            int facingDirection = enemyBrainBase.IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);


            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyBrainBase.transform.position, MaxAttackRange,enemyBrainBase.SpottableMask);

            foreach (var t in hits)
            {
                if (t.TryGetComponent<IDamageable>(out var damageable))
                {
                    if (damageable != null && !damageable.IsDead)
                    {
                        DamageData data = new DamageData
                        {
                            Damage = damage,
                            HitStopTime = hitStopTime,
                            DamagedObject = damageable,
                            Instigator = ctx.Owner
                        };
                        damageable.TakeDamage(data);
                    }

                }
            }
        }
    }

}
