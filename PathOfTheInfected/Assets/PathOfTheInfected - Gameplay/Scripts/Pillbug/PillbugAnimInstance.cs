using PathOfTheInfected.Animation.BlendSpaces;
using TidiMovementComponent2D.Animation;
using TidiMovementComponent2D.Animation.BlendSpaces;
using TidiMovementComponent2D.Animation.BlendSpaces.Playables;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Enemies
{
    public class PillbugAnimInstance : TidiAnimInstance
    {
        [Header("Pillbug Animation - movement")]
        [Header("Pillbug Animation Settings")]
        [SerializeField] private AnimationClip attackClip;
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private AnimationClip idleClip;

        private PillbugEnemyBrain _brain;
        private float _pillbugSpeed;
        public int AttackAnimHash { get; private set; }
        public int WalkAnimHash { get; private set; }

        public int IdleAnimHash { get; private set; }


        protected override void AnimationInitialize()
        {
            _brain = GetComponent<PillbugEnemyBrain>();
        }

        protected override void AnimationStart()
        {

        }

        protected override void AnimationUpdate()
        {
        }

        protected override void AnimationFixedUpdate()
        {
            _pillbugSpeed = _brain.EnemyVel.magnitude;
            if (_pillbugSpeed > 0.1f)
            {
                PlayAnimationIfNotCurrent(WalkAnimHash, 0f, 0, true);
            }
            else
            {
                PlayAnimationIfNotCurrent(IdleAnimHash, 0f, 0, true);
            }
        }

        protected override void SetAnimHashes()
        {
            base.SetAnimHashes();
            AttackAnimHash = Animator.StringToHash(attackClip.name);
            WalkAnimHash = Animator.StringToHash(walkClip.name);
            IdleAnimHash = Animator.StringToHash(idleClip.name);
        }
    }
}