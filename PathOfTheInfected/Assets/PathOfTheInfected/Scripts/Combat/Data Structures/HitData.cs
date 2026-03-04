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
        public GameObject source;
        public GameObject target;
        public bool isFirstHit;
        public int firstHitDamageBoost;
        public AttackDefinition attackDefinition;
        public int comboDamageScalingLevel;
        public bool isPlayerDamage;
        public bool isAttackerInAir;
        public float timeStamp;
    }
}