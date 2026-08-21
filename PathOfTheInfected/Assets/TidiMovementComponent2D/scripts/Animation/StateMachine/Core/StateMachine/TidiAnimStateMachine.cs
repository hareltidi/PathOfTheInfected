
namespace TidiMovementComponent2D.Animation
{
    public class TidiAnimStateMachine
    {

        /// <summary>
        /// The current animation state wer'e in
        /// </summary>
        public TidiAnimBaseState CurrentState { get; private set; }

        /// <summary>
        /// The previous animation state we wer'e in
        /// </summary>
        public TidiAnimBaseState PreviousState { get; private set; }

        /// <summary>
        /// The animation state we want to switch to from our current state.
        /// </summary>
        public TidiAnimBaseState NextState { get; private set; }

        /// <summary>
        /// Initializes the first state of the Animation instance. Call this method in the awake/Start method
        /// In your Anim instance.
        /// </summary>
        /// <param name="startState">The first animation state we should be in</param>
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

        /// <summary>
        /// Applies our requests to switch states and initializes the new state.
        /// </summary>
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
