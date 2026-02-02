namespace PathOfTheInfected.Enemy
{
    public class EnemyStateMachine
    {
        public EnemyBaseState CurrentState { get; set; }
        public EnemyBaseState PreviousState { get; set; }
        public EnemyBaseState NextState { get; set; }

        public void InitializeDefaultState(EnemyBaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.StateEnter();
        }

        public void RequestStateChange(EnemyBaseState newState)
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