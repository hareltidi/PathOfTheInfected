using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerDashCancelVerticalStateSm : PlayerStateSm
    {
        private float _dashCancelStartingSpeed;

        private float _dashCancelTime;

        public PlayerDashCancelVerticalStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            _dashCancelStartingSpeed = player.velocity.y;
            _dashCancelTime = 0.0f;
        }

        public override void StateExit()
        {
            base.StateExit();
            player.StopDashTracking("Dash Timer Ended");
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate(); _dashCancelTime += Time.fixedDeltaTime;
            CheckTransitions();
            DashCut();
            if (player.IsGrounded)
                player.Move(moveStats.GroundAcceleration, moveStats.GroundDeceleration, InputManager.Movement);
            else
                player.Move(moveStats.AirAcceleration, moveStats.AirDeceleration, InputManager.Movement);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void OnInputCommand(InputCommand command)
        {
            AirborneInputCommands(command);
        }

        private void CheckTransitions()
        {
            if (Land() || WallSlide() || WallJump() || CoyoteJump() || AirJump() || Dash())
                return;
            DashTimeEnded();
        }

        private bool DashTimeEnded()
        {
            if (_dashCancelTime < (double)moveStats.DashTimeForUpwardsCancel)
                return false;
            playerStateMachine.RequestStateChange(player.DashFastFallState);
            return true;
        }

        private void DashCut()
        {
            if (_dashCancelTime >= (double)moveStats.DashTimeForUpwardsCancel)
                return;
            player.ChangeVerticalVelocity(Mathf.Lerp(_dashCancelStartingSpeed, 0.0f,
                _dashCancelTime / moveStats.DashTimeForUpwardsCancel));
        }
    }
}