using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(fileName = "FlyingEnemyWanderSO", menuName = "Enemy/States/Flying/FlyingEnemyWanderSO", order = 0)]
    public class FlyingEnemyWanderSO : EnemyBaseState
    {
        [field: SerializeField] public float WanderRadius { get; protected set; } = 15f;
        public Vector2 CurrentWanderTarget { get; protected set; }
        public float wallCheckRayRadius = 0.5f;
        public bool trackWanderPath = true;
        public override void StateEnter()
        {
            if (EnemyBrainBase.HasLastKnownTarget)
            {
                CurrentWanderTarget = EnemyBrainBase.LastKnownTargetPosition;
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
                if (EnemyBrainBase.HasLastKnownTarget)
                {
                    EnemyBrainBase.HasLastKnownTarget = false;
                }

                CurrentWanderTarget = GetNextWanderTarget();
            }
        }

        private void CalculateEnemyMovement()
        {
            Vector2 pos = EnemyBrainBase.transform.position;
            Vector2 dir = (CurrentWanderTarget - pos).normalized;

            EnemyBrainBase.MoveEnemy(dir);
        }

        public override void DrawGizmosOnSelected(EnemyBrainBase en)
        {
            if (en == null || !trackWanderPath) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(en.InitialPosition, WanderRadius);
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (EnemyBrainBase.isSpottableDetected)
            {
                StateMachine.RequestStateChange(EnemyBrainBase.spottableDetectedState);

            }

            if (EnemyBrainBase.isSpottableInAttackRange)
            {
                StateMachine.RequestStateChange(EnemyBrainBase.spottableInAttackRangeState);
            }
        }

        #region Enemy target calculation

        private Vector2 CalculateWanderTarget()
        {
            Vector2 origin = EnemyBrainBase.InitialPosition;
            Vector2 circle = Random.insideUnitCircle * WanderRadius;
            return circle + origin;
        }

        private bool CheckForWalls(Vector2 target, float radius = 0.5f)
        {
           var hits =  Physics2D.OverlapCircleAll(target, radius, LayerMask.GetMask("ground"));
           return hits.Length > 0;
        }

        private Vector2 GetNextWanderTarget()
        {
            Vector2 target;
            do
            {
                target = CalculateWanderTarget();
            } while (CheckForWalls(target));
            return target;
        }

        private void EnemyWallCheck()
        {
            Vector2 pos = EnemyBrainBase.transform.position;
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
                EnemyBrainBase.transform.position,
                CurrentWanderTarget
            ) < 0.3f;
        }
        #endregion
    }
}