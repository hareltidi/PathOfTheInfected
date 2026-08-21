using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerFallingStateSm : PlayerStateSm
    {
        private bool _hasTriggeredFallAnimation;

        public PlayerFallingStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            _hasTriggeredFallAnimation = false;
        }

        public override void StateExit()
        {
            base.StateExit();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            CheckAndTriggerFallAnimation();
            Fall();
            player.Move(moveStats.AirAcceleration, moveStats.AirDeceleration, InputManager.Movement);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        private void Fall()
        {
            player.IncrementVerticalVelocity(moveStats.Gravity * Time.fixedDeltaTime);
        }

        public override void OnInputCommand(InputCommand command)
        {
            AirborneInputCommands(command);
        }

        private void TransitionChecks()
        {
            if (!Land() && !WallSlide() && !WallJumpPostBuffer() && !WallJump() && !CoyoteJump() && !AirJump()) Dash();
        }

        private void CheckAndTriggerFallAnimation()
        {
            if (!_hasTriggeredFallAnimation &&
                player.FallStartY - player.Rb.position.y >= moveStats.MinFallDistanceForLand)
            {
                _hasTriggeredFallAnimation = true;
            }
        }
    }
}