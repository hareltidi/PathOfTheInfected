using TidiMovementComponent2D.Animation;
using UnityEngine;

namespace PathOfTheInfected.Animation
{
    /// <summary>
    /// The animation state for when the player is in the air. here all the In-Air animations will
    /// be evaluated and played.
    /// </summary>
    public class POIInAirAnimState : TidiAnimBaseState
    {
        protected POIAnimInstance AnimatorManagerInstance;
        public POIInAirAnimState(POIAnimInstance animInstance, TidiAnimStateMachine stateMachine)
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

        public override void StateUpdate()
        {
        }

        public override void StateFixedUpdate()
        {
            TransitionChecks();
        }

        public override void EvaluateStateAnimations()
        {
            if (AnimatorManagerInstance.inAirIsAirDashFalling)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.InAirFallAnim);
            }

            if (AnimatorManagerInstance.inAirIsJumping)
            {
                AnimatorManagerInstance.PlayAnimationForced(AnimatorManagerInstance.InAirJumpAnim, 0f);
            }

            if (AnimatorManagerInstance.inAirIsDashing)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingDashAnim, 0f);
            }
            if (AnimatorManagerInstance.inAirIsWallSliding)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.StandingWallslideAnim);
            }
            else if (!AnimatorManagerInstance.inAirIsAirDashFalling && !AnimatorManagerInstance.inAirIsJumping &&
                     !AnimatorManagerInstance.inAirIsDashing)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.InAirFallAnim);
            }
        }

        protected override void TransitionChecks()
        {
            if (AnimatorManagerInstance.Player.IsGrounded)
            {
                stateMachine.RequestStateChange(AnimatorManagerInstance.StandingState);
            }
        }
    }
}