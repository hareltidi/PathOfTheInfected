using PathOfTheInfected.Combat;
using PathOfTheInfected.Damagable;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "MeleePunchSO", menuName = "Enemy/Attack/Melee/MeleePunchSO", order = 0)]
    public class MeleePunchSO : AttackSOBase
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


            Collider2D hit = Physics2D.OverlapBox(
                center,
                size,
                0f,
                enemyBrainBase.SpottableMask
            );

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