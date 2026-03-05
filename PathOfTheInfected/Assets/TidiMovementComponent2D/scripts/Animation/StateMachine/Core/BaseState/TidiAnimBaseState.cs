using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    /// <summary>
    /// The base state for our animation states.
    /// </summary>
    public abstract class TidiAnimBaseState
    {
        protected TidiAnimStateMachine stateMachine;
        protected TidiAnimInstance animInstance;

        ///<summary>
        ///<para>Called when we are entering a state.</para>
        ///</summary>
        /// <remarks>Override this method to implement any initialization or setup logic that should occur when the state is
        /// entered. This method is typically called by the state machine framework during a state transition.</remarks>
        public abstract void StateEnter();


        /// <summary>
        /// Performs cleanup or transition logic when exiting the current state.
        /// </summary>
        /// <remarks>Override this method to implement any actions that should occur when the state is
        /// exited, such as releasing resources or resetting state-specific data. This method is typically called by the
        /// state machine framework during a state transition.</remarks>
        public abstract void StateExit();

        /// <summary>
        /// The StateUpdate method is called once per frame while the state is active.
        /// </summary>
        /// <remarks>Override this method to implement state-specific behavior that needs to be updated each frame.</remarks>
        public abstract void StateUpdate();

        /// <summary>
        /// Performs per-physics-timestep logic for the current state. Called during the physics update cycle.
        /// </summary>
        /// <remarks>Override this method to implement state-specific behavior that must be updated at a
        /// fixed time interval, such as physics calculations or movement. This method is typically called once per
        /// physics frame by the state machine or controller.</remarks>
        public abstract void StateFixedUpdate();

        /// <summary>
        /// Evaluates and updates the current state animations based on the latest state changes.
        /// </summary>
        /// <remarks>Call this method to ensure that any animations associated with state transitions are
        /// processed and updated. The specific behavior depends on the implementation in a derived class.</remarks>
        public abstract void EvaluateStateAnimations();

        /// <summary>
        /// Performs validation or checks required before transitioning to a new state.
        /// </summary>
        /// <remarks>Override this method in a derived class to implement custom logic that determines
        /// whether a state transition is valid. This method is typically called as part of a state transition process
        /// and may throw exceptions if preconditions are not met.</remarks>
        protected abstract void TransitionChecks();
    }
}
