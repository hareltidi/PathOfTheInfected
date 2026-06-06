using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyDamagedSO", menuName = "Enemy/States/General/EnemyDamagedSO", order = 0)]
    public class EnemyDamagedSO : EnemyBaseState
    {
        public override void StateEnter()
        {
            base.StateEnter();
            CurrentEnemyBrain?.MoveEnemy(Vector2.zero);
        }
        public override void TransitionChecks()
        {
            if (!CurrentEnemyBrain || StateMachine == null) return;
            if (!CurrentEnemyBrain.isEnemyDamaged)
            {
                if (CurrentEnemyBrain.isSpottableDetected)
                {
                    StateMachine.RequestStateChange(CurrentEnemyBrain.spottableDetectedState);
                }

                if (CurrentEnemyBrain.isSpottableInAttackRange)
                {
                    StateMachine.RequestStateChange(CurrentEnemyBrain.spottableInAttackRangeState);
                }

                if (!CurrentEnemyBrain.isSpottableDetected && !CurrentEnemyBrain.isSpottableInAttackRange)
                {
                    StateMachine.RequestStateChange(CurrentEnemyBrain.noSpottableDetectedState);
                }
            }
        }
    }
}