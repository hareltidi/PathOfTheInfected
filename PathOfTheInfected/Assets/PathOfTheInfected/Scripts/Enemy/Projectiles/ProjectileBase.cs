using System;
using PathOfTheInfected.Combat;
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
        protected AttackDefinition AttackDefinition;
        protected AttackContext Context;
        protected Rigidbody2D rb;
        protected Vector2 direction;

        public virtual void InitProjectileValuesFromAttack(AttackContext ctx,AttackDefinition definition, Vector2 dir, Vector3 position)
        {
            rb = GetComponent<Rigidbody2D>();
            Context = ctx;
            AttackDefinition = definition;
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
            IHitResponder test = other.gameObject.GetComponent<IHitResponder>();

            if (test != null)
            {
                HitData data = new HitData()
                {
                    source = gameObject,
                    target = other.gameObject,
                    comboDamageScalingLevel = 1,
                    firstHitDamageBoost = 0,
                    isFirstHit = false,
                    isPlayerDamage = false,
                    isAttackerInAir = false,
                    timeStamp = Time.timeSinceLevelLoad,
                    attackDefinition = AttackDefinition
                };
                if (other.gameObject == data.source) return; // if the bullet hits the one who shot, ignore it

                HitDispatcher.ProcessHit(data);

                Destroy(gameObject);
            }
        }
    }
}