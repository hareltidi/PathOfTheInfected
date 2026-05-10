using PathOfTheInfected.Animation.BlendSpaces;
using TidiMovementComponent2D.Animation;
using TidiMovementComponent2D.Animation.BlendSpaces;
using TidiMovementComponent2D.Animation.BlendSpaces.Playables;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Enemies
{
    public class BatAnimInstance : TidiAnimInstance
    {
        [SerializeField] private BlendSpace1DFloat movementBlendSpace;
        [SerializeField] private TidiAnimationDriver animationDriver;
        private BatEnemyBrain _brain;
        private float _batSpeed;

        protected override void AnimationInitialize()
        {
            _brain = GetComponent<BatEnemyBrain>();
        }
        protected override void AnimationStart()
        {

        }

        protected override void AnimationUpdate()
        {
            _batSpeed = _brain.EnemyVel.magnitude;
            BlendResult result = movementBlendSpace.Evaluate(_batSpeed);
            animationDriver?.Apply(in result);
        }

        protected override void AnimationFixedUpdate()
        {

        }
    }
}