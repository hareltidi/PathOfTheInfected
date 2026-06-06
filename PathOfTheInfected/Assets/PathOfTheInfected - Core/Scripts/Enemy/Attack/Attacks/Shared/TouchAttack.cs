using PathOfTheInfected.Combat;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Core.Scripts.Enemy.Attack.Attacks.Shared
{
    [CreateAssetMenu(fileName = "TouchAttack", menuName = "Enemy/CurrentAttack/Shared/TouchAttack", order = 0)]
    public class TouchAttack : AttackSOBase
    {
        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);
            HitData data = new HitData()
            {
                attackDefinition = AttackDef,
                isFirstHit = false,
                isPlayerDamage = false,
                isAttackerInAir = false,
                source = ctx.Owner.GameObject,
                timeStamp = Time.timeSinceLevelLoad,
                target = ctx.Target.gameObject,
                firstHitDamageBoost = 0,
                comboDamageScalingLevel = 1,
            };

            var result = HitDispatcher.ProcessHit(ref data);

            ReactToHitResult(in result);
        }


    }
}