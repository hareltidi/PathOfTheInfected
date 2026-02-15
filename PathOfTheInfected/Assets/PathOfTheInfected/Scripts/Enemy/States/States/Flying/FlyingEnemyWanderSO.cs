using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "FlyingEnemyWanderSO", menuName = "Enemy/States/Flying/FlyingEnemyWanderSO", order = 0)]
    public class FlyingEnemyWanderSO : EnemyBaseState
    {
        [field: SerializeField] public float WanderRadius { get; protected set; } = 15f;
        [SerializeField] private float reachThreshold = 0.5f;
        public Vector2 CurrentWanderTarget { get; protected set; }
        public float wallCheckRayRadius = 0.5f;
        public bool trackWanderPath = true;
        public float investigationDuration = 1f;
        public float investigationRadius = 8f;
        private float _investigationTimer;
        private bool _isInvestigating;

        public override void StateEnter()
        {
            if (CurrentEnemyBrain.HasLastKnownTarget)
            {
                CurrentWanderTarget = CurrentEnemyBrain.LastKnownTargetPosition;
            }
            else
            {
                CurrentWanderTarget = GetNextWanderTarget();
            }
        }

        public override void StateFixedUpdate()
        {
            CalculateEnemyMovement();
            EnemyWallCheck();
            if (ReachedTarget())
            {
                if (CurrentEnemyBrain.HasLastKnownTarget)
                {
                    CurrentEnemyBrain.HasLastKnownTarget = false;
                    _isInvestigating = true;
                }

                CurrentWanderTarget = GetNextWanderTarget();
            }
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
            if (_isInvestigating)
            {
                _investigationTimer += Time.deltaTime;
            }

            if (_investigationTimer >= investigationDuration)
            {
                _investigationTimer = 0;
                _isInvestigating = false;
            }
        }

        private Vector2 GenerateInvestigationPoint()
        {
            Vector2 center = CurrentEnemyBrain.LastKnownTargetPosition;
            Vector2 offset = Random.insideUnitCircle * investigationRadius;
            return center + offset;
        }


        private void CalculateEnemyMovement()
        {
            CurrentEnemyBrain.MoveTo(CurrentWanderTarget);
        }

        public override void DrawGizmosOnSelected(EnemyBrainBase en)
        {
            if (!en || !trackWanderPath) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(en.InitialPosition, WanderRadius);
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

        #region Enemy target calculation

        private Vector2 CalculateWanderTarget()
        {
            Vector2 origin = CurrentEnemyBrain.InitialPosition;
            Vector2 circle = Random.insideUnitCircle * WanderRadius;
            return circle + origin;
        }

        private bool CheckForWalls(Vector2 target, float radius = 0.5f)
        {
            var hits = Physics2D.OverlapCircleAll(target, radius, LayerMask.GetMask("ground"));
            return hits.Length > 0;
        }

        private Vector2 GetNextWanderTarget()
        {
            if (_isInvestigating)
            {
                return GenerateInvestigationPoint();
            }

            Vector2 target;
            do
            {
                target = CalculateWanderTarget();
            } while (CheckForWalls(target));

            return target;
        }

        private void EnemyWallCheck()
        {
            Vector2 pos = CurrentEnemyBrain.transform.position;
            Vector2 dir = (CurrentWanderTarget - pos).normalized;

            float checkDistance = wallCheckRayRadius + 0.2f;

            RaycastHit2D hit = Physics2D.CircleCast(
                pos,
                wallCheckRayRadius,
                dir,
                checkDistance,
                LayerMask.GetMask("ground")
            );

            if (hit.collider)
            {
                CurrentWanderTarget = GetNextWanderTarget();
            }
        }

        private bool ReachedTarget()
        {
            return Vector2.Distance(
                CurrentEnemyBrain.transform.position,
                CurrentWanderTarget
            ) < reachThreshold;
        }

        #endregion
    }
}