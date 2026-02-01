using System; 
using UnityEngine;

namespace TidiMovementComponent2D.MovingPlatforms
{
    [DefaultExecutionOrder(-101)]
    public class ConveyorBelt : MonoBehaviour, IVelocityInheritable
    {
        [Header("Configuration")]
        [SerializeField]
        private float _moveSpeed = 1f;
        [SerializeField]
        private bool _launchOnExit = true;
        [SerializeField]
        private bool _checkForWalls;
        [Header("Debug")]
        [SerializeField]
        private bool _showGizmos = true;

        public Vector2 GetVelocity() => (Vector2)(transform.right * _moveSpeed);

        public bool ImpartMomentumOnExit { get; set; } = true;

        public bool ProbesShouldLead { get; set; }

        public bool NeedsFuturePositionBoxcastCheck => _checkForWalls;

        public bool LaunchVerticallyOnExit => _launchOnExit;

        public Transform Transform
        {
            get
            {
                return transform;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos)
                return;
            Gizmos.color = Color.yellow;
            Vector3 from1 = (bool)(UnityEngine.Object)GetComponent<Collider2D>() ? GetComponent<Collider2D>().bounds.center : transform.position;
            Vector3 direction = transform.right * _moveSpeed;
            Gizmos.DrawRay(from1, direction);
            if ((double)_moveSpeed == 0.0)
                return;
            Vector3 from2 = from1 + direction;
            Vector3 vector3_1 = Quaternion.Euler(0.0f, 0.0f, 160f) * transform.right;
            Vector3 vector3_2 = Quaternion.Euler(0.0f, 0.0f, -160f) * transform.right;
            Gizmos.DrawRay(from2, vector3_1 * 0.5f);
            Gizmos.DrawRay(from2, vector3_2 * 0.5f);
        }
    }
}
