using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    public class CrouchingAnimState : TidiAnimBaseState
    {

        AnimatorManager animatorManagerInstance;


        public CrouchingAnimState(AnimatorManager animInstance, TidiAnimStateMachine stateMachine)
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
            if (animatorManagerInstance.crouchingIsWalking)
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.CrouchingWalkAnim);
            }
            else
            {
                animatorManagerInstance.PlayAnimation(animatorManagerInstance.CrouchingIdleAnim);
            }
        }

        protected override void TransitionChecks()
        {
            if (!animatorManagerInstance.isCrouching)
            {
                stateMachine.RequestStateChange(animatorManagerInstance.StandingState);
            }
        }
    }
}
