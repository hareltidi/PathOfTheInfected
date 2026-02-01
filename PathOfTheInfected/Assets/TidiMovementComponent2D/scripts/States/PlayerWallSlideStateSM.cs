using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerWallSlideStateSm : PlayerStateSm
    {
        public PlayerWallSlideStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            if (player.wallSlideParticles.isPlaying)
                player.wallSlideParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            player.wallSlideParticles.gameObject.SetActive(true);
            player.wallSlideParticles.Play();
            if (player.moveStats.ResetDashOnWallSlide)
                player.ResetDashes();
            if (player.moveStats.ResetJumpsOnWallSlide)
                player.ReplenishJumps();
            player.ResetDashValues();
        }

        public override void StateExit()
        {
            base.StateExit();
            player.wallSlideParticles.Stop();
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            player.ChangeVerticalVelocity(Mathf.Lerp(player.velocity.y, -player.moveStats.WallSlideSpeed,
                player.moveStats.WallSlideDecelerationSpeed * Time.fixedDeltaTime));
            player.Move(moveStats.AirAcceleration, moveStats.AirDeceleration, InputManager.Movement);
        }

        public override void OnInputCommand(InputCommand command)
        {
            if (command.type == CommandType.JumpPressed)
            {
                player.WallJumpCoyoteTimer = 0.0f;
                playerStateMachine.RequestStateChange(player.WallJumpAscendingState, "JumpPressed from WallSlide");
            }
            else
            {
                AirborneInputCommands(command);
            }
        }

        private void TransitionChecks()
        {
            if (player.ShouldStopWallSliding())
            {
                player.WallJumpCoyoteTimer = moveStats.WallJumpCoyoteTime;
                player.StopWallSliding();
                playerStateMachine.RequestStateChange(player.FallingState, "not touching wall anymore");
            }
            else
            {
                if (Dash() || !player.HasLanded())
                    return;
                player.StateMachine.RequestStateChange(player.IdleState, "land");
            }
        }
    }
}