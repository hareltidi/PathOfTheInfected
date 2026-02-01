using System.Runtime.CompilerServices;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace TidiMovementComponent2D.Core
{
    public class PlayerStateMachineSm
    {
        private string _transitionReason;

        public PlayerStateSm CurrentState { get; private set; }

        public PlayerStateSm PreviousState { get; private set; }

        public PlayerStateSm NextState { get; private set; }

        public void InitializeDefaultState(PlayerStateSm startState)
        {
            CurrentState = startState;
            CurrentState.StateEnter();
        }


        /// <summary>
        ///     <para>Use this method to request a state change. The state change will be applied on the next fixed update. </para>
        /// </summary>
        public void RequestStateChange(PlayerStateSm newState, [CallerMemberName] string reason = "")
        {
            if (NextState == null)
            {
                NextState = newState;
                _transitionReason = reason;
            }
        }

        public void ApplyQueuedStateChange()
        {
            if (NextState != null)
            {
                if (PlayerSm.Instance.showStateTrailLog)
                    Debug.Log("STATE TRANSITION: [" + CurrentState?.GetType().Name + "] -> [" +
                              NextState.GetType().Name + "] | Reason: [" + _transitionReason + "]");
                CurrentState?.StateExit();
                PreviousState = CurrentState;
                CurrentState = NextState;
                NextState = null;
                _transitionReason = null;
                CurrentState.StateEnter();
            }
        }

        public void OnInputCommand(InputCommand command)
        {
            CurrentState?.OnInputCommand(command);
        }
    }
}