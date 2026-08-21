using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerDashFastFallStateSm : PlayerStateSm
    {
        public PlayerDashFastFallStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
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
            CheckTransitions();
            DashFastFall();
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
            if (!Land() && !WallSlide() && !WallJumpPostBuffer() && !WallJump() && !CoyoteJump() && !AirJump())
            {
                Dash();
            }
            if (player.Controller.IsStep(player.velocity) || player.IsGrounded)
            {
                ReturnToOriginState();
            }
        }

        private void DashFastFall()
        {
            player.IncrementVerticalVelocity(moveStats.Gravity * moveStats.DashGravityOnReleaseMultiplier *
                                              Time.fixedDeltaTime);
        }
    }
}