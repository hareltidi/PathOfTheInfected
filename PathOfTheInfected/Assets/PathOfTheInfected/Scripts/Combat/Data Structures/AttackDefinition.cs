using UnityEngine;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// Represents the definition of an attack within the combat system.
    /// This class is used to define the properties of an attack, such as its base damage,
    /// recovery time, impact behavior, and type. It is a ScriptableObject that can be
    /// created and configured within the Unity Editor.
    /// </summary>
    /// <remarks>
    /// An attack's characteristics are defined using its base damage, type, and specific
    /// timing properties like hit stop and recovery time. These properties determine how
    /// the attack interacts with the game world and characters during combat.
    /// <seealso cref="PathOfTheInfected.Combat.AttackType"/>
    /// </remarks>
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "Attacks/Base/AttackDefinition", order = 0)]
    public class AttackDefinition : ScriptableObject
    {
        public float baseDamage = 1;
        public float hitStopTime;
        public float recoveryTime;
        public AttackType attackType;
    }
}