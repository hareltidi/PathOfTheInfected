using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Represents the context of an attack, providing essential information
    /// that other systems or components need to execute, manage, and finalize attacks.
    /// </summary>
    public class AttackContext
    {
        /// <summary>
        /// Who is performing the attack
        /// </summary>
        public IAttackOwnerable Owner;

        /// <summary>
        /// Who is being attacked
        /// </summary>
        public Transform Target;

        /// <summary>
        /// Tracks the elapsed time since the current attack phase began.
        /// This is used to manage transitions between attack phases (e.g., wind-up, active, recovery)
        /// and determine when time-dependent conditions are met during an attack.
        /// </summary>
        public float Timer;

        /// <summary>
        /// The current phase of the attack, representing its progression through distinct
        /// stages: WindUp, Active, Recovery, and PoiseRecovery. Each stage corresponds
        /// to a specific point in the attack lifecycle, determining the behavior and
        /// logic to be executed during that phase.
        /// </summary>
        public AttackPhase Phase;

        /// <summary>
        /// State flags
        /// </summary>
        public bool HasHit;


        /// <summary>
        /// Indicates whether the attack process has been completed.
        /// This flag is set to true when the attack has fully executed, signaling
        /// that no further logic or updates are required for the current attack context.
        /// </summary>
        public bool IsFinished;
    }

    public enum AttackPhase
    {
        WindUp = 0,
        Active = 1,
        Recovery = 2,
        PoiseRecovery = 3
    }

    public interface IAttackOwnerable
    {
        GameObject GameObject { get; set; }
        bool IsFacingRight { get; set; }
        bool IsGrounded { get;}
        Transform Transform { get; set;}

        float CurrentPoise { get; set; }

        float MaxPoise { get; set; }

        LayerMask SpottableMask { get; set; }

    }
}