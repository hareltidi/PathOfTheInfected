using UnityEngine;

namespace PathOfTheInfected.Combat
{
    [Tooltip("Represents the definition of an attack done by the player.")]
    [CreateAssetMenu(fileName = "PlayerAttackDefinition", menuName = "Attacks/PlayerAttackDefinition", order = 0)]
    public class PlayerAttackDefinition : AttackDefinition
    {
        public float attackBufferTime = 0;
        public float startupTime = 0f;
        public float activeTime = 0.1f;
        [Tooltip("Extra seconds added after activeTime so the timeline can match visual clip tail/crossfade.")]
        public float animationEndPadding = 0f;
        public float attackRange;
        public float firstHitDamageBoost;
    }
}