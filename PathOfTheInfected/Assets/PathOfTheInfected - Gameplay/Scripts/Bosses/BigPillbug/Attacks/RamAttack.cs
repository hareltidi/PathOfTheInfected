using PathOfTheInfected.Combat;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.BigPillbug
{
    [CreateAssetMenu(fileName = "RamAttack", menuName = "Boss/Attacks/BigPillbug/RamAttack")]
    public class RamAttack : AttackSOBase
    {
        [SerializeField] private float chargeSpeed = 10f;
        [SerializeField] private float chargeDuration = 1f;

        public override void AttackLogic(AttackContext ctx)
        {
            ctx.Timer += Time.fixedDeltaTime;
            switch (ctx.Phase)
            {
                case AttackPhase.WindUp:
                    if (ctx.Timer >= windupDuration)
                    {
                        ctx.Timer = 0f;
                        ctx.Phase = AttackPhase.Active;
                    }
                    break;
                case AttackPhase.Active:
                    // Perform the charge movement
                    if (ctx.Owner.GameObject.TryGetComponent(out Rigidbody2D rb))
                    {
                        Vector2 dir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left;
                        rb.linearVelocity = new Vector2(dir.x * chargeSpeed, rb.linearVelocity.y);
                    }

                    // Check collisions in front to damage player
                    Collider2D hit = Physics2D.OverlapBox(ctx.Owner.Transform.position, new Vector2(2f, 2f), 0f);
                    if (hit && hit.transform == ctx.Target && !ctx.HasHit)
                    {
                        HitData data = new HitData()
                        {
                            attackDefinition = attackDef,
                            source = ctx.Owner.GameObject,
                            target = hit.gameObject,
                        };
                        HitDispatcher.ProcessHit(ref data);
                        ctx.HasHit = true;
                    }

                    if (ctx.Timer >= chargeDuration)
                    {
                        ctx.Phase = AttackPhase.Recovery;
                        ctx.Timer = 0f;
                    }
                    break;
                case AttackPhase.Recovery:
                    if (ctx.Owner.GameObject.TryGetComponent(out Rigidbody2D r))
                    {
                        r.linearVelocity = new Vector2(0f, r.linearVelocity.y);
                    }
                    if (ctx.Timer >= attackDef.recoveryTime)
                    {
                        ctx.IsFinished = true;
                    }
                    break;
                case AttackPhase.PoiseRecovery:
                    RecoverPoise(ctx);
                    break;
            }
        }
    }
}
