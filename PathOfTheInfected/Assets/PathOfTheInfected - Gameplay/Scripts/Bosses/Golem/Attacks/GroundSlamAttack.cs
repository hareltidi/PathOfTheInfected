using PathOfTheInfected.Combat;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.Golem
{
    [CreateAssetMenu(fileName = "GroundSlamAttack", menuName = "Boss/Attacks/Golem/GroundSlamAttack")]
    public class GroundSlamAttack : AttackSOBase
    {
        [SerializeField] private Vector2 hitboxSize = new(4f, 2f);
        [SerializeField] private float damageOffsetX = 2f;
        [SerializeField] private LayerMask spottableMask;
        [SerializeField] private float knockbackStrength = 10f;
        [SerializeField] private AnimationClip slamAnim;

        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            Vector2 dir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left;
            Vector2 spawnPos = new Vector2(
                ctx.Owner.Transform.position.x + (dir.x * damageOffsetX),
                ctx.Owner.Transform.position.y
            );

            Collider2D hit = Physics2D.OverlapBox(spawnPos, hitboxSize, 0f, spottableMask);
            IHitResponder hitResponder = hit?.GetComponent<IHitResponder>();
            if (hitResponder  != null)
            {
                Debug.Log("Ground Slam!");
                HitData data = new HitData()
                {
                    attackDefinition = AttackDef,
                    isFirstHit = false,
                    isPlayerDamage = false,
                    isAttackerInAir = false,
                    source = ctx.Owner.GameObject,
                    timeStamp = Time.timeSinceLevelLoad,
                    target = hit.gameObject,
                    firstHitDamageBoost = 0,
                    comboDamageScalingLevel = 1,
                    attackDir = dir,
                    knockbackStrength = knockbackStrength,
                };
                var result = HitDispatcher.ProcessHit(ref data);
                ReactToHitResult(result);
            }
        }
    }
}
