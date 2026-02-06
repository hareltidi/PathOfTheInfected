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

    public interface IDamageable
    {
        public void TakeDamage(DamageData damageData);
        public void Die();

        public GameObject GameObject { get; set; }

        public bool IsDead { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
    }
}