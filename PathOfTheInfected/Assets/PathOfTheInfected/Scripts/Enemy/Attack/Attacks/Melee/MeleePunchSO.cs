using PathOfTheInfected.Damagable;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "MeleePunchSO", menuName = "Enemy/Attack/MeleePunchSO", order = 0)]
    public class MeleePunchSO : AttackSOBase
    {
        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);
            Enemy enemy = ctx.Owner;
            // Box cast and if we hit an IDamageable, damage his ass up
            Vector2 baseCenter = (enemy.min.position + enemy.max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(enemy.max.position.x - enemy.min.position.x),
                Mathf.Abs(enemy.max.position.y - enemy.min.position.y)
            );

            float range = enemy.maxSpotRange;
            int facingDirection = enemy.IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);


            Collider2D[] hits = Physics2D.OverlapBoxAll(
                center,
                size,
                0f,
                enemy.SpottableMask
            );

            foreach (var t in hits)
            {
                if (t.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable?.TakeDamage(damage);
                }
            }
        }
    }
}