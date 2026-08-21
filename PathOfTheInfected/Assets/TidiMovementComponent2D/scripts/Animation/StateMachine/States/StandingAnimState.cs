using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    public class StandingAnimState : TidiAnimBaseState
    {
       protected AnimatorManager AnimatorManagerInstance;
        public StandingAnimState(AnimatorManager animInstance, TidiAnimStateMachine stateMachine)
        {
            this.animInstance = animInstance;
            this.stateMachine = stateMachine;
        }
        public override void StateEnter()
        {
            AnimatorManagerInstance = (AnimatorManager)animInstance;
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
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingRunAnim);
            }
            else if (AnimatorManagerInstance.standingIsWalking)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingWalkAnim);
            }
            else
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingIdleAnim);
            }

            // Other
            if (AnimatorManagerInstance.standingIsJumping)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingJumpAnim);
            }

            if (AnimatorManagerInstance.standingIsInAir)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingIdleAnim);
            }

            if (AnimatorManagerInstance.standingIsDashing)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingDashAnim,
                    0.2f, 0,true, true);
            }

            if (AnimatorManagerInstance.standingIsWallSliding)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingWallslideAnim);
            }

            if (AnimatorManagerInstance.standingIsAirDashFalling)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingIdleAnim);
            }

            if (AnimatorManagerInstance.standingIsSliding)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingSlideAnim);
            }
        }

        protected override void TransitionChecks()
        {
            if (AnimatorManagerInstance.isCrouching)
            {
                stateMachine.RequestStateChange(AnimatorManagerInstance.CrouchingState);
            }
        }
    }
}
