using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/States/EnemyAggroSO", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : EnemyBaseState
    {
        public Transform target;
        private float _bestDistSq = 0;
        private Vector2 _enemyPos = Vector2.zero;

        public override void StateEnter()
        {
            base.StateEnter();
            _bestDistSq = float.MaxValue;
            _enemyPos = Vector2.zero;
        }

        public override void StateFixedUpdate()
        {
            EnemyBrainBase.MoveTo(FindClosestTarget());
        }

        private Transform FindClosestTarget()
        {
            _enemyPos = EnemyBrainBase.transform.position;

            foreach (var spottable in EnemyBrainBase.VisibleSpottables)
            {
                float distSq = (spottable.Transform.position - (Vector3)_enemyPos).sqrMagnitude;
                if (distSq < _bestDistSq)
                {
                    _bestDistSq = distSq;
                    target = spottable.Transform;
                }
            }
            return target;
        }

        public override void TransitionChecks()
        {
            base.TransitionChecks();
            if (EnemyBrainBase.isSpottableInAttackRange)
            {
                _stateMachine?.RequestStateChange(EnemyBrainBase.spottableInAttackRangeState);
            }

            if (!EnemyBrainBase.isSpottableDetected)
            {
                _stateMachine?.RequestStateChange(EnemyBrainBase.noSpottableDetectedState);
            }
        }
    }
}