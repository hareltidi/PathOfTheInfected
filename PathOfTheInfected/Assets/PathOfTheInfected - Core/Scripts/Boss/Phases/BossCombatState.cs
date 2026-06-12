using PathOfTheInfected.Core.Scripts.Boss.States;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Core.Scripts.Boss.Phases
{
    [CreateAssetMenu(fileName = "BossCombatState", menuName = "Boss/States/BossCombatState", order = 0)]
    public class BossCombatState : BossState
    {
        private int _currentAttackIdx;
        public override void StateEnter()
        {
            base.StateEnter();

            if (CurrentBossBrain.AttackContext == null)
            {
                CurrentBossBrain.AttackContext = new AttackContext();
            }

            _currentAttackIdx = GetNextAttackIndex();


            CurrentBossBrain.CurrentAttackIndex = _currentAttackIdx;

            CurrentBossBrain.CurrentAttack.InitAttack(
                CurrentBossBrain.AttackContext,
                CurrentBossBrain,
                CurrentBossBrain.PlayerTarget
            );
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            if (!CurrentBossBrain || !CurrentBossBrain.PlayerTarget) return;
            // Ensure AttackContext is initialized
            if (CurrentBossBrain.AttackContext == null)
            {
                CurrentBossBrain.AttackContext = new AttackContext();
                CurrentBossBrain.CurrentAttack.InitAttack(CurrentBossBrain.AttackContext, CurrentBossBrain, CurrentBossBrain.PlayerTarget);
                return;
            }

            if (!CurrentBossBrain.AttackContext.IsFinished)
            {
                CurrentBossBrain.CurrentAttack.AttackLogic(CurrentBossBrain.AttackContext);
            }
            else
            {
                _currentAttackIdx = GetNextAttackIndex();
                CurrentBossBrain.CurrentAttackIndex = _currentAttackIdx;
                CurrentBossBrain.CurrentAttack.InitAttack(CurrentBossBrain.AttackContext, CurrentBossBrain, CurrentBossBrain.PlayerTarget);
            }
        }

        public int GetNextAttackIndex()
        {
            if (!CurrentBossBrain.CurrentPhase || CurrentBossBrain.CurrentPhase.AllowedAttacks.Count == 0)
            {
                Debug.LogWarning("No allowed attacks in the current phase.");
                return -1; // Return -1 to indicate no valid attack index
            }

            switch (CurrentBossBrain.CurrentPhase.attackSelectionMode)
            {
                case AttackSelectionMode.Random:
                    return Random.Range(0, CurrentBossBrain.CurrentPhase.AllowedAttacks.Count);
                case AttackSelectionMode.Sequence:
                    int nextIndex = CurrentBossBrain.CurrentAttackIndex + 1;
                    if (nextIndex >= CurrentBossBrain.CurrentPhase.AllowedAttacks.Count)
                    {
                        nextIndex = 0; // Loop back to the start of the list
                    }
                    return nextIndex;
            }

            return -1; // Return -1 to indicate no valid attack index
        }
    }
}