using PathOfTheInfected.Combat;
using UnityEngine;

namespace PathOfTheInfected.Player.Combat.Attacks
{
    [CreateAssetMenu(fileName = "PlayerAttackSoBase", menuName = "Attacks/Player/Base/PlayerAttackSoBase", order = 0)]
    public class PlayerAttackSoBase : ScriptableObject
    {
        public PlayerAttackDefinition attackDef;
        public bool debugHit = false;
        protected PlayerCombat PlayerCombat;
        protected CombatFlags AttackFlag;



        public virtual void InitAttack(PlayerCombat playerCombat, CombatFlags attackFlag)
        {
            PlayerCombat = playerCombat;
            AttackFlag = attackFlag;
        }


        public virtual void StartAttack()
        {
            PerformAttack();
        }

        public virtual void PerformAttack()
        {

        }

        public virtual void EndAttack()
        {
            PlayerCombat.ClearCombatState(AttackFlag);
            PlayerCombat.StartRecovery(attackDef.recoveryTime);
        }


        public virtual void ReactToHitResult(HitResult hitResult)
        {
            PlayerCombat.LastHitResult = hitResult;
            if (debugHit)
            {
                Debug.Log($"{hitResult.FinalDamage} Damage was inflicted onto the target with the outcome being: {hitResult.Outcome}");
            }
        }

        public CombatHitContext BuildCombatHitContext(HitResult hitResult)
        {
            CombatHitContext context = new CombatHitContext
            {
                Source = PlayerCombat.gameObject,
                Target = hitResult.Target,
                AttackDefinition = attackDef,
                AttackerIsAirborne = !PlayerCombat.PlayerOwner.IsGrounded,
                Outcome = hitResult.Outcome,
                FinalDamage = hitResult.FinalDamage,
            };
            return context;
        }
    }
}