using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/States/Grounded/EnemyAggroSO", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : EnemyBaseState
    {
        public Transform target;

        public override void StateEnter()
        {
            base.StateEnter();
        }

        public override void StateFixedUpdate()
        {
            target = CurrentEnemyBrain.ClosestTarget;
            CurrentEnemyBrain.MoveTo(target);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (CurrentEnemyBrain.isSpottableInAttackRange)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.spottableInAttackRangeState);
            }

            if (!CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.noSpottableDetectedState);
            }
        }
    }
}