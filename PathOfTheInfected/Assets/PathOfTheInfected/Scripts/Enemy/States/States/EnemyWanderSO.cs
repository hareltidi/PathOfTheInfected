using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyWanderState", menuName = "Enemy/States/EnemyWanderState", order = 0)]
    public class EnemyWanderSO : NoSpottableDetectedStateSOBase
    {
        public float wanderRange = 15f;
        public Vector2 wanderDirection = Vector2.right;
        public Vector2 wanderTargetPosition;

        public override void StateEnter()
        {
            wanderTargetPosition = wanderDirection *  wanderRange;
        }
    }
}