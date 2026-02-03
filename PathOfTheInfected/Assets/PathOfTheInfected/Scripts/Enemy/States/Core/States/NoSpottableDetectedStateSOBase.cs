using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "NoSpottableDetected", menuName = "Enemy/States/Core/BaseStates/NoSpottableDetected", order = 0)]
    public abstract class NoSpottableDetectedStateSOBase : EnemyBaseState
    {
        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
        }

        public override void StateEnter()
        {
            base.StateEnter();
        }

        public override void DrawGizmosOnSelected(Enemy en)
        {
            base.DrawGizmosOnSelected(en);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void StateInit(Enemy enemy, EnemyStateMachine stateMachine)
        {
            base.StateInit(enemy, stateMachine);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (_enemy.isSpottableDetected)
            {
                _stateMachine.RequestStateChange(_enemy.spottableDetectedState);

            }

            if (_enemy.isSpottableInAttackRange)
            {
                _stateMachine.RequestStateChange(_enemy.spottableInAttackRangeState);
            }
        }
    }
}