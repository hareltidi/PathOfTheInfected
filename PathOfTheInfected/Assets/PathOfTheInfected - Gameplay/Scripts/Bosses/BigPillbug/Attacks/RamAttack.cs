using PathOfTheInfected.Combat;
using PathOfTheInfected.Core.Scripts.Boss;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.BigPillbug
{
    [CreateAssetMenu(fileName = "RamAttack", menuName = "Boss/Attacks/BigPillbug/RamAttack")]
    public class RamAttack : AttackSOBase
    {
        [SerializeField] private float chargeDuration = 1f;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private LayerMask spottableMask;
        [SerializeField] private float offsetX = 10;


        private BossBrain _cachedBossBrain;
        public override void InitAttack(AttackContext ctx, IAttackOwnerable owner, Transform target)
        {
            base.InitAttack(ctx, owner, target);
            _cachedBossBrain = (BossBrain)owner;
        }

        public override void AttackLogic(AttackContext ctx)
        {
            ctx.Timer += Time.fixedDeltaTime;
            switch (ctx.Phase)
            {
                case AttackPhase.WindUp:
                    if (ctx.Target)
                    {
                        float diffX = ctx.Target.position.x - ctx.Owner.Transform.position.x;
                        bool shouldFaceRight = diffX >= 0;
                        if (shouldFaceRight != ctx.Owner.IsFacingRight)
                        {
                            ctx.Owner.IsFacingRight = shouldFaceRight;
                            var euler = ctx.Owner.Transform.eulerAngles;
                            ctx.Owner.Transform.eulerAngles = new Vector3(euler.x, shouldFaceRight ? 0f : 180f, euler.z);
                        }
                    }

                    if (ctx.Timer >= windupDuration)
                    {
                        ctx.Timer = 0f;
                        ctx.Phase = AttackPhase.Active;
                    }
                    break;
                case AttackPhase.Active:
                    // Perform the charge movement
                    if (_cachedBossBrain)
                    {
                        _cachedBossBrain.MoveTo(ctx.Target.position);
                    }
                    // Check collisions in front to damage player
                    Vector2 offsetH = new Vector2(ctx.Owner.Transform.position.x + offsetX,  ctx.Owner.Transform.position.y);
                    Vector2 finalPos = (Vector2) ctx.Owner.Transform.position + offsetH;
                    Collider2D hit = Physics2D.OverlapBox(finalPos, new Vector2(2f, 2f), 0f, spottableMask);
                    IHitResponder hitResponder = hit?.GetComponent<IHitResponder>();
                    if (hitResponder != null && !ctx.HasHit)
                    {
                        HitData data = new HitData()
                        {
                            attackDefinition = AttackDef,
                            source = ctx.Owner.GameObject,
                            target = hit.gameObject,
                            isPlayerDamage = false,
                            isAttackerInAir = !ctx.Owner.IsGrounded,
                            timeStamp = Time.timeSinceLevelLoad,
                            attackDir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left,
                            knockbackStrength = knockbackForce
                        };
                        Debug.Log(data.attackDir);
                        Debug.Log(data.knockbackStrength);
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
                        Debug.Log("In Attack Recovery");
                    }
                    if (ctx.Timer >= AttackDef.recoveryTime)
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
