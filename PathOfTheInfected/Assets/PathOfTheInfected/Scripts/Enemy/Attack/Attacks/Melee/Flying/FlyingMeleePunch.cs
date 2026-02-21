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
            float range = MaxAttackRange;


            Collider2D[] hits = Physics2D.OverlapCircleAll(enemyBrainBase.transform.position, range,enemyBrainBase.SpottableMask);

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
