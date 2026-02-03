using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "SpottableInAttackRange", menuName = "Enemy/States/Core/BaseStates/SpottableInAttackRange", order = 0)]
    public class SpottableInAttackRangeSOBase : EnemyBaseState
    {
        public override void TransitionChecks()
        {
            if (_enemy.isSpottableDetected && !_enemy.isSpottableInAttackRange)
            {
                _stateMachine?.RequestStateChange(_enemy.spottableDetectedState);
            }
            else if (!_enemy.isSpottableInAttackRange && !_enemy.isSpottableDetected)
            {
                _stateMachine?.RequestStateChange(_enemy.noSpottableDetectedState);
            }
        }
    }
}