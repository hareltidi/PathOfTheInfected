using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyStats - base", menuName = "Base enemy stats", order = 0)]
    public class EnemyStatsSOBase : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 1f;
        public float acceleration = 1f;
    }
}