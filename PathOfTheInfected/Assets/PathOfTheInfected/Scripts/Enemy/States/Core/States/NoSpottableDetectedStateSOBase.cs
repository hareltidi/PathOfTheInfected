using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "NoSpottableDetected", menuName = "Enemy/States/Core/BaseStates", order = 0)]
    public class NoSpottableDetectedStateSOBase : EnemyBaseState
    {
        public override void TransitionChecks()
        {
            if (_enemy.isSpottableDetected)
            {
                _stateMachine?.RequestStateChange(_enemy.spottableDetectedState);
            }

            if (_enemy.spottableInAttackRangeState)
            {
                _stateMachine?.RequestStateChange(_enemy.spottableInAttackRangeState);
            }
        }
    }
}