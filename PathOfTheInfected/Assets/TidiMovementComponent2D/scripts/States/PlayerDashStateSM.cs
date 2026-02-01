using System;
using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TidiMovementComponent2D.States
{
    public class PlayerDashStateSm : PlayerStateSm
    {
        private float _dashDelayTimer;
        private Vector2 _dashIntentDirection;

        private float _dashTimer;
        private bool _isPerformingSlopeDash;
        private float _slopeDashAngle;

        public PlayerDashStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            _isPerformingSlopeDash = false;
            _slopeDashAngle = 0.0f;
            InitiateDash();
        }

        public override void StateExit()
        {
            base.StateExit();
            if (player.IsGrounded)
            {
                player.ResetDashes();
            }
            player.Anim.SetBool("isDashing", false);
            player.IsAirDashing = false;
            if (!NextStateIsGroundState()) return;
            player.ChangeVerticalVelocity(-1f / 1000f);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            DashPhysics();
        }

        public override void OnInputCommand(InputCommand command)
        {
            if (command.type == CommandType.JumpPressed && CanGroundOrCoyoteJump())
            {
                player.JumpBufferTimer = 0.0f;
                player.SpawnJumpParticles(player.jumpParticles);
                playerStateMachine.RequestStateChange(player.JumpAscendingState);
            }
            else
            {
                AirborneInputCommands(command);
            }
        }

        private void TransitionChecks()
        {
            if (WallSlide() || AirJump())
                return;
            DashTimerOver();
        }

        private void InitiateDash()
        {
            ++player.NumberOfDashesUsed;
            if (!player.IsGrounded && player.DashCoyoteTimer > 0.0)
                --player.NumberOfDashesUsed;
            _dashDelayTimer = moveStats.DashFreezeTime;
            _dashTimer = 0.0f;
            player.DashOnGroundTimer = moveStats.TimeBtwDashesOnGround;
            player.DashStartY = player.Rb.position.y;
            Object.Instantiate(player.dashParticles, player.transform.position,
                Quaternion.FromToRotation(Vector2.right,  - player.DashDirection));
            player.GhostTrail.LeaveGhostTrail(moveStats.DashTime * 1.75f);
            player.StopWallSliding();
            player.DashBufferTimer = 0.0f;
            if (player.DashDirection.y > 0.0)
            {
                player.StartDashTracking();
                player.DidHeadBumpSlideThisAirborneState = false;
            }

            player.ChangeVerticalVelocity(0.0f);
            player.velocity.x = 0.0f;
        }

        private void CalculateDashDirection()
        {
            player.DashDirection = InputManager.Movement;
            player.TurnCheck(player.DashDirection);
            var vector2 = Vector2.zero;
            var num1 = Vector2.Distance(player.DashDirection, moveStats.DashDirections[0]);
            for (var index = 0; index < moveStats.DashDirections.Length; ++index)
            {
                if (player.DashDirection == moveStats.DashDirections[index])
                {
                    vector2 = player.DashDirection;
                    break;
                }

                var num2 = Vector2.Distance(player.DashDirection, moveStats.DashDirections[index]);
                if ((Mathf.Abs(moveStats.DashDirections[index].x) != 1.0 ? 0 :
                        Mathf.Abs(moveStats.DashDirections[index].y) == 1.0 ? 1 : 0) != 0)
                {
                    var num3 = num2 - moveStats.DashDiagonallyBias;
                }
                else if (num2 < (double)num1)
                {
                    num1 = num2;
                    vector2 = moveStats.DashDirections[index];
                }
            }

            if (vector2 == Vector2.zero)
                vector2 = !player.IsFacingRight ? Vector2.left : Vector2.right;
            _dashIntentDirection = vector2;
            if (player.IsGrounded && vector2.y < 0.0 && vector2.x != 0.0)
                vector2 = new Vector2(Mathf.Sign(vector2.x), 0.0f);
            player.DashDirection = new Vector2(vector2.x, vector2.y);
        }

        public void DashPhysics()
        {
            if (_dashDelayTimer > 0.0)
            {
                CalculateDashDirection();
                player.ChangeVerticalVelocity(0.0f);
                player.velocity.x = 0.0f;
                _dashDelayTimer -= Time.fixedDeltaTime;
                if (_dashDelayTimer > 0.0)
                    return;
                _isPerformingSlopeDash = player.IsGrounded && player.Controller.State.SlopeAngle > 0.0 &&
                                         player.DashDirection.y == 0.0 && Mathf.Sign(player.DashDirection.x) !=
                                         (double)Mathf.Sign(player.Controller.State.SlopeNormal.x);
                if (_isPerformingSlopeDash)
                    _slopeDashAngle = player.Controller.State.SlopeAngle;
            }

            _dashTimer += Time.fixedDeltaTime;
            if (moveStats.DashDirectionMatchesSlopeDirection && _isPerformingSlopeDash)
            {
                player.velocity.x = Mathf.Cos(_slopeDashAngle * ((float)Math.PI / 180f)) * moveStats.DashSpeed *
                                     player.DashDirection.x;
                player.ChangeVerticalVelocity(Mathf.Sin(_slopeDashAngle * ((float)Math.PI / 180f)) *
                                               moveStats.DashSpeed);
            }
            else if (player.BumpedHead)
            {
                if (moveStats.DashFollowSlopesWhenHeadTouching && player.Controller.State.CeilingAngle > 0.0)
                {
                    var ceilingNormal = player.Controller.State.CeilingNormal;
                    player.ChangeWholeVelocity(player.velocity -
                                                Vector2.Dot(player.velocity, ceilingNormal) * ceilingNormal);
                }
                else
                {
                    player.ChangeVerticalVelocity(0.0f);
                    return;
                }
            }
            else
            {
                player.velocity.x = moveStats.DashSpeed * player.DashDirection.x;
                if (player.DashDirection.y != 0.0 || player.IsAirDashing)
                    player.ChangeVerticalVelocity(moveStats.DashSpeed * player.DashDirection.y);
                else if (!player.IsJumping && player.DashDirection.y == 0.0)
                    player.ChangeVerticalVelocity(-1f / 1000f);
            }

            if (!moveStats.DebugShowDashAngle)
                return;
            Debug.DrawRay((Vector2)player.coll.bounds.center,
                player.velocity.normalized * (moveStats.ExtraRayDebugDistance * 4f), Color.cyan);
        }

        private bool DashTimerOver()
        {
            if (_dashTimer >= (double)moveStats.DashTime)
            {
                if (_dashIntentDirection.y >= 0.0 && Mathf.Abs(player.velocity.x) > (double)moveStats.MaxRunSpeed)
                    player.velocity.x = moveStats.MaxRunSpeed * Mathf.Sign(player.velocity.x);
                if (player.IsGrounded && Mathf.Abs(InputManager.Movement.x) < (double)moveStats.MoveThreshold)
                {
                    player.StateMachine.RequestStateChange(player.IdleState, "is grounded with no movement input");
                    return true;
                }

                if (player.IsGrounded && Mathf.Abs(InputManager.Movement.x) > (double)moveStats.MoveThreshold)
                {
                    if (InputManager.RunIsHeld)
                    {
                        playerStateMachine.RequestStateChange(player.RunState, "is grounded and running");
                        return true;
                    }

                    playerStateMachine.RequestStateChange(player.WalkState, "isGrounded and dashing");
                    return true;
                }

                if (player.velocity.y >= 0.0 && !player.IsGrounded)
                {
                    playerStateMachine.RequestStateChange(player.DashCancelVerticalState,
                        "we're moving up (or 0) and not grounded");
                    return true;
                }

                if (player.velocity.y < 0.0 && !player.IsGrounded)
                {
                    playerStateMachine.RequestStateChange(player.DashFastFallState,
                        "we're moving down and are not grounded");
                    return true;
                }
            }

            return false;
        }

        private bool NextStateIsGroundState()
        {
            var nextState = player.StateMachine.NextState;
            return nextState == player.IdleState || nextState == player.WalkState || nextState == player.RunState;
        }
    }
}