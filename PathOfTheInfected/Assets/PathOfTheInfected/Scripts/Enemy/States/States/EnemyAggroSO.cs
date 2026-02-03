using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/States/Core/BaseStates", fileName = "EnemyAggroSO", order = 0)]
    public class EnemyAggroSO : SpottableDetectedSOBase
    {
        public Transform target;
        public Vector3 closestPos;

        public override void StateEnter()
        {
            closestPos = _enemy.VisibleSpottables[0].Transform.position;
        }

        public override void StateFixedUpdate()
        {
            foreach (var spottable in _enemy.VisibleSpottables)
            {
                if ((spottable.Transform.position - _enemy.transform.position).sqrMagnitude <
                    (closestPos - _enemy.transform.position).sqrMagnitude)
                {
                    target = spottable.Transform;
                    closestPos = spottable.Transform.position;
                }
            }

            _enemy.MoveTo(target.position);
        }
    }
}