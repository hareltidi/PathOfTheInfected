using TidiMovementComponent2D.Animation;

namespace PathOfTheInfected.Animation
{
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

            // combat
        }

        protected override void TransitionChecks()
        {
            if (!AnimatorManagerInstance.Player.IsGrounded)
            {
                stateMachine.RequestStateChange(AnimatorManagerInstance.InAirState);
            }
        }
    }
}