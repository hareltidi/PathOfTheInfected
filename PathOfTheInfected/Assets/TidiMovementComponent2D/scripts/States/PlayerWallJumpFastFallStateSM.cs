using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerWallJumpFastFallStateSm : PlayerStateSm
    {
        public PlayerWallJumpFastFallStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
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
            WallJumpFastFall();
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

        private void TransitionChecks()
        {
            if (!Land() && !WallJump() && !WallJumpPostBuffer() && !WallSlide() && !CoyoteJump() && !AirJump()) Dash();
        }

        private void WallJumpFastFall()
        {
            player.IncrementVerticalVelocity(moveStats.Gravity * moveStats.WallJumpGravityOnReleaseMultiplier *
                                              Time.fixedDeltaTime);
        }
    }
}