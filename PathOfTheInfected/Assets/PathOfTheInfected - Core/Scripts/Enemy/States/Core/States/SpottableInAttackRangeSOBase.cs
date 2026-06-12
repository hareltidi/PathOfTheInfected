using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "SpottableInAttackRange", menuName = "Enemy/States/Core/BaseStates/SpottableInAttackRange", order = 0)]
    public class SpottableInAttackRangeSOBase : EnemyBaseState
    {
        public override void TransitionChecks()
        {
            if (CurrentEnemyBrain.isSpottableDetected && !CurrentEnemyBrain.isSpottableInAttackRange)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.spottableDetectedState);
            }
            else if (!CurrentEnemyBrain.isSpottableInAttackRange && !CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.noSpottableDetectedState);
            }
        }
    }
}