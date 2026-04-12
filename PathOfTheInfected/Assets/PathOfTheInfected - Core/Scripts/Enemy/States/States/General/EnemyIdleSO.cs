using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyIdleSO", menuName = "Enemy/States/General/EnemyIdleSO", order = 0)]
    public class EnemyIdleSO : EnemyBaseState
    {
        public override void StateEnter()
        {
            base.StateEnter();
            CurrentEnemyBrain.MoveEnemy(Vector2.zero);
        }
    }
}