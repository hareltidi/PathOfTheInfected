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

        public override void DrawGizmosOnSelected(EnemyBrainBase en)
        {
            base.DrawGizmosOnSelected(en);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void StateInit(EnemyBrainBase enemyBrainBase, EnemyStateMachine stateMachine)
        {
            base.StateInit(enemyBrainBase, stateMachine);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (EnemyBrainBase.isSpottableDetected)
            {
                _stateMachine.RequestStateChange(EnemyBrainBase.spottableDetectedState);

            }

            if (EnemyBrainBase.isSpottableInAttackRange)
            {
                _stateMachine.RequestStateChange(EnemyBrainBase.spottableInAttackRangeState);
            }
        }
    }
}