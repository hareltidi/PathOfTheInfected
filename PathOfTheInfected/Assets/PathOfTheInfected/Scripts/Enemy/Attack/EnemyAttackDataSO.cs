using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "Enemy/Data/Attack/EnemyAttackDataSO", menuName = "EnemyAttackDataSO", order = 0)]
    public class EnemyAttackDataSO : ScriptableObject
    {
        public int damage = 1;
        public float poiseConsumed = 1.5f;
        public float hitStopTime = 0.5f;
        public float maxAttackRange = 5f;
    }
}
