using PathOfTheInfected.Enemy.Projectiles;

using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "FireProjectileSO", menuName = "Enemy/CurrentAttack/Ranged/FireProjectileSO", order = 0)]
    public class FireProjectileSO : AttackSOBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float spawnMarginX = 5;
        [SerializeField] private float spawnMarginY = 5;
        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            Vector2 dir = ctx.Owner.IsFacingRight ? Vector2.right : Vector2.left;

            float signedMarginX = ctx.Owner.IsFacingRight ? spawnMarginX : -spawnMarginX;
            float signedMarginY = spawnMarginY;


            Vector2 spawnPos = new Vector2(
                ctx.Owner.Transform.position.x + signedMarginX,
                ctx.Owner.Transform.position.y + signedMarginY
            );

            GameObject projectileToSpawn = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            ProjectileBase projectileLogic = projectileToSpawn.GetComponent<ProjectileBase>();
            projectileLogic?.InitProjectileValuesFromAttack(ctx, AttackDef, dir, spawnPos, ctx.Owner.GameObject);
        }
    }
}