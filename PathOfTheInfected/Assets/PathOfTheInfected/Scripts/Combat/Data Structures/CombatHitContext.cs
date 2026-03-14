using UnityEngine;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// Struct that represents the context of a combat hit to be used inside our combat subsystems.
    /// </summary>
    public struct CombatHitContext
    {
        public GameObject Source;
        public GameObject Target;

        public AttackDefinition AttackDefinition;

        public HitOutcome Outcome;

        public float FinalDamage;

        public int ComboScalingLevel;

        public bool AttackerIsAirborne;

        public bool TargetKilled;
    }
}