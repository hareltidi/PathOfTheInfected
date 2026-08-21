using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerWallJumpAscendingStateSm : PlayerStateSm
    {
        private bool _isPastWallJumpApexThreshold;
        private float _timePastWallJumpApexThreshold;
        private float _wallJumpApexPoint;

        public PlayerWallJumpAscendingStateSm(PlayerSm player, PlayerStateMachineSm stateMachine): base(player, stateMachine)
        {

        }

        public override void StateEnter()
        {
            base.StateEnter();
            InitiateWallJump();
        }

        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            WallJumpAscendingPhysics();
            player.Move(moveStats.WallJumpMoveAcceleration, moveStats.WallJumpMoveDeceleration,
                InputManager.Movement);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void OnInputCommand(InputCommand command)
        {
            if (command.type == CommandType.JumpReleased)
            {
                playerStateMachine.RequestStateChange(player.WallJumpCutState, "JumpReleased during Wall Jump");
            }
            else
            {
                AirborneInputCommands(command);
            }
        }

        private void TransitionChecks()
        {
            if (HitHead() || WallJump() || AirJump())
                return;
            Dash();
        }

        private void InitiateWallJump()
        {
            player.StopWallSliding();
            player.ChangeVerticalVelocity(moveStats.InitialWallJumpVelocity);
            player.velocity.x = Mathf.Abs(moveStats.WallJumpDirection.x) * -player.LastWallDirection;
            player.trailRenderer.emitting = true;
            player.DidHeadBumpSlideThisAirborneState = false;
            player.JumpStartY = player.Rb.position.y;
            player.StartJumpTracking();
            player.ReplenishJumps();
        }

        public void WallJumpAscendingPhysics()
        {
            if (player.velocity.y >= 0.0)
            {
                _wallJumpApexPoint = Mathf.InverseLerp(moveStats.InitialWallJumpVelocity, 0.0f, player.velocity.y);
                if (_wallJumpApexPoint > (double)moveStats.ApexThreshold)
                {
                    if (!_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = true;
                        _timePastWallJumpApexThreshold = 0.0f;
                    }

                    if (!_isPastWallJumpApexThreshold)
                        return;
                    _timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                    if (_timePastWallJumpApexThreshold < (double)moveStats.ApexHangTime)
                        player.ChangeVerticalVelocity(0.0f);
                    else
                        player.ChangeVerticalVelocity(-0.01f);
                }
                else
                {
                    if (_isPastWallJumpApexThreshold)
                        _isPastWallJumpApexThreshold = false;
                    player.IncrementVerticalVelocity(moveStats.WallJumpGravity * Time.fixedDeltaTime);
                }
            }
            else
            {
                playerStateMachine.RequestStateChange(player.WallJumpFastFallState);
            }
        }
    }
}