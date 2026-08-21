using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerJumpFastFallStateSm : PlayerStateSm
    {
        public PlayerJumpFastFallStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
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
            FastFall();
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
            if (!Land() && !WallJumpPostBuffer() && !WallJump() && !WallSlide() && !CoyoteJump() && !AirJump()) Dash();
        }

        private void FastFall()
        {
            player.IncrementVerticalVelocity(moveStats.Gravity * moveStats.GravityOnReleaseMultiplier *
                                              Time.fixedDeltaTime);
        }
    }
}