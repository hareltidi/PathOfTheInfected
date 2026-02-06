using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "SpottableInAttackRange", menuName = "Enemy/States/Core/BaseStates/SpottableInAttackRange", order = 0)]
    public class SpottableInAttackRangeSOBase : EnemyBaseState
    {
        public override void TransitionChecks()
        {
            if (EnemyBrainBase.isSpottableDetected && !EnemyBrainBase.isSpottableInAttackRange)
            {
                _stateMachine?.RequestStateChange(EnemyBrainBase.spottableDetectedState);
            }
            else if (!EnemyBrainBase.isSpottableInAttackRange && !EnemyBrainBase.isSpottableDetected)
            {
                _stateMachine?.RequestStateChange(EnemyBrainBase.noSpottableDetectedState);
            }
        }
    }
}