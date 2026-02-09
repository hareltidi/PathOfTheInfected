using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Damagable

{
    public struct DamageData
    {
        public int Damage;
        public float HitStopTime;
        public EnemyBrainBase Instigator;
        public IDamageable DamagedObject;
    }

    /// <summary>
    /// Interface for objects that can be damaged.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage to the object implementing this method (As part of <see cref="IDamageable"/> interface).
        /// </summary>
        /// <param name="damageData">A struct containing damage details, including the damage amount, hit stop time,
        /// the entity responsible for causing the damage, and the object being damaged.</param>
        public void TakeDamage(DamageData damageData);

        /// <summary>
        /// Handles the death process of the object implementing this method.
        /// Marks the object as dead and removes it from the scene or game world.
        /// (as part of <see cref="IDamageable"/> interface)
        /// </summary>
        public void Die();

        /// <summary>
        /// The game object that implements the <see cref="IDamageable"/> interface.
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// Is the object who implements the <see cref="IDamageable"/> interface dead?
        /// </summary>
        public bool IsDead { get; set; }
        /// <summary>
        /// The current health of the object implementing the <see cref="IDamageable"/> interface.
        /// </summary>
        public int CurrentHealth { get; set; }
        /// <summary>
        /// The maximum health of the object implementing the <see cref="IDamageable"/> interface.
        /// </summary>
        public int MaxHealth { get; set; }
    }
}