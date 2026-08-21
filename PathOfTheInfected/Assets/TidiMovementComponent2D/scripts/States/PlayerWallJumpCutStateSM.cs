using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerWallJumpCutStateSm : PlayerStateSm
    {
        private float _wallJumpCutStartingSpeed;

        private float _wallJumpCutTime;

        public PlayerWallJumpCutStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            _wallJumpCutTime = 0.0f;
            _wallJumpCutStartingSpeed = player.velocity.y;
        }

        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            CheckTransitions();
            WallJumpCut();
            player.Move(moveStats.WallJumpMoveAcceleration, moveStats.WallJumpMoveDeceleration,
                InputManager.Movement);
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
            if (HitHead() || WallJump() || AirJump() || Dash() ||
                _wallJumpCutTime < (double)moveStats.TimeForUpwardsCancel)
                return;
            playerStateMachine.RequestStateChange(player.WallJumpFastFallState, "timer has lapsed");
        }

        private void WallJumpCut()
        {
            if (_wallJumpCutTime >= (double)moveStats.TimeForUpwardsCancel)
                return;
            player.ChangeVerticalVelocity(Mathf.Lerp(_wallJumpCutStartingSpeed, 0.0f,
                _wallJumpCutTime / moveStats.TimeForUpwardsCancel));
            _wallJumpCutTime += Time.fixedDeltaTime;
        }
    }
}