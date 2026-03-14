using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// The base class for all enemy states. This class provides the structure and common functionality for all specific enemy states, such as idle, patrol, chase, attack, etc. 
    /// Each specific state will inherit from this base class and implement its own behavior while utilizing the shared functionality provided here.
    /// </summary>
    /// <remarks> Every state that is derived from this base state can be divided into 3 main and general pseudo states:
    /// No spottable detected state: This is the state where the enemy has not detected any spottable targets. In this state, 
    /// the enemy might be idle, patrolling, or performing some other behavior that does not involve actively pursuing a target.
    /// Spottable detected state: This is the state where the enemy has detected a spottable target. In this state, the enemy will typically move towards the target and prepare to attack.
    /// Spottable in attack range state: This is the state where the enemy is within range to attack a detected target. In this state, the enemy will execute its attack behavior.
    /// </remarks>
    [CreateAssetMenu(fileName =  "EnemyStateBase", menuName = "Enemy/States/Core/EnemyStateBase")]
    public class EnemyBaseState : ScriptableObject
    {
       #region protected Fields
       /// <summary>
       /// The enemy that owns this state
       /// </summary>
       protected EnemyBrainBase CurrentEnemyBrain;
       /// <summary>
       /// The state machine that manages this state
       /// </summary>
       protected EnemyStateMachine StateMachine;
       #endregion

        #region Virtual Methods
        /// <summary>
        /// Sets initial variables for states to function properly
        /// </summary>
        /// <param name="enemyBrainBase">the owner of the state</param>
        /// <param name="stateMachine">the state machine responsible for the state</param>
        public virtual void StateInit(EnemyBrainBase enemyBrainBase, EnemyStateMachine stateMachine)
        {
            CurrentEnemyBrain = enemyBrainBase;
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Called at a start of each state - Like the Start function on Monobehaviors
        /// </summary>
        public virtual void StateEnter()
        {

        }

        /// <summary>
        /// Called at the end of each state - Like the OnDestroy or on disable functions on Monobehaviors
        /// </summary>
        public virtual void StateExit()
        {

        }

        /// <summary>
        /// Called in each frame this state is active - Like the Update function on Monobehaviors
        /// </summary>
        public virtual void StateUpdate()
        {
            TransitionChecks();
        }

        /// <summary>
        /// Called in each physics step this state is active - like the FixedUpdate function on Monobehaviors
        /// </summary>
        public virtual void StateFixedUpdate()
        {

        }

        /// <summary>
        /// method for handling state transitions
        /// </summary>
        public virtual void TransitionChecks()
        {

        }

        /// <summary>
        /// draws gizmos when we select our enemy in the inspector. (if gizmos are enabled)
        /// </summary>
        /// <param name="en">enemy that called this</param>
        public virtual void DrawGizmosOnSelected(EnemyBrainBase en)
        {

        }

        #endregion
    }
}
