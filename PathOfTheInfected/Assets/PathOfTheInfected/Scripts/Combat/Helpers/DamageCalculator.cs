using UnityEngine;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// Provides methods to calculate the final damage value to be inflicted on a target based on
    /// various parameters such as base damage, combo scaling, and first-hit bonus.
    /// This class is commonly used in conjunction with components like <c>HitData</c>
    /// and <c>HitResponse</c> to determine the result of combat interactions in the game.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// A method for calculating the damage inflicted by a hit.
        /// </summary>
        /// <param name="hitData">The <see cref="HitData"/> that was generated in early steps</param>
        /// <returns>The final damage we need to inflict to the target</returns>
        public static int CalculateDamage(HitData hitData)
        {
            // Get the base damage
            float damage = hitData.attackDefinition.baseDamage;

            // Apply damage scaling and damage boosts
            damage *= hitData.comboDamageScalingLevel;
            damage += hitData.firstHitDamageBoost;

            // Round the damage
            return Mathf.RoundToInt(damage);
        }
    }
}