using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    public class StandingAnimState : TidiAnimBaseState
    {
        AnimatorManager animatorManagerInstance;
        public StandingAnimState(AnimatorManager animInstance, TidiAnimStateMachine stateMachine)
        {
            this.animInstance = animInstance;
            this.stateMachine = stateMachine;
        }
        public override void StateEnter()
        {
            animatorManagerInstance = (AnimatorManager)animInstance;

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
            if (animatorManagerInstance.standingIsRunning)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingRunAnim);
            }
            else if (animatorManagerInstance.standingIsWalking)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingWalkAnim);
            }
            else
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingIdleAnim);
            }

            // Other
            if (animatorManagerInstance.standingIsJumping)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingJumpAnim);
            }

            if (animatorManagerInstance.standingIsInAir)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingIdleAnim);
            }

            if (animatorManagerInstance.standingIsDashing)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingDashAnim, 0.2f, true, true);
            }

            if (animatorManagerInstance.standingIsWallSliding)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingWallslideAnim);
            }

            if (animatorManagerInstance.standingIsAirDashFalling)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingIdleAnim);
            }

            if (animatorManagerInstance.standingIsSliding)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.StandingSlideAnim);
            }
        }

        protected override void TransitionChecks()
        {
            if (animatorManagerInstance.isCrouching)
            {
                stateMachine.RequestStateChange(animatorManagerInstance.CrouchingState);
            }
        }
    }
}
