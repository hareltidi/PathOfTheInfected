using UnityEditorInternal;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyAttackSOBase", menuName = "Enemy/States/Grounded/EnemyAttackSOBase", order = 0)]
    public class EnemyAttackSOBase : EnemyBaseState
    {
        /// <summary>
        /// The context containing data and state information for enemy attack behavior.
        /// Used to manage and track the state of an ongoing attack, including the owner performing
        /// the attack, the target being attacked, the timeline of the attack, and the current phase of the attack.
        /// </summary>
        AttackContext context = new();

        public override void StateEnter()
        {
            if (CurrentEnemyBrain && CurrentEnemyBrain.AttackTarget != null && CurrentEnemyBrain.AttackTarget.Transform)
            {
                CurrentEnemyBrain.attack.InitAttack(context, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }
        }

        public override void StateFixedUpdate()
        {
            // TODO: for some reason, this line makes the transitions between attack and aggro states on grounded enemies to go insane. Why? Why you askin' me?
            CurrentEnemyBrain.MoveEnemy(Vector2.zero);
            if (!CurrentEnemyBrain || CurrentEnemyBrain.AttackTarget == null || !CurrentEnemyBrain.AttackTarget.Transform) return;
            base.StateFixedUpdate();
            if (!context.IsFinished)
            {
                CurrentEnemyBrain.attack.AttackLogic(context);
            }
            else
            {
                CurrentEnemyBrain.attack.InitAttack(context, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
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
        }
    }
}