using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "FlyingEnemyAttack", menuName = "Enemy/States/Flying/FlyingEnemyAttackSO", order = 0)]
    public class FlyingEnemyAttackSO : EnemyBaseState
    {
        /// <summary>
        /// The context containing data and state information for enemy attack behavior.
        /// Used to manage and track the state of an ongoing attack, including the owner performing
        /// the attack, the target being attacked, the timeline of the attack, and the current phase of the attack.
        ///
        /// This uses the persistent AttackContext from EnemyBrainBase to maintain recovery state
        /// across state transitions, preventing recovery timers from being reset when the enemy
        /// temporarily leaves the attack state.
        /// </summary>

        public override void StateEnter()
        {
            if (!CurrentEnemyBrain || CurrentEnemyBrain.AttackTarget == null || !CurrentEnemyBrain.AttackTarget.Transform) return;

            if (CurrentEnemyBrain.AttackContext == null)
            {
                CurrentEnemyBrain.AttackContext = new AttackContext();
                CurrentEnemyBrain.attack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }

            // Start fresh attack if the previous one was finished
            if (CurrentEnemyBrain.AttackContext.IsFinished)
            {
                CurrentEnemyBrain.attack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }
        }

        public override void StateFixedUpdate()
        {
            if (!CurrentEnemyBrain || CurrentEnemyBrain.AttackTarget == null || !CurrentEnemyBrain.AttackTarget.Transform) return;
            base.StateFixedUpdate();
            CurrentEnemyBrain.MoveBoss(Vector2.zero, true);

            // Ensure AttackContext is initialized
            if (CurrentEnemyBrain.AttackContext == null)
            {
                CurrentEnemyBrain.AttackContext = new AttackContext();
                CurrentEnemyBrain.attack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
                return;
            }

            if (!CurrentEnemyBrain.AttackContext.IsFinished)
            {
                CurrentEnemyBrain.attack.AttackLogic(CurrentEnemyBrain.AttackContext);
            }
            else
            {
                CurrentEnemyBrain.attack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }

        }

        public override void TransitionChecks()
        {
            if (!CurrentEnemyBrain.isSpottableInAttackRange && CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.spottableDetectedState);
            }

            if (!CurrentEnemyBrain.isSpottableInAttackRange && !CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.noSpottableDetectedState);
            }

            if (CurrentEnemyBrain.isEnemyDamaged)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.damagedState);
            }
        }
    }
}