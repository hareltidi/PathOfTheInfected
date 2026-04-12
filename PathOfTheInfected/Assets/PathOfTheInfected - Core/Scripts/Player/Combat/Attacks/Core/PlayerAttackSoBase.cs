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


        /// <summary>
        /// Initializes the player's attack by setting the associated combat manager and attack flags.
        /// </summary>
        /// <param name="playerCombat">The player's combat manager responsible for handling attacks and combat states.</param>
        /// <param name="attackFlag">The combat flag indicating the type or state of the attack being initiated.</param>
        public virtual void InitAttack(PlayerCombat playerCombat, CombatFlags attackFlag)
        {
            PlayerCombat = playerCombat;
            AttackFlag = attackFlag;
        }


        public virtual void StartAttack()
        {
            PerformAttack();
        }


        /// <summary>
        /// Executes the core logic for performing the player's attack action. This method is designed to be overridden
        /// by specific attack implementations to define the behavior of the attack, such as executing collision checks,
        /// applying damage, or triggering animations.
        /// </summary>
        public virtual void PerformAttack()
        {

        }


        /// <summary>
        /// Ends the current attack by clearing the associated combat state and initiating the recovery period.
        /// </summary>
        public virtual void EndAttack()
        {
            PlayerCombat.ClearCombatState(AttackFlag);
            PlayerCombat.StartRecovery(attackDef.recoveryTime);
        }


        /// <summary>
        /// Processes the results of a hit during combat, updating the combat state with the final damage and hit outcome.
        /// Optionally logs debug information about the damage inflicted and the outcome if debugging is enabled.
        /// </summary>
        /// <param name="hitResult">The result of the hit, including damage dealt, outcome, and target information.</param>
        public virtual void ReactToHitResult(HitResult hitResult)
        {
            PlayerCombat.LastHitResult = hitResult;
            if (debugHit)
            {
                Debug.Log($"{hitResult.FinalDamage} Damage was inflicted onto the target with the outcome being: {hitResult.Outcome}");
            }
        }

        /// <summary>
        /// Builds a context object encapsulating the details of a combat hit, including combat sources, targets, and outcomes.
        /// </summary>
        /// <param name="hitResult">The result of a successfully executed hit, containing outcome, damage data, and the target.</param>
        /// <returns>A <see cref="CombatHitContext"/> struct containing combat hit information for further processing.</returns>
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