using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerIdleStateSm : PlayerStateSm
    {
        public PlayerIdleStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            CheckForAndApplyJumpBuffer();
            CheckForAndApplyDashBuffer();
            player.trailRenderer.emitting = false;
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            player.Move(moveStats.GroundAcceleration, moveStats.GroundDeceleration, InputManager.Movement);
        }

        public override void OnInputCommand(InputCommand command)
        {
            GroundedInputCommands(command);
        }

        private void TransitionChecks()
        {
            if (!Slide())
            {
                if (!player.IsGrounded)
                    playerStateMachine.RequestStateChange(player.FallingState, "No longer grounded");
                else if (Mathf.Abs(InputManager.Movement.x) > moveStats.MoveThreshold && !InputManager.RunIsHeld)
                    player.StateMachine.RequestStateChange(player.WalkState, "movement");
                else if (Mathf.Abs(InputManager.Movement.x) > moveStats.MoveThreshold && InputManager.RunIsHeld &&
                         moveStats.movementCapabilities.canRun)
                    player.StateMachine.RequestStateChange(player.RunState, "movement with run button held");
            }
        }
    }
}