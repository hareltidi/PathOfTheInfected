using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyWanderState", menuName = "Enemy/States/EnemyWanderState", order = 0)]
    public class EnemyWanderSo : NoSpottableDetectedStateSOBase
    {
        [field: SerializeField] public float WanderRange { get; protected set; } = 15f;
        public Vector2 WanderDirection { get; protected set; }
        public Vector2 WanderMaxPosition { get; protected set; }
        public Vector2 WanderMinPosition { get; protected set; }
        public Vector2 CurrentWanderTarget { get; protected set; }
        public float threshold = 0.5f;
        public bool trackWanderPath = true;

        public override void StateEnter()
        {
            Vector2 origin = _enemy.transform.position;

            WanderDirection = _enemy.IsFacingRight ? Vector2.right : Vector2.left;
            WanderMaxPosition = origin + WanderDirection * WanderRange;
            WanderMinPosition = origin - WanderDirection * WanderRange;
            CurrentWanderTarget = WanderMaxPosition;
        }

        public override void StateFixedUpdate()
        {
            CalculateEnemyMovement();
            float dx = CurrentWanderTarget.x - _enemy.transform.position.x;
            if (Mathf.Abs(dx) <= threshold)
            {
                CurrentWanderTarget = (CurrentWanderTarget == WanderMaxPosition) ? WanderMinPosition : WanderMaxPosition;
            }
        }

        private void CalculateEnemyMovement()
        {
            float dx = CurrentWanderTarget.x - _enemy.transform.position.x;
            float dir = Mathf.Sign(dx);
            float desiredVx = dir * _enemy.moveSpeed;
            Vector2 vel = new Vector2(desiredVx, 0f);
            _enemy.MoveEnemy(vel);
        }

        public override void DrawGizmosOnSelected(Enemy en)
        {
            if (en == null || !trackWanderPath) return;

            Vector3 origin = en.transform.position;
            Vector3 dir = en.IsFacingRight ? Vector3.right : Vector3.left;

            Vector3 max = origin + dir * WanderRange;
            Vector3 min = origin - dir * WanderRange;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(min, max);
        }
    }
}