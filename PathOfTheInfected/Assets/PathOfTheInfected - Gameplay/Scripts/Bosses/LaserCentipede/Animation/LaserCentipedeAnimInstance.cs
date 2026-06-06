using TidiMovementComponent2D.Animation;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Scripts.Bosses.LaserCentipede.Animation
{
    public class LaserCentipedeAnimInstance : TidiAnimInstance
    {
        [Header("Laser Centipede Animations")]
        public AnimationClip idleAnim;
        public AnimationClip walkAnim;

        private LaserCentipedeBrain _ownerBrain;
        private float Speed => _ownerBrain.BossVel.magnitude;

        public int WalkAnimHash { get; private set; }
        public int IdleAnimHash { get; private set; }
        protected override void AnimationInitialize()
        {
            _ownerBrain = GetComponentInParent<LaserCentipedeBrain>();
        }

        protected override void SetAnimHashes()
        {
            base.SetAnimHashes();
            WalkAnimHash = Animator.StringToHash(walkAnim.name);
            IdleAnimHash = Animator.StringToHash(idleAnim.name);
        }

        protected override void AnimationStart()
        {
        }

        protected override void AnimationUpdate()
        {
            PlayAnimationIfNotCurrent(Speed > 0.1f ? WalkAnimHash : IdleAnimHash);
        }

        protected override void AnimationFixedUpdate()
        {
        }
    }
}