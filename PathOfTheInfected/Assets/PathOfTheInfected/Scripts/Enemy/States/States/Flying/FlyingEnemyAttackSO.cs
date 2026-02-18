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
        /// </summary>
        private AttackContext _context = new();

        public override void StateEnter()
        {
            if (CurrentEnemyBrain && CurrentEnemyBrain.AttackTarget != null && CurrentEnemyBrain.AttackTarget.Transform)
            {
                CurrentEnemyBrain.attack.InitAttack(_context, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }
        }

        public override void StateFixedUpdate()
        {
            if (!CurrentEnemyBrain || CurrentEnemyBrain.AttackTarget == null || !CurrentEnemyBrain.AttackTarget.Transform) return;
            base.StateFixedUpdate();
            CurrentEnemyBrain.MoveEnemy(Vector2.zero, true);
            if (!_context.IsFinished)
            {
                CurrentEnemyBrain.attack.AttackLogic(_context);
            }
            else
            {
                CurrentEnemyBrain.attack.InitAttack(_context, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
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