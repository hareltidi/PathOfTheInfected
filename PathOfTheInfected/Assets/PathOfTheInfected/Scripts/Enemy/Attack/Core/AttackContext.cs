using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class AttackContext
    {
        /// Who is performing the attack
        public EnemyBrainBase Owner;

        // Who is being attacked
        public Transform Target;

        // Timeline
        public float Timer;

        public AttackPhase Phase;

        // State flags
        public bool HasHit;

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