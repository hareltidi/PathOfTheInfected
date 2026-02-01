using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.States
{
    public class PlayerRunStateSm : PlayerStateSm
    {
        public PlayerRunStateSm(PlayerSm player, PlayerStateMachineSm stateMachine)
            : base(player, stateMachine)
        {
        }

        public override void StateEnter()
        {
            base.StateEnter();
            CheckForAndApplyJumpBuffer();
            CheckForAndApplyDashBuffer();
        }

        public override void StateExit()
        {
            base.StateExit();
            if (player.speedParticles.isPlaying) player.speedParticles.Stop();
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }

        public override void StateFixedUpdate()
        {
            base.StateFixedUpdate();
            TransitionChecks();
            player.Move(moveStats.GroundAcceleration, moveStats.GroundDeceleration, InputManager.Movement,
                moveStats.MaxRunSpeed);
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
                    playerStateMachine.RequestStateChange(player.FallingState, "not grounded anymore");
                else if (Mathf.Abs(InputManager.Movement.x) < moveStats.MoveThreshold)
                    player.StateMachine.RequestStateChange(player.IdleState, "no movement input");
                else if (Mathf.Abs(InputManager.Movement.x) > moveStats.MoveThreshold && !InputManager.RunIsHeld)
                    player.StateMachine.RequestStateChange(player.WalkState, "no run input");
            }
        }
    }
}