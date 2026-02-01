using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.Core
{
    public class PlayerStateSm
    {
        protected PlayerMovementStatsSm moveStats;
        protected PlayerSm player;
        protected PlayerStateMachineSm playerStateMachine;

        public PlayerStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
        {
            this.player = player;
            playerStateMachine = stateMachine;
            moveStats = this.player.moveStats;
        }

        public virtual void StateEnter()
        {
            if (!player.showEnteredStateDebugLog)
                return;
            Debug.Log("Entered State: " + playerStateMachine.CurrentState);
        }

        public virtual void StateExit()
        {
        }

        public virtual void StateUpdate()
        {
        }

        public virtual void StateFixedUpdate()
        {
            player.JumpTimers();
            player.HandleWallJumpDirectionAndPostBufferTimer();
            player.DashTimers();
        }

        protected bool HitHead()
        {
            if (!player.BumpedHead)
                return false;
            if (moveStats.JumpFollowSlopesWhenHeadTouching && player.Controller.State.CeilingAngle > 0.0)
            {
                var ceilingNormal = player.Controller.State.CeilingNormal;
                player.ChangeWholeVelocity(player.velocity -
                                            Vector2.Dot(player.velocity, ceilingNormal) * ceilingNormal);
                return false;
            }

            if (player.moveStats.KillVerticalVelocityOnHeadBump)
                player.ChangeVerticalVelocity(0.0f);
            playerStateMachine.RequestStateChange(player.JumpFastFallState, "Hit Head");
            return true;
        }

        protected bool CanGroundOrCoyoteJump()
        {
            return player.CanJump();
        }

        private bool CanAirJump()
        {
            return (!player.IsGrounded ? 1 : player.IsSliding ? 1 : 0) != 0 &&
                   player.NumberOfAirJumpsUsed < moveStats.NumberOfAirJumpsAllowed;
        }

        protected bool AirJump()
        {
            if (player.JumpBufferTimer > 0.0 && !player.IsGrounded && !player.IsTouchingWall &&
                player.AttemptWallJumpBuffer())
            {
                player.JumpBufferTimer = 0.0f;
                playerStateMachine.RequestStateChange(player.WallJumpAscendingState, "Buffered Wall Jump");
                return true;
            }

            if (!CanAirJump() || player.JumpBufferTimer <= 0.0)
                return false;
            player.JumpBufferTimer = 0.0f;
            ++player.NumberOfAirJumpsUsed;
            player.SpawnJumpParticles(player.secondJumpParticles);
            playerStateMachine.RequestStateChange(player.JumpAscendingState, "Air Jump");
            return true;
        }

        protected bool CoyoteJump()
        {
            if (!CanGroundOrCoyoteJump() || player.JumpBufferTimer <= 0.0)
                return false;
            player.JumpBufferTimer = 0.0f;
            player.SpawnJumpParticles(player.jumpParticles);
            playerStateMachine.RequestStateChange(player.JumpAscendingState, "Coyote Time Jump");
            return true;
        }

        protected bool Dash()
        {
            if (player.DashBufferTimer <= 0.0 || (!player.CanDash() && !player.CanAirDash()))
                return false;
            player.DashBufferTimer = 0.0f;
            playerStateMachine.RequestStateChange(player.DashState);
            return true;
        }

        protected bool WallSlide()
        {
            if (!player.ShouldWallSlide())
                return false;
            playerStateMachine.RequestStateChange(player.WallSlideState, "Wall Slide");
            return true;
        }

        protected bool CanWallJump()
        {
            return player.CanPostBufferWallJump() || player.CanStandardWallJump();
        }

        protected bool WallJump()
        {
            var flag = player.Controller.State.WallAngle >= (double)moveStats.MinAngleForWallSlide &&
                       player.Controller.State.WallAngle <= (double)moveStats.MaxAngleForWallSlide;
            if (!(CanWallJump() & flag))
                return false;
            player.JumpBufferTimer = 0.0f;
            player.WallJumpCoyoteTimer = 0.0f;
            playerStateMachine.RequestStateChange(player.WallJumpAscendingState, "Wall Jump");
            return true;
        }

        protected bool WallJumpPostBuffer()
        {
            var flag = player.Controller.State.WallAngle >= (double)moveStats.MinAngleForWallSlide &&
                       player.Controller.State.WallAngle <= (double)moveStats.MaxAngleForWallSlide;
            if (((!player.CanPostBufferWallJump() ? 0 : player.JumpBufferTimer > 0.0 ? 1 : 0) & (flag ? 1 : 0)) == 0)
                return false;
            player.JumpBufferTimer = 0.0f;
            player.WallJumpCoyoteTimer = 0.0f;
            playerStateMachine.RequestStateChange(player.WallJumpAscendingState, "Wall Jump Post Buffer State");
            return true;
        }

        protected bool Land()
        {
            if (player.HasLanded())
            {
                if ((player.Controller.State.SlopeAngle <= (double)moveStats.MaxSlopeAngle ? 0 :
                        player.Controller.State.SlopeAngle < (double)moveStats.MinAngleForWallSlide ? 1 : 0) != 0)
                    playerStateMachine.RequestStateChange(player.SlideState, "Landed on slope too steep to walk");
                if (player.IsGrounded && Mathf.Abs(InputManager.Movement.x) < (double)moveStats.MoveThreshold)
                {
                    player.StateMachine.RequestStateChange(player.IdleState, "landed with no move input");
                    return true;
                }

                if (player.IsGrounded && Mathf.Abs(InputManager.Movement.x) > (double)moveStats.MoveThreshold)
                {
                    if (InputManager.RunIsHeld)
                    {
                        playerStateMachine.RequestStateChange(player.RunState, "landed with run input");
                        return true;
                    }

                    playerStateMachine.RequestStateChange(player.WalkState, "landed with walk input");
                    return true;
                }
            }

            return false;
        }

        protected bool Slide()
        {
            if (!player.IsGrounded || !player.Controller.IsSliding)
                return false;
            playerStateMachine.RequestStateChange(player.SlideState);
            return true;
        }

        public virtual void OnInputCommand(InputCommand command)
        {
        }

        public virtual void AirborneInputCommands(InputCommand command)
        {
            if (command.type == CommandType.JumpPressed)
            {
                player.JumpBufferTimer = moveStats.JumpBufferTime;
                player.BufferedJumpState = PlayerSm.BufferedJumpType.Full;
            }
            else if (command.type == CommandType.JumpReleased)
            {
                if (player.JumpBufferTimer <= 0.0)
                    return;
                player.BufferedJumpState = PlayerSm.BufferedJumpType.Cut;
            }
            else
            {
                if (command.type != CommandType.DashPressed)
                    return;
                player.DashBufferTimer = moveStats.DashBufferTime;
            }
        }

        public virtual void GroundedInputCommands(InputCommand command)
        {
            if (command.type == CommandType.JumpPressed && CanGroundOrCoyoteJump())
            {
                player.JumpBufferTimer = 0.0f;
                player.BufferedJumpState = PlayerSm.BufferedJumpType.None;
                player.SpawnJumpParticles(player.jumpParticles);
                playerStateMachine.RequestStateChange(player.JumpAscendingState);
            }
            else if (command.type == CommandType.CrouchPressed && player.moveStats.movementCapabilities.canCrouch)
            {
                playerStateMachine.RequestStateChange(player.CrouchState);
            }
            else
            {
                if (command.type != CommandType.DashPressed || !player.CanDash())
                    return;
                playerStateMachine.RequestStateChange(player.DashState);
            }
        }

        public void CheckForAndApplyJumpBuffer()
        {
            if (player.JumpBufferTimer <= 0.0)
                return;
            player.JumpBufferTimer = 0.0f;
            playerStateMachine.RequestStateChange(player.JumpAscendingState, "Buffered Jump");
        }

        public void CheckForAndApplyDashBuffer()
        {
            if (player.DashBufferTimer <= 0.0)
                return;
            player.DashBufferTimer = 0.0f;
            playerStateMachine.RequestStateChange(player.DashState);
        }

        protected void ReturnToOriginState()
        {
            playerStateMachine.RequestStateChange(player.IdleState);
        }
    }
}
