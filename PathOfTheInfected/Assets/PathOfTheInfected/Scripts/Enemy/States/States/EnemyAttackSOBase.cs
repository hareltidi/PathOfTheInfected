using UnityEditorInternal;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyAttackSOBase", menuName = "Enemy/States/Grounded/EnemyAttackSOBase", order = 0)]
    public class EnemyAttackSOBase : EnemyBaseState
    {
        AttackContext context = new();

        public override void StateEnter()
        {
            if (EnemyBrainBase && EnemyBrainBase.AttackTarget != null && EnemyBrainBase.AttackTarget.Transform)
            {
                EnemyBrainBase.attack.InitAttack(context, EnemyBrainBase, EnemyBrainBase.AttackTarget.Transform);
            }
        }

        public override void StateFixedUpdate()
        {
            if (!EnemyBrainBase || EnemyBrainBase.AttackTarget == null || !EnemyBrainBase.AttackTarget.Transform) return;
            base.StateFixedUpdate();
            if (!context.IsFinished)
            {
                EnemyBrainBase.attack.AttackLogic(context);
            }
            else
            {
                EnemyBrainBase.attack.InitAttack(context, EnemyBrainBase, EnemyBrainBase.AttackTarget.Transform);
            }

        }

        public override void TransitionChecks()
        {
            if (!EnemyBrainBase.isSpottableInAttackRange && EnemyBrainBase.isSpottableDetected)
            {
                StateMachine.RequestStateChange(EnemyBrainBase.spottableDetectedState);
            }

            if (!EnemyBrainBase.isSpottableInAttackRange && !EnemyBrainBase.isSpottableDetected)
            {
                StateMachine.RequestStateChange(EnemyBrainBase.noSpottableDetectedState);
            }
        }
    }
}