using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerJumpCutStateSm : PlayerStateSm
    {
        private float _jumpCutStartingSpeed;

        private float _jumpCutTime;


        public PlayerJumpCutStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            _jumpCutTime = 0.0f;
            _jumpCutStartingSpeed = player.velocity.y;
        }

        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            CheckTransitions();
            JumpCut();
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
            if (HitHead() || WallJump() || AirJump() || Dash() ||
                _jumpCutTime < (double)moveStats.TimeForUpwardsCancel)
                return;
            playerStateMachine.RequestStateChange(player.JumpFastFallState, "timer has lapsed");
        }

        private void JumpCut()
        {
            if (_jumpCutTime >= (double)moveStats.TimeForUpwardsCancel)
                return;
            player.ChangeVerticalVelocity(Mathf.Lerp(_jumpCutStartingSpeed, 0.0f,
                _jumpCutTime / moveStats.TimeForUpwardsCancel));
            _jumpCutTime += Time.fixedDeltaTime;
        }
    }
}