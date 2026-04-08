using UnityEngine;

namespace PathOfTheInfected.Combat
{
    [Tooltip("Represents the definition of an attack done by the player.")]
    [CreateAssetMenu(fileName = "PlayerAttackDefinition", menuName = "Attacks/PlayerAttackDefinition", order = 0)]
    public class PlayerAttackDefinition : AttackDefinition
    {
        public float attackBufferTime = 0;
        public float attackRange;
        public float firstHitDamageBoost;
    }
}