using PathOfTheInfected.Enemy;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Bosses.BigPillbug
{
    [CreateAssetMenu(fileName = "SummonAttack", menuName = "Boss/Attacks/BigPillbug/SummonAttack")]
    public class SummonAttack : AttackSOBase
    {
        [SerializeField] private GameObject pillbugPrefab;
        [SerializeField] private int summonCount = 3;
        [SerializeField] private float spawnRadius = 2f;

        public override void PerformAttack(AttackContext ctx)
        {
            base.PerformAttack(ctx);

            if (!pillbugPrefab) return;

            for (int i = 0; i < summonCount; i++)
            {
                Vector2 spawnPos = (Vector2)ctx.Owner.Transform.position + Random.insideUnitCircle * spawnRadius;
                // keep spawn above ground if possible
                spawnPos.y = Mathf.Max(spawnPos.y, ctx.Owner.Transform.position.y);

                Instantiate(pillbugPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
