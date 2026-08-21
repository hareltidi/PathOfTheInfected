using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    public class CrouchingAnimState : TidiAnimBaseState
    {

        protected AnimatorManager AnimatorManagerInstance;


        public CrouchingAnimState(AnimatorManager animInstance, TidiAnimStateMachine stateMachine)
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
            if (AnimatorManagerInstance.crouchingIsWalking)
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.CrouchingWalkAnim);
            }
            else
            {
                AnimatorManagerInstance.PlayAnimationIfNotCurrent(AnimatorManagerInstance.CrouchingIdleAnim);
            }
        }

        protected override void TransitionChecks()
        {
            if (!AnimatorManagerInstance.isCrouching)
            {
                stateMachine.RequestStateChange(AnimatorManagerInstance.StandingState);
            }
        }
    }
}
