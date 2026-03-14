using PathOfTheInfected.Enemy.Projectiles;

using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "FireProjectileSO", menuName = "Enemy/Attack/Ranged/FireProjectileSO", order = 0)]
    public class FireProjectileSO : AttackSOBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float spawnMargin;
        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            Vector2 dir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left;

            float signedMargin = ctx.Owner.IsFacingRight ? spawnMargin : -spawnMargin;


            Vector2 spawnPos = new Vector2(
                ctx.Owner.transform.position.x + signedMargin,
                ctx.Owner.transform.position.y
            );

            GameObject projectileToSpawn = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            ProjectileBase projectileLogic = projectileToSpawn.GetComponent<ProjectileBase>();
            projectileLogic?.InitProjectileValuesFromAttack(ctx, attackDef, dir, spawnPos);
        }
    }
}