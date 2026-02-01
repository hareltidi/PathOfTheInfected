using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerJumpAscendingStateSm : PlayerStateSm
    {
        private float _apexPoint;
        private bool _isPastApexThreshold;
        private float _timePastApexThreshold;

        public PlayerJumpAscendingStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            _isPastApexThreshold = false;
            _timePastApexThreshold = 0.0f;
            InitiateJump();
            if (player.BufferedJumpState == PlayerSm.BufferedJumpType.Cut)
                playerStateMachine.RequestStateChange(player.JumpCutState, "Buffered jump cut");
            player.BufferedJumpState = PlayerSm.BufferedJumpType.None;
        }

        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            JumpAscendingPhysics();
            player.Move(moveStats.AirAcceleration, moveStats.AirDeceleration, InputManager.Movement);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        private void InitiateJump()
        {
            player.ConsumeCoyoteTime();
            player.ChangeVerticalVelocity(moveStats.InitialJumpVelocity);
            if (!player.trailRenderer.emitting)
            {
                player.trailRenderer.emitting = true;
            }
            player.DidHeadBumpSlideThisAirborneState = false;
            player.JumpStartY = player.Rb.position.y;
            player.StartJumpTracking();
        }

        private void JumpAscendingPhysics()
        {
            if (player.velocity.y >= 0.0)
            {
                _apexPoint = Mathf.InverseLerp(moveStats.InitialJumpVelocity, 0.0f, player.velocity.y);
                if (_apexPoint > (double)moveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0.0f;
                    }

                    if (!_isPastApexThreshold)
                        return;
                    _timePastApexThreshold += Time.fixedDeltaTime;
                    if (_timePastApexThreshold < (double)moveStats.ApexHangTime)
                        player.ChangeVerticalVelocity(0.0f);
                    else
                        player.ChangeVerticalVelocity(-0.01f);
                }
                else
                {
                    if (_isPastApexThreshold)
                        _isPastApexThreshold = false;
                    player.IncrementVerticalVelocity(moveStats.Gravity * Time.fixedDeltaTime);
                }
            }
            else
            {
                player.StateMachine.RequestStateChange(player.JumpFastFallState);
            }
        }

        public override void OnInputCommand(InputCommand command)
        {
            if (command.type == CommandType.JumpReleased && player.velocity.y > 0.0)
                playerStateMachine.RequestStateChange(player.JumpCutState, "JumpReleased during Jump Ascent");
            else
                AirborneInputCommands(command);
        }

        private void TransitionChecks()
        {
            if (HitHead() || WallJump() || AirJump())
                return;
            Dash();
        }
    }
}