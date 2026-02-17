using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Represents the aggressive state for a grounded enemy, managing target acquisition and movement toward detected
    /// targets.
    /// </summary>
    /// <remarks>This state is typically used when an enemy has detected a target and is actively pursuing it.
    /// The state handles updating the target and transitioning to other states based on detection and attack range
    /// conditions. Use this state within an enemy state machine to enable aggressive pursuit behavior.</remarks>
    /// <remarks>Pseudo state: Spottable detected</remarks>
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