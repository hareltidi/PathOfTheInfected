using System;
using UnityEngine;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// Struct that represents the final damage data
    /// </summary>
    [Serializable]
    public struct HitData
    {
        /// <summary>
        /// Who did the hit
        /// </summary>
        public GameObject source;
        /// <summary>
        /// Who was hit
        /// </summary>
        public GameObject target;
        /// <summary>
        /// Is this the first hit of the combo? (only applicable to player attacks)
        /// </summary>
        public bool isFirstHit;
        /// <summary>
        /// How much damage should we add to the first hit? (only applicable to player attacks)
        /// </summary>
        public float firstHitDamageBoost;

        /// <summary>
        /// Reference to the definition of the attack, detailing its properties and behavior.
        /// </summary>
        public AttackDefinition attackDefinition;

        /// <summary>
        /// The current combo multiplier in this attack
        /// </summary>
        public float comboDamageScalingLevel;
        /// <summary>
        /// Did the player inflict the damage?
        /// </summary>
        public bool isPlayerDamage;
        /// <summary>
        /// Is the attacker in the air?
        /// </summary>
        public bool isAttackerInAir;
        /// <summary>
        /// When the attack occurred.
        /// </summary>
        public float timeStamp;

        publ
    }
}