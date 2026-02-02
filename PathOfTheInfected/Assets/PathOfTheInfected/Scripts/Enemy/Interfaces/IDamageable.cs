namespace PathOfTheInfected.Enemy
{
    public interface IDamageable
    {
        public void TakeDamage(int damage);
        public void Die();

        public bool IsDead { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
    }
}