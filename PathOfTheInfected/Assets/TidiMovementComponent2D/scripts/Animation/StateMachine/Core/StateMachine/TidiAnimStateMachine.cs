using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    public class TidiAnimStateMachine
    {
        public TidiAnimBaseState CurrentState { get; private set; }
        public TidiAnimBaseState PreviousState { get; private set; }
        public TidiAnimBaseState NextState { get; private set; }

        public void InitializeDefaultState(TidiAnimBaseState startState)
        {
            CurrentState = startState;
            CurrentState.StateEnter();
        }


        /// <summary>
        ///   Use this method to request a state change. The state change will be applied on the next fixed update.
        /// </summary>
        /// <param name="newState">The new state to transition to</param>
        public void RequestStateChange(TidiAnimBaseState newState)
        {
            if (NextState == null)
            {
                NextState = newState;
            }
        }

        public void ApplyQueuedStateChange()
        {
            if (NextState != null)
            {
                CurrentState?.StateExit();
                PreviousState = CurrentState;
                CurrentState = NextState;
                NextState = null;
                CurrentState.StateEnter();
            }
        }
    }
}
