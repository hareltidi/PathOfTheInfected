using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName =  "EnemyStateBase", menuName = "Enemy/States/Core/EnemyStateBase")]
    public class EnemyBaseState : ScriptableObject
    {
       #region protected Fields
       protected EnemyBrainBase EnemyBrainBase;
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
            EnemyBrainBase = enemyBrainBase;
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
