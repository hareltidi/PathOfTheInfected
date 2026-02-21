using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Represents the aggressive state for a grounded enemy, managing target acquisition and movement toward detected
    /// targets.
    /// </summary>
    /// <remarks>This state is typically used when an enemy has detected a target and is actively pursuing it.
    /// The state handles updating the target and transitioning to other states based on detection and attack range
    /// conditions. Use this state within an enemy state machine to enable aggressive pursuit behavior.</remarks>
    /// <remarks>Pseudo state: Spottable detected</remarks>
    [CreateAssetMenu(menuName = "Enemy/States/Grounded/EnemyAggroSO", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : EnemyBaseState
    {
        public Transform Target { get; private set; }

        [SerializeField] private float aggroDelay = 0.5f;
        private float _aggroTimer;

        [Header("Personality")]
        [SerializeField] private bool useOffsetPursuit = true;
        [SerializeField] private float offsetRadiusMin = 2f;
        [SerializeField] private float offsetRadiusMax = 4f;
        [SerializeField] private float offsetRefreshInterval = 0.6f;

        private float _offsetTimer;
        private Vector2 _currentOffsetTarget;

        public override void StateEnter()
        {
            base.StateEnter();
            _aggroTimer = 0f;
            _offsetTimer = offsetRefreshInterval; // force an immediate offset pick
            Target = null;
            _currentOffsetTarget = (Vector2)CurrentEnemyBrain.transform.position; // safe fallback
        }

        public override void StateFixedUpdate()
        {
            if (_aggroTimer < aggroDelay)
            {
                _aggroTimer += Time.fixedDeltaTime;
                return;
            }

            if (!Target)
            {
                Target = CurrentEnemyBrain.ClosestTarget;
            }

            if (!Target)
            {
                if (CurrentEnemyBrain.HasLastKnownTarget)
                    CurrentEnemyBrain.MoveTo(CurrentEnemyBrain.LastKnownTargetPosition);
                return;
            }

            if (!useOffsetPursuit)
            {
                CurrentEnemyBrain.MoveTo(Target);
                return;
            }

            _offsetTimer += Time.fixedDeltaTime;
            if (_offsetTimer >= offsetRefreshInterval)
            {
                _offsetTimer = 0f;

                float r = Random.Range(offsetRadiusMin, offsetRadiusMax);

                Vector2 dir = Random.insideUnitCircle;
                if (dir.sqrMagnitude < 0.0001f)
                {
                    dir = Vector2.right;
                }

                dir.Normalize();
                _currentOffsetTarget = (Vector2)Target.position + dir * r;
            }

            CurrentEnemyBrain.MoveTo(_currentOffsetTarget);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();

            if (CurrentEnemyBrain.isSpottableInAttackRange)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.spottableInAttackRangeState);
            }

            if (!CurrentEnemyBrain.isSpottableDetected)
            {
                StateMachine?.RequestStateChange(CurrentEnemyBrain.noSpottableDetectedState);
            }
        }
    }
}