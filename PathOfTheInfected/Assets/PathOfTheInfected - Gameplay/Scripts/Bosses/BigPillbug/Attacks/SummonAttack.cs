using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.BigPillbug
{
    [CreateAssetMenu(fileName = "SummonAttack", menuName = "Boss/Attacks/BigPillbug/SummonAttack")]
    public class SummonAttack : AttackSOBase
    {
        [SerializeField] private GameObject pillbugPrefab;
        [SerializeField] private int summonCount = 3;
        [SerializeField] private float spawnRadius = 2f;

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
                    if (!ctx.HasHit)
                    {
                        PerformAttack(ctx);
                    }
                    else
                    {
                        ctx.Phase = AttackPhase.Recovery;
                        ctx.Timer = 0f;
                    }
                    break;
                case AttackPhase.Recovery:
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

        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            if (!pillbugPrefab) return;

            for (int i = 0; i < summonCount; i++)
            {
                Vector2 dir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left;
                Vector2 spawnDir = dir * spawnRadius;
                Vector2 spawnPos = (Vector2)ctx.Owner.Transform.position + spawnDir;
                // keep spawn above ground if possible
                spawnPos.y = Mathf.Max(spawnPos.y, ctx.Owner.Transform.position.y);

                Instantiate(pillbugPrefab, spawnPos, Quaternion.identity);
            }
            ctx.HasHit = true;
        }
    }
}
