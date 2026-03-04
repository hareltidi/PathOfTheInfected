using UnityEngine;

namespace PathOfTheInfected.Combat
{
    public class DamageCalculator
    {
        /// <summary>
        /// A method for calculating the damage inflicted by a hit.
        /// </summary>
        /// <param name="hitData">The <see cref="HitData"/> that was generated in early steps</param>
        /// <returns>The final damage we need to inflict to the target</returns>
        public static int CalculateDamage(HitData hitData)
        {
            int damage = hitData.attackDefinition.baseDamage;

            damage *= hitData.comboDamageScalingLevel;
            damage += hitData.firstHitDamageBoost;

            return Mathf.RoundToInt(damage);
        }
    }
}