using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/States/Grounded/EnemyAggroSO", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : EnemyBaseState
    {
        public Transform target;
        private Vector2 _enemyPos = Vector2.zero;

        public override void StateEnter()
        {
            base.StateEnter();
            _enemyPos = Vector2.zero;
        }

        public override void StateFixedUpdate()
        {
            target = EnemyBrainBase.ClosestTarget;
            EnemyBrainBase.MoveTo(target);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (EnemyBrainBase.isSpottableInAttackRange)
            {
                StateMachine?.RequestStateChange(EnemyBrainBase.spottableInAttackRangeState);
            }

            if (!EnemyBrainBase.isSpottableDetected)
            {
                StateMachine?.RequestStateChange(EnemyBrainBase.noSpottableDetectedState);
            }
        }
    }
}