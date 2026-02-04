using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class AttackContext
    {
        public AttackContext(Enemy owner, Transform target, float timer, AttackPhase phase, bool hasHit, bool isFinished)
        {
            Owner = owner;
            Target = target;
            Timer = timer;
            Phase = phase;
            HasHit = hasHit;
            IsFinished = isFinished;
        }

        // Who is performing the attack
        public Enemy Owner;

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
        Recovery = 2
    }
}