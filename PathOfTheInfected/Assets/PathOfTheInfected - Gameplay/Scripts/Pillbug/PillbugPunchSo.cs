using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "PillbugPunchSO", menuName = "Enemy/CurrentAttack/Melee/Pillbug/PillbugPunchSO", order = 0)]
    public class PillbugPunchSo : MeleePunchSO
    {
        private PillbugAnimInstance _animInstance;

        public override void InitAttack(AttackContext ctx, IAttackOwnerable owner, Transform target)
        {
            base.InitAttack(ctx, owner, target);
            _animInstance = ctx.Owner.GameObject.GetComponent<PillbugAnimInstance>();
        }


        public override void AttackLogic(AttackContext ctx)
        {
            base.AttackLogic(ctx);
            switch (ctx.Phase)
            {
               case AttackPhase.WindUp:
                   _animInstance?.PlayAnimationIfNotCurrent(_animInstance.AttackAnimHash, 0f, 0,
                       true, true);
                   break;
            }
        }
    }
}