using System;
using PathOfTheInfected.Damagable;
using TidiGenericObjectPooling;
using UnityEngine;

namespace PathOfTheInfected.Enemy.Projectiles
{
    public class ProjectileBase : MonoBehaviour
    {
        [SerializeField] protected float projectileSpeed;
        [SerializeField] protected float projectileLifeTime = 2f;
        protected float lifetimeTimer = 0;
        protected DamageData Data;
        protected AttackContext Context;
        protected Rigidbody2D rb;
        protected Vector2 direction;

        public virtual void InitProjectileValuesFromAttack(AttackContext ctx,DamageData data, Vector2 dir, Vector3 position)
        {
            rb = GetComponent<Rigidbody2D>();
            Context = ctx;
            Data = data;
            direction = dir;
            transform.position = position;
        }

        private void FixedUpdate()
        {
            if (rb)
            {
                rb.MovePosition(rb.position + direction * (projectileSpeed * Time.fixedDeltaTime));
            }
        }

        private void Update()
        {
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= projectileLifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject == Data.Instigator.gameObject) return;


            IDamageable test = other.gameObject.GetComponent<IDamageable>();

            if (test != null)
            {
                Data.DamagedObject = test;

                test.TakeDamage(Data);
                Destroy(gameObject);
            }
        }
    }
}