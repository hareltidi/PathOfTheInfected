using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyIdleSO", menuName = "Enemy/States/General/EnemyIdleSO", order = 0)]
    public class EnemyIdleSO : EnemyBaseState
    {
        public override void StateEnter()
        {
            base.StateEnter();
            CurrentEnemyBrain.MoveEnemy(Vector2.zero);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.spottableDetectedState);
            }

            if (CurrentEnemyBrain.isSpottableInAttackRange)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.spottableInAttackRangeState);
            }

            if (CurrentEnemyBrain.isEnemyDamaged)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.damagedState);
            }
        }
    }
}