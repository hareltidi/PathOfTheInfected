using UnityEngine;

namespace TidiMovementComponent2D.MovingPlatforms
{
    public class MovingPlatform : MonoBehaviour, IVelocityInheritable
    {
        [Header("Configuration")]
        [SerializeField]
        private Vector3 _moveOffset = new Vector3(0.0f, 3f, 0.0f);
        [SerializeField]
        private float _duration = 2f;
        [SerializeField]
        private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1f, 1f);
        [SerializeField]
        private bool _loop = true;
        [SerializeField]
        private bool _checkForWalls;
        [Header("Collision Logic")]
        [SerializeField]
        private bool _pushPassenger = true;
        [SerializeField]
        private LayerMask _passengerMask;
        [SerializeField]
        private float _pushCollisionPadding = 0.015f;
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _timer;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private VisualInterpolator _visuals;

        public bool ProbesShouldLead { get; set; } = true;

        public bool ImpartMomentumOnExit { get; set; } = true;

        public bool LaunchVerticallyOnExit { get; }

        public Vector3 PositionDelta { get; private set; }

        public bool NeedsFuturePositionBoxcastCheck => _checkForWalls;

        public Transform Transform => transform;

        public Vector2 GetVelocity() => PositionDelta / Time.fixedDeltaTime;

        private void Awake()
        {
            _visuals = GetComponentInChildren<VisualInterpolator>();
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _collider = GetComponent<BoxCollider2D>();
            _startPosition = _rb.position;
            _endPosition = _startPosition + _moveOffset;
        }

        private void FixedUpdate()
        {
            Vector2 position = _rb.position;
            _timer += Time.fixedDeltaTime;
            Vector2 vector2 = Vector2.Lerp(_startPosition, _endPosition, _easeCurve.Evaluate(_loop ? Mathf.PingPong(_timer / _duration, 1f) : Mathf.Clamp01(_timer / _duration)));
            Vector2 moveAmount = vector2 - position;
            if (moveAmount != Vector2.zero && _pushPassenger)
            {
                PushPassengers(moveAmount);
            }
            _rb.position = vector2;
            PositionDelta = _rb.position - position;
            if (!(_visuals)) return;
            _visuals.UpdatePhysicsState();
        }

        private void PushPassengers(Vector2 moveAmount)
        {
            Bounds bounds = _collider.bounds;
            Vector2 center = bounds.center;
            Vector2 size1 = bounds.size;
            float num = moveAmount.magnitude + _pushCollisionPadding;
            Vector2 normalized = moveAmount.normalized;
            Vector2 size2 = size1;
            Vector2 direction = normalized;
            double distance = num;
            int passengerMask = _passengerMask;
            RaycastHit2D raycastHit2D = Physics2D.BoxCast(center, size2, 0.0f, direction, (float)distance, passengerMask);
            if (!raycastHit2D || raycastHit2D.transform == transform || Vector2.Dot(normalized, raycastHit2D.normal) >= -0.0099999997764825821)
                return;
            raycastHit2D.transform.GetComponent<IPushable>()?.ApplyExternalPush(moveAmount, transform);
        }

        private void OnDrawGizmos()
        {
            Vector3 from = Application.isPlaying ? _startPosition : transform.position;
            Vector3 to = from + _moveOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(from, to);
        }
    }
}
