using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyAttackSOBase", menuName = "Enemy/States/EnemyAttackSOBase", order = 0)]
    public class EnemyAttackSOBase : EnemyBaseState
    {
        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            AttackContext context = new AttackContext(_enemy, _enemy.AttackTarget.Transform, 0f, AttackPhase.WindUp,
                false, false);
            _enemy.attack.AttackLogic(context);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
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