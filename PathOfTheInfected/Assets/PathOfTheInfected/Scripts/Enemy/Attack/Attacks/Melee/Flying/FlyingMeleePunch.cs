using PathOfTheInfected.Combat;
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


            Collider2D hit = Physics2D.OverlapCircle(enemyBrainBase.transform.position, range,enemyBrainBase.SpottableMask);

            // build the hit data:
            HitData data = new HitData()
            {
                attackDefinition = attackDef,
                isFirstHit = false,
                isPlayerDamage = false,
                isAttackerInAir = false,
                source = ctx.Owner.gameObject,
                timeStamp = Time.timeSinceLevelLoad,
                target = hit.gameObject,
                firstHitDamageBoost = 0,
                comboDamageScalingLevel = 1,
            };

            // process the hit
           var result = HitDispatcher.ProcessHit(data);

           ReactToHitResult(result);
        }
    }

}
