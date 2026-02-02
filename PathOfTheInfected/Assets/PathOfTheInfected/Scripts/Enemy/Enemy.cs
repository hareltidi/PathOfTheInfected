using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable
    {
        #region Interface Variables
        #region IDamageable

        public bool IsDead { get; set; }
       [field: SerializeField] public int CurrentHealth { get; set; }

        public int MaxHealth { get; set; }
        #endregion

        #region IEnemyMoveable

        public Rigidbody2D RB { get; set; }
        public bool IsFacingRight { get; set; } = true;

        #endregion
        #endregion

        #region Damage

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
        }

        public void Die()
        {
            Destroy(gameObject);
        }

        #endregion

        public float moveSpeed = 1f;
        public float acceleration = 1f;

        #region EnemyMovement

        public void MoveEnemy(Vector2 velocity)
        {
           var b = Mathf.Sign(velocity.x) * moveSpeed;
           var t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);
           velocity.x = Mathf.Lerp(velocity.x, b, t);
           if (Mathf.Abs(velocity.x - b) > 0.0099999997764825821) return;
           velocity.x = b;
        }

        public void CheckForLeftOrRightFacing(Vector2 velocity)
        {
            if (IsFacingRight && velocity.x < 0f)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                IsFacingRight = !IsFacingRight;
            }
            else if (!IsFacingRight && velocity.x > 0f)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                IsFacingRight = !IsFacingRight;
            }
        }

        #endregion


    }
}