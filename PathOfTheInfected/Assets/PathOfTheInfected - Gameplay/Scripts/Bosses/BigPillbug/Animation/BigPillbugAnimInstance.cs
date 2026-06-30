using PathOfTheInfected.Core.Scripts.Boss;
using TidiMovementComponent2D.Animation;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Scripts.Bosses.BigPillbug.Animation
{
    public class BigPillbugAnimInstance : TidiAnimInstance
    {
        [Header("Laser Centipede Animations")]
        public AnimationClip idleAnim;
        public AnimationClip walkAnim;
        public AnimationClip vulnerableAnim;

        public int WalkAnimHash { get; private set; }
        public int IdleAnimHash { get; private set; }
        public int VulnerableAnimHash { get; private set; }

        private BossBrain _ownerBrain;
        private float Speed => _ownerBrain.BossVel.magnitude;
        protected override void AnimationInitialize()
        {
            _ownerBrain = GetComponentInParent<BossBrain>();
        }

        protected override void AnimationStart()
        {
        }

        protected override void SetAnimHashes()
        {
            base.SetAnimHashes();
            WalkAnimHash = Animator.StringToHash(walkAnim.name);
            IdleAnimHash = Animator.StringToHash(idleAnim.name);
            VulnerableAnimHash = Animator.StringToHash(vulnerableAnim.name);
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