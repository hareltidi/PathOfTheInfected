using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerCrouchState : PlayerStateSm
    {
        public PlayerCrouchState(PlayerSm player, PlayerStateMachineSm stateMachine) : base(player, stateMachine)
        {
        }

        public override void OnInputCommand(InputCommand command)
        {
            GroundedInputCommands(command);
        }

        public override void StateEnter()
        {
            base.StateEnter();
            UpdateCollider(player.moveStats.CrouchedBoxSize, player.moveStats.CrouchedBoxOffset);
            CheckForAndApplyJumpBuffer();
            CheckForAndApplyDashBuffer();
            player.Controller.CalculateRaySpacing();
        }

        public override void StateExit()
        {
            UpdateCollider(player.StandingBoxSize, player.StandingBoxOffset);
            player.Controller.CalculateRaySpacing();
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            player.Move(moveStats.GroundAcceleration, moveStats.GroundDeceleration, InputManager.Movement,
                moveStats.MaxCrouchSpeed);
            TransitionChecks();
        }

        private void TransitionChecks()
        {
            if (!Slide())
            {
                if (!player.IsGrounded && player.CanUnCrouch())
                {
                    playerStateMachine.RequestStateChange(player.FallingState, "not grounded anymore");
                }
                else if (Mathf.Abs(InputManager.Movement.x) > moveStats.MoveThreshold && !InputManager.CrouchIsHeld && player.CanUnCrouch())
                {
                    player.StateMachine.RequestStateChange(player.WalkState, "no crouch input");
                }
                else if (Mathf.Abs(InputManager.Movement.x) < moveStats.MoveThreshold && !InputManager.CrouchIsHeld && player.CanUnCrouch())
                {
                    player.StateMachine.RequestStateChange(player.IdleState, "no movement input");
                }

            }
        }

        private void UpdateCollider(Vector2 size, Vector2 offset)
        {
            player.BoxCollider.size = size;
            player.BoxCollider.offset = offset;
        }
    }
}
