using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/States/EnemyAggroSO", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : EnemyBaseState
    {
        public Transform target;
        private float _bestDistSq = float.MaxValue;
        private Vector2 _enemyPos = Vector2.zero;

        public override void StateEnter()
        {
            base.StateEnter();
            target = null;
        }

        public override void StateFixedUpdate()
        {
            if (!target) return;
            _enemy.MoveTo(FindClosestTarget());
        }

        private Transform FindClosestTarget()
        {
            _enemyPos = _enemy.transform.position;

            foreach (var spottable in _enemy.VisibleSpottables)
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
            if (_enemy.isSpottableInAttackRange)
            {
                _stateMachine?.RequestStateChange(_enemy.spottableInAttackRangeState);
            }

            if (!_enemy.isSpottableDetected)
            {
                _stateMachine?.RequestStateChange(_enemy.noSpottableDetectedState);
            }
        }
    }
}