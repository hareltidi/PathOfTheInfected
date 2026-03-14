using TidiMovementComponent2D.Animation;
using UnityEngine;

namespace PathOfTheInfected.Animation
{
    /// <summary>
    /// The animation state for standing. Here all the standing animations will
    /// be evaluated and played.
    /// </summary>
    public class POIStandingAnimState : TidiAnimBaseState
    {
        protected POIAnimInstance AnimatorManagerInstance;
        public POIStandingAnimState(POIAnimInstance animInstance, TidiAnimStateMachine stateMachine)
        {
            this.animInstance = animInstance;
            this.stateMachine = stateMachine;
        }
        public override void StateEnter()
        {
            AnimatorManagerInstance = (POIAnimInstance)animInstance;
        }

        public override void StateExit()
        {

        }

        public override void StateFixedUpdate()
        {
            TransitionChecks();
        }

        public override void StateUpdate()
        {

        }

        public override void EvaluateStateAnimations()
        {
            // Keyboard movement
            if (AnimatorManagerInstance.standingIsRunning)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingRunAnim, 0f);
            }
            else if (AnimatorManagerInstance.standingIsWalking)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingWalkAnim, 0f);
            }
            else
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingIdleAnim, 0f);
            }

            // Other
            if (AnimatorManagerInstance.standingIsJumping)
            {
                AnimatorManagerInstance.PlayAnimationForced(AnimatorManagerInstance.StandingJumpAnim, 0f);
            }

            if (AnimatorManagerInstance.standingIsInAir)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingIdleAnim);
            }

            if (AnimatorManagerInstance.standingIsDashing)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingDashAnim, 0f);
            }

            if (AnimatorManagerInstance.standingIsWallSliding)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingWallslideAnim);
            }

            if (AnimatorManagerInstance.standingIsSliding)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingSlideAnim);
            }
        }

        protected override void TransitionChecks()
        {
            if (!AnimatorManagerInstance.OwnerPlayer.IsGrounded)
            {
                stateMachine.RequestStateChange(AnimatorManagerInstance.InAirState);
            }
        }
    }
}