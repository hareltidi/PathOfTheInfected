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

        private bool _goingToMax;

        [Header("Stability")]
        [SerializeField, Tooltip("Prevents flip-flip when we arrive at a boundary and physics overshoots a bit.")]
        private float switchCooldown = 0.1f;

        private float _nextAllowedSwitchTime;

        public override void StateEnter()
        {
            Vector2 origin = CurrentEnemyBrain.InitialPosition;

            // Grounded wander is horizontal: define true min/max X regardless of current facing.
            WanderMinPosition = new Vector2(origin.x - WanderRange, origin.y);
            WanderMaxPosition = new Vector2(origin.x + WanderRange, origin.y);

            _nextAllowedSwitchTime = 0f;

            if (CurrentEnemyBrain.HasLastKnownTarget)
            {
                CurrentWanderTarget = CurrentEnemyBrain.LastKnownTargetPosition;
                _goingToMax = CurrentWanderTarget.x >= origin.x;
            }
            else
            {
                // Start by walking in the direction we're facing (purely cosmetic/personality).
                _goingToMax = CurrentEnemyBrain.IsFacingRight;
                CurrentWanderTarget = _goingToMax ? WanderMaxPosition : WanderMinPosition;
            }
        }

        public override void StateFixedUpdate()
        {
            CalculateEnemyMovement();

            if (Time.timeSinceLevelLoad < _nextAllowedSwitchTime) return;
            if (HasReachedOrPassedTarget())
            {
                if (CurrentEnemyBrain.HasLastKnownTarget)
                {
                    CurrentEnemyBrain.HasLastKnownTarget = false;
                }

                _goingToMax = !_goingToMax;
                CurrentWanderTarget = _goingToMax ? WanderMaxPosition : WanderMinPosition;

                _nextAllowedSwitchTime = Time.timeSinceLevelLoad + switchCooldown;
            }
        }

        private bool HasReachedOrPassedTarget()
        {
            float x = CurrentEnemyBrain.transform.position.x;

            if (_goingToMax)
            {
                return x >= (WanderMaxPosition.x - threshold);
            }

            return x <= (WanderMinPosition.x + threshold);
        }

        private void CalculateEnemyMovement()
        {
            CurrentEnemyBrain.MoveTo(CurrentWanderTarget);
        }

        public override void DrawGizmosOnSelected(EnemyBrainBase en)
        {
            if (!en || !trackWanderPath) return;

            Vector3 origin = en.InitialPosition;
            Vector3 max = new Vector3(origin.x + WanderRange, origin.y, origin.z);
            Vector3 min = new Vector3(origin.x - WanderRange, origin.y, origin.z);

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