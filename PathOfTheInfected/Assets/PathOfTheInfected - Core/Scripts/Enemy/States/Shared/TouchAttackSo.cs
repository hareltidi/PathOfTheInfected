using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Core.Scripts.Enemy.States.Shared
{
    [CreateAssetMenu(fileName = "TouchAttackSO", menuName = "Enemy/States/Shared/TouchAttackSO", order = 0)]
    public class TouchAttackSo : EnemyBaseState
    {
        public AttackSOBase touchAttack;
        private bool _touched = false;
        public override void StateEnter()
        {
            if (!CurrentEnemyBrain || CurrentEnemyBrain.AttackTarget == null || !CurrentEnemyBrain.AttackTarget.Transform) return;

            if (CurrentEnemyBrain.AttackContext == null)
            {
                CurrentEnemyBrain.AttackContext = new AttackContext();
                touchAttack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }

            // Start fresh attack if the previous one was finished
            if (CurrentEnemyBrain.AttackContext.IsFinished)
            {
                touchAttack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
            }
        }

        public override void StateFixedUpdate()
        {
            if (!CurrentEnemyBrain || CurrentEnemyBrain.AttackTarget == null || !CurrentEnemyBrain.AttackTarget.Transform) return;
            base.StateFixedUpdate();

            _touched = Physics2D.OverlapCircle(CurrentEnemyBrain.transform.position, touchAttack.MaxAttackRange, CurrentEnemyBrain.SpottableMask);

            if (_touched)
            {
                CurrentEnemyBrain.MoveEnemy(Vector2.zero, true);
                if (CurrentEnemyBrain.AttackContext == null)
                {
                    CurrentEnemyBrain.AttackContext = new AttackContext();
                    touchAttack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
                    return;
                }

                if (!CurrentEnemyBrain.AttackContext.IsFinished)
                {
                    touchAttack.AttackLogic(CurrentEnemyBrain.AttackContext);
                }
                else
                {
                    touchAttack.InitAttack(CurrentEnemyBrain.AttackContext, CurrentEnemyBrain, CurrentEnemyBrain.AttackTarget.Transform);
                }
            }
        }
    }
}