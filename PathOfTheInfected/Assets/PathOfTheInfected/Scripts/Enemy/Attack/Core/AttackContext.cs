using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Represents the context of an attack, providing essential information
    /// that other systems or components need to execute, manage, and finalize attacks.
    /// </summary>
    public class AttackContext
    {
        /// Who is performing the attack
        public EnemyBrainBase Owner;

        /// Who is being attacked
        public Transform Target;

        /// <summary>
        /// Tracks the elapsed time since the current attack phase began.
        /// This is used to manage transitions between attack phases (e.g., wind-up, active, recovery)
        /// and determine when time-dependent conditions are met during an attack.
        /// </summary>
        public float Timer;

        /// The current phase of the attack, representing its progression through distinct
        /// stages: WindUp, Active, Recovery, and PoiseRecovery. Each stage corresponds
        /// to a specific point in the attack lifecycle, determining the behavior and
        /// logic to be executed during that phase.
        public AttackPhase Phase;

        /// State flags
        public bool HasHit;


        /// Indicates whether the attack process has been completed.
        /// This flag is set to true when the attack has fully executed, signaling
        /// that no further logic or updates are required for the current attack context.
        public bool IsFinished;
    }

    public enum AttackPhase
    {
        WindUp = 0,
        Active = 1,
        Recovery = 2,
        PoiseRecovery = 3
    }
}