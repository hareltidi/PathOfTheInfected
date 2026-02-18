using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// This state represents the wandering behavior of a grounded enemy when it has no detected targets. The enemy will move back and forth within a defined range from its initial position, creating a patrolling effect. 
    /// The state manages the wander direction, target positions, and transitions to other states based on target detection and attack range conditions. For our grounded enemies, this state serves as the default behavior when no spottable targets are detected, 
    /// allowing them to patrol their area until a target is spotted or comes within attack range.
    /// </summary>
    /// <remarks>Pseudo state: No spottable detected</remarks>
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

        /// <summary>
        /// Determines whether the enemy has reached its current wander target.
        /// </summary>
        /// <returns>
        /// True if the enemy's position is within the defined threshold of the current wander target;
        /// otherwise, false.
        /// </returns>
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
            if (en|| !trackWanderPath) return;

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