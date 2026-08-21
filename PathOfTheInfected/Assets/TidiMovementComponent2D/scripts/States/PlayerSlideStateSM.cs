using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerSlideStateSm : PlayerStateSm
    {
        public PlayerSlideStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void OnInputCommand(InputCommand command)
        {
            if (command.type == CommandType.JumpPressed && CanGroundOrCoyoteJump())
            {
                player.JumpBufferTimer = 0f;
                player.SpawnJumpParticles(player.jumpParticles);
                playerStateMachine.RequestStateChange(player.JumpAscendingState, "Ground Jump from Slide");
            }
            else
            {
                AirborneInputCommands(command);
            }
        }

        public override void StateEnter()
        {
            base.StateEnter();
        }

        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            HandleSlide();
            player.Move(moveStats.GroundAcceleration, moveStats.GroundDeceleration, InputManager.Movement);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        private void HandleSlide()
        {
            player.IncrementVerticalVelocity(moveStats.Gravity * Time.fixedDeltaTime);
        }

        private void TransitionChecks()
        {
            if (!player.Controller.IsSliding)
            {
                if (player.IsGrounded)
                {
                    if (Mathf.Abs(InputManager.Movement.x) > moveStats.MoveThreshold)
                    {
                        if (InputManager.RunIsHeld && moveStats.movementCapabilities.canRun)
                            player.StateMachine.RequestStateChange(player.RunState,
                                "no longer sliding and hit the ground running");
                        else
                            player.StateMachine.RequestStateChange(player.WalkState,
                                "no longer sliding and hit the ground walking");
                    }
                    else
                    {
                        player.StateMachine.RequestStateChange(player.IdleState,
                            "no longer sliding, no movement input");
                    }
                }
                else
                {
                    player.StateMachine.RequestStateChange(player.FallingState,
                        "no longer sliding, and no longer grounded");
                }
            }
            else
            {
                if (AirJump()) return;
                if (!player.IsGrounded && !player.IsTouchingWall)
                {
                    playerStateMachine.RequestStateChange(player.FallingState, "No longer grounded");
                }
                else
                {
                    if (player.Controller.State.SlopeAngle > moveStats.MaxSlopeAngle &&
                        player.Controller.State.SlopeAngle < moveStats.MinAngleForWallSlide) return;
                    if (player.ShouldWallSlide())
                    {
                        playerStateMachine.RequestStateChange(player.WallSlideState, "Slid onto a wall slide");
                    }
                    else if (Mathf.Abs(InputManager.Movement.x) > moveStats.MoveThreshold)
                    {
                        if (InputManager.RunIsHeld && moveStats.movementCapabilities.canRun)
                            player.StateMachine.RequestStateChange(player.RunState,
                                "Slid onto runnable slope with input");
                        else
                            player.StateMachine.RequestStateChange(player.WalkState,
                                "Slid onto walkable slope with input");
                    }
                    else
                    {
                        player.StateMachine.RequestStateChange(player.IdleState, "Slid onto walkable ground");
                    }
                }
            }
        }
    }
}