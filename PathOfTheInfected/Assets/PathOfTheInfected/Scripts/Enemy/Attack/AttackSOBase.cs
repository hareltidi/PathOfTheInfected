using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "AttackSOBase", menuName = "Enemy/Attack/AttackSOBase", order = 0)]
    public class AttackSOBase : ScriptableObject
    {
        [field: SerializeField] public float MaxAttackRange { get; protected set; } = 10f;
        [field: SerializeField] public float PoiseConsumed { get; protected set; } = 0.5f;


        /// <summary>
        /// Runs every frame to run attack logic
        /// </summary>
        /// <param name="ctx">The attack context we have on this specific attack</param>
        public virtual void AttackLogic(AttackContext ctx)
        {
            Debug.Log("Attacking");
        }

        /// <summary>
        /// Performs the actual attack
        /// </summary>
        /// <param name="ctx">The attack context we have on this specific attac</param>
        public virtual void PerformAttack(AttackContext ctx)
        {

        }
    }
}
