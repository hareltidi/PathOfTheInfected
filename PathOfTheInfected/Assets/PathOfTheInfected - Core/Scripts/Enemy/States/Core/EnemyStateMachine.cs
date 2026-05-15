using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class EnemyStateMachine
    {
        public EnemyBaseState CurrentState { get; set; }
        public EnemyBaseState PreviousState { get; set; }
        public EnemyBaseState NextState { get; set; }


        /// <summary>
        /// Sets the first state for our states to run
        /// </summary>
        /// <param name="startingState">The first state we should be in at the start of the game</param>
        public void InitializeDefaultState(EnemyBaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.StateEnter();
        }

        /// <summary>
        /// Requests a state change
        /// </summary>
        /// <param name="newState">The new state to switch to</param>
        /// <param name="reason">Why were switching states - optional</param>
        /// <param name="debug">Should we print the state transition and the reason for the transition?</param>
        public void RequestStateChange(EnemyBaseState newState, string reason = "", bool debug = false)
        {
            if (!NextState)
            {
                NextState = newState;
            }
            if (reason != "" && debug)
            {
                Debug.Log($"State change requested: {CurrentState?.name} -> {newState.name}. Reason: {reason}");
            }
        }

        /// <summary>
        /// Applies our que of states to be executed
        /// </summary>
        public void ApplyQueuedStateChange()
        {
            if (NextState)
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