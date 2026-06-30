using UnityEngine;

namespace PathOfTheInfected.Core.Scripts.Boss.States
{
    [CreateAssetMenu(fileName =  "BossStateBase", menuName = "Boss/States/Core/BossStateBase")]
    public class BossState : ScriptableObject
    {
        #region protected Fields
        /// <summary>
        /// The boss that owns this state
        /// </summary>
        protected BossBrain CurrentBossBrain;
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Sets initial variables for states to function properly
        /// </summary>
        /// <param name="enemyBrainBase">the owner of the state</param>
        public virtual void StateInit(BossBrain enemyBrainBase)
        {
            CurrentBossBrain = enemyBrainBase;
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
        }

        /// <summary>
        /// Called in each physics step this state is active - like the FixedUpdate function on Monobehaviors
        /// </summary>
        public virtual void StateFixedUpdate()
        {

        }
        #endregion
    }
}