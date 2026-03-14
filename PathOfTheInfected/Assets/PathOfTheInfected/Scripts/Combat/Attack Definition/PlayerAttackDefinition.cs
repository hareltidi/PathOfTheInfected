using UnityEngine;

namespace PathOfTheInfected.Combat
{
    [CreateAssetMenu(fileName = "PlayerAttackDefinition", menuName = "Attacks/PlayerAttackDefinition", order = 0)]
    public class PlayerAttackDefinition : AttackDefinition
    {
        public float attackBufferTime = 0;
        public float attackRange;
        public float firstHitDamageBoost;
    }
}