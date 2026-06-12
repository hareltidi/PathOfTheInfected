using UnityEngine;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// Helper class for logging combat hit contexts.
    /// </summary>
    public static class CombatHitContextPrinter
    {
        /// <summary>
        /// Logs the given <see cref="CombatHitContext"/> to the console.
        /// </summary>
        /// <param name="context">The <see cref="CombatHitContext"/> to log</param>
        public static void LogCombatHitContext(in CombatHitContext context)
        {
            Debug.Log($"=== Combat Hit Context ===\n" +
                      $"Source: {(context.Source != null ? context.Source.name : "None")}\n" +
                      $"Target: {(context.Target != null ? context.Target.name : "None")}\n" +
                      $"CurrentAttack Definition: {(context.AttackDefinition != null ? context.AttackDefinition.name : "None")}\n" +
                      $"Outcome: {context.Outcome}\n" +
                      $"Final Damage: {context.FinalDamage}\n" +
                      $"Attacker Is Airborne: {context.AttackerIsAirborne}\n");
        }
    }
}