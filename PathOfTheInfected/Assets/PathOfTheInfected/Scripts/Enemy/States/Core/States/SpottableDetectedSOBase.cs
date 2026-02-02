using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "SpottableDetected", menuName = "Enemy/States/Core/BaseStates/SpottableDetected", order = 0)]
    public class SpottableDetectedSOBase : EnemyBaseState
    {
        public override void TransitionChecks()
        {
            if (_enemy.isSpottableInAttackRange)
            {
                _stateMachine?.RequestStateChange(_enemy.spottableInAttackRangeState);
            }
        }

        public override void DrawGizmosOnSelected(Enemy en)
        {
            base.DrawGizmosOnSelected(en);
        }
    }
}