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

        [Header("Stuck Detection")]
        [SerializeField, Tooltip("If the enemy doesn't make progress toward its wander target for this many seconds, pick a new target.")]
        private float stuckTimeout = 1.0f;

        private float _stuckTimer;
        private float _lastDistanceToTarget = float.MaxValue;

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

            _stuckTimer = 0f;
            _lastDistanceToTarget = Vector2.Distance(CurrentEnemyBrain.transform.position, CurrentWanderTarget);
        }

        public override void StateFixedUpdate()
        {
            CalculateEnemyMovement();

            if (Time.timeSinceLevelLoad < _nextAllowedSwitchTime) return;
            // Stuck detection: if we aren't getting closer to our target for a while, force a target switch
            float dist = Vector2.Distance(CurrentEnemyBrain.transform.position, CurrentWanderTarget);
            if (dist + 0.01f < _lastDistanceToTarget)
            {
                // made progress
                _stuckTimer = 0f;
            }
            else
            {
                _stuckTimer += Time.fixedDeltaTime;
            }
            _lastDistanceToTarget = dist;

            if (_stuckTimer >= stuckTimeout)
            {
                // Force a switch to avoid getting stuck
                _stuckTimer = 0f;
                _goingToMax = !_goingToMax;
                CurrentWanderTarget = _goingToMax ? WanderMaxPosition : WanderMinPosition;
                _nextAllowedSwitchTime = Time.timeSinceLevelLoad + switchCooldown;
                return;
            }

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

        /// <summary>
        /// Determines whether the enemy has reached or passed the current target position
        /// based on its movement direction and the defined threshold.
        /// </summary>
        /// <returns>True if the enemy has reached or passed the target position; otherwise, false.</returns>
        private bool HasReachedOrPassedTarget()
        {
            float x = CurrentEnemyBrain.transform.position.x;

            if (_goingToMax)
            {
                return x >= (WanderMaxPosition.x - threshold);
            }

            return x <= (WanderMinPosition.x + threshold);
        }

        /// <summary>
        /// Calculates and updates the enemy's movement logic during its wander state.
        /// Moves the enemy towards the currently assigned wander target using the
        /// associated enemy brain's movement behavior.
        /// </summary>
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

            if (CurrentEnemyBrain.isEnemyDamaged)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.damagedState);
            }
        }
    }
}