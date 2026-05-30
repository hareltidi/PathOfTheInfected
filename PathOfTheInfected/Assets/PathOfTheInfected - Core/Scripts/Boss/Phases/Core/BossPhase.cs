using System.Collections.Generic;
using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Core.Scripts.Boss

{


    public enum AttackSelectionMode : uint
    {
        Random = 0,
        Sequence = 1,
    }

    [CreateAssetMenu(fileName = "BossPhase", menuName = "Boss/Phases/Core/BossPhase")]
    public class BossPhase : ScriptableObject
    {
        [field: SerializeField] public List<AttackSOBase> AllowedAttacks { get; private set; }
        public AttackSelectionMode attackSelectionMode = AttackSelectionMode.Random;
        public float transitionHealthPercentThreshold = 0.5f;
        [Range(0f, 1f)]
        public float aggressionWeight;

        #region protected Fields
       /// <summary>
       /// The boss that owns this state
       /// </summary>
       protected BossBrain CurrentBossBrain;
       #endregion

        #region Virtual Methods
        /// <summary>
        /// Sets initial variables for phase to function properly
        /// </summary>
        /// <param name="bossBrainBase">the owner of the phase</param>
        public virtual void PhaseInit(BossBrain bossBrainBase)
        {
            CurrentBossBrain = bossBrainBase;
        }

        /// <summary>
        /// Called at a start of each phase - Like the Start function on Monobehaviors
        /// </summary>
        public virtual void PhaseEnter()
        {

        }

        /// <summary>
        /// Called at the end of each phase - Like the OnDestroy or on disable functions on Monobehaviors
        /// </summary>
        public virtual void PhaseExit()
        {

        }

        /// <summary>
        /// Called in each frame this phase is active - Like the Update function on Monobehaviors
        /// </summary>
        public virtual void PhaseUpdate()
        {
            ShouldTransition();
        }

        /// <summary>
        /// Called in each physics step this phase is active - like the FixedUpdate function on Monobehaviors
        /// </summary>
        public virtual void PhaseFixedUpdate()
        {

        }


        public virtual bool ShouldTransition()
        {
            float healthNormalized = CurrentBossBrain.Health.CurrentHealth / CurrentBossBrain.Health.MaxHealth;
            return healthNormalized <= transitionHealthPercentThreshold;
        }

        /// <summary>
        /// draws gizmos when we select our enemy in the inspector. (if gizmos are enabled)
        /// </summary>
        /// <param name="en">enemy that called this</param>
        public virtual void DrawGizmosOnSelected(BossBrain en)
        {

        }

        #endregion
    }

}