using UnityEditorInternal;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyAttackSOBase", menuName = "Enemy/States/EnemyAttackSOBase", order = 0)]
    public class EnemyAttackSOBase : EnemyBaseState
    {
        AttackContext context = new();

        public override void StateEnter()
        {
            _enemy.attack.InitAttack(context, _enemy, _enemy.AttackTarget.Transform);
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            if (!context.IsFinished)
            {
                _enemy.attack.AttackLogic(context);
            }
            else
            {
                _enemy.attack.InitAttack(context, _enemy, _enemy.AttackTarget.Transform);
            }

        }

        public override void TransitionChecks()
        {
            if (!_enemy.isSpottableInAttackRange && _enemy.isSpottableDetected)
            {
                _stateMachine.RequestStateChange(_enemy.spottableDetectedState);
            }

            if (!_enemy.isSpottableInAttackRange && !_enemy.isSpottableDetected)
            {
                _stateMachine.RequestStateChange(_enemy.noSpottableDetectedState);
            }
        }
    }
}