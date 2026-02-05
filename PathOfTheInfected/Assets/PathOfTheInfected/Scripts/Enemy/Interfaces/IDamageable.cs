using UnityEngine;

namespace PathOfTheInfected.Damagable

{
    public struct DamageData
    {
        public int damage;
        public float hitStopTime;
    }

    public interface IDamageable
    {
        public void TakeDamage(DamageData damagedata);
        public void Die();

        public bool IsDead { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
    }
}