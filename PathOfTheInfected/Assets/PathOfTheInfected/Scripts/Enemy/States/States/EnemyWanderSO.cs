using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "EnemyWanderState", menuName = "Enemy/States/Grounded/EnemyWanderState", order = 0)]
    public class EnemyWanderSo : EnemyBaseState
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
            Vector2 origin = CurrentEnemyBrain.InitialPosition;

            WanderDirection = CurrentEnemyBrain.IsFacingRight ? Vector2.right : Vector2.left;
            WanderMaxPosition = origin + WanderDirection * WanderRange;
            WanderMinPosition = origin - WanderDirection * WanderRange;
            if (CurrentEnemyBrain.HasLastKnownTarget)
            {
                CurrentWanderTarget = CurrentEnemyBrain.LastKnownTargetPosition;
            }
            else
            {
                CurrentWanderTarget = WanderMaxPosition;
            }
        }

        public override void StateFixedUpdate()
        {
            CalculateEnemyMovement();
            if (HasReachedTarget())
            {
                if (CurrentEnemyBrain.HasLastKnownTarget)
                {
                    CurrentEnemyBrain.HasLastKnownTarget = false;
                }

                CurrentWanderTarget = (CurrentWanderTarget == WanderMaxPosition) ? WanderMinPosition : WanderMaxPosition;
            }
        }

        private bool HasReachedTarget()
        {
            float dx = CurrentWanderTarget.x - CurrentEnemyBrain.transform.position.x;
            return Mathf.Abs(dx) <= threshold;
        }

        private void CalculateEnemyMovement()
        {
           CurrentEnemyBrain.MoveTo(CurrentWanderTarget);
        }

        public override void DrawGizmosOnSelected(EnemyBrainBase en)
        {
            if (en == null || !trackWanderPath) return;

            Vector3 origin = en.InitialPosition;
            Vector3 dir = en.IsFacingRight ? Vector3.right : Vector3.left;

            Vector3 max = origin + dir * WanderRange;
            Vector3 min = origin - dir * WanderRange;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(min, max);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.spottableDetectedState);

            }

            if (CurrentEnemyBrain.isSpottableInAttackRange)
            {
                StateMachine.RequestStateChange(CurrentEnemyBrain.spottableInAttackRangeState);
            }
        }
    }
}