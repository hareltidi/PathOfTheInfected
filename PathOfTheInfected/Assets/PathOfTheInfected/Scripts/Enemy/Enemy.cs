using System.Collections.Generic;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{


    public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable
    {
        #region Interface Variables
        #region IDamageable

        public bool IsDead { get; set; }
       [field: SerializeField] public int CurrentHealth { get; set; }

        public int MaxHealth { get; set; }
        #endregion

        #region IEnemyMoveable

        public Rigidbody2D RB { get; set; }
        [field: SerializeField] public bool IsFacingRight { get; set; } = true;

        #endregion
        #endregion

        #region Damage

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
        }

        public void Die()
        {
            Destroy(gameObject);
        }

        #endregion

        #region Plug-In States
        public EnemyStateMachine StateMachine;
        public EnemyBaseState noSpottableDetectedState;
        public EnemyBaseState spottableDetectedState;
        public EnemyBaseState spottableInAttackRangeState;
        #endregion

        #region Serialized Members

        [Header("Box cast")]
        public Transform max;
        public Transform min;
        [SerializeField] private LayerMask spottableMask;


        [Header("State switch conditions")]
        public bool isSpottableDetected = false;
        public bool isSpottableInAttackRange = false;

        [Header("Debugging - track ranges")]
        public bool trackSpotRange = false;
        public bool trackAttackRange = false;

        [Header("Spottable Detection")]
        public float maxSpotRange = 10f;
        #endregion

        #region Private and non-serialized members
        private readonly List<ISpottable> _visibleSpottables = new();
        public List<ISpottable> VisibleSpottables => _visibleSpottables;
        #endregion

        private void Awake()
        {
            RB = GetComponent<Rigidbody2D>();
            StateMachine = new EnemyStateMachine();
            noSpottableDetectedState = Instantiate(noSpottableDetectedState);
            spottableDetectedState = Instantiate(spottableDetectedState);
            spottableInAttackRangeState = Instantiate(spottableInAttackRangeState);
        }

        private void Start()
        {
            noSpottableDetectedState.StateInit(this, StateMachine);
            spottableDetectedState.StateInit(this, StateMachine);
            spottableInAttackRangeState.StateInit(this, StateMachine);

            StateMachine?.InitializeDefaultState(noSpottableDetectedState);
        }

        private void Update()
        {
            DetectVisibleSpottables();
            StateMachine?.CurrentState.StateUpdate();
        }

        private void FixedUpdate()
        {
            StateMachine?.ApplyQueuedStateChange();
            StateMachine?.CurrentState.StateFixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            DrawStatesGizmos();
            DrawingSpottingRange();

        }

        protected virtual void DetectVisibleSpottables()
        {
            _visibleSpottables.Clear();
            Vector2 baseCenter = (min.position + max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(max.position.x - min.position.x),
                Mathf.Abs(max.position.y - min.position.y)
            );

            float range = maxSpotRange;
            int facingDirection = IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);


            Collider2D[] hits = Physics2D.OverlapBoxAll(
                center,
                size,
                0f,
                spottableMask
            );

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out ISpottable spottable))
                {
                    _visibleSpottables.Add(spottable);
                }
            }
            isSpottableDetected = _visibleSpottables.Count > 0;
        }

        protected virtual void DrawingSpottingRange()
        {
            if (!trackSpotRange) return;
            Vector2 baseCenter = (min.position + max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(max.position.x - min.position.x),
                Mathf.Abs(max.position.y - min.position.y)
            );

            float range = maxSpotRange;
            int facingDirection = IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center, size);
        }

        protected virtual void AttackCheck()
        {
        }

        private void DrawStatesGizmos()
        {
            noSpottableDetectedState?.DrawGizmosOnSelected(this);
            spottableDetectedState?.DrawGizmosOnSelected(this);
            spottableInAttackRangeState?.DrawGizmosOnSelected(this);
        }

        #region EnemyMovement
        [Header("Movement")]
        public float moveSpeed = 1f;
        public float acceleration = 1f;

        public void MoveEnemy(Vector2 velocity)
        {
            if (!RB) return;
            var b = Mathf.Sign(velocity.x) * moveSpeed;
            var t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);
            velocity.x = Mathf.Lerp(velocity.x, b, t);
            CheckForLeftOrRightFacing(velocity);
            if (Mathf.Abs(velocity.x - b) > 0.0099999997764825821)
            {
                return;
            }
            velocity.x = b;
            RB.linearVelocity = velocity;
        }

        public void MoveTo(Transform target)
        {
            if (!target) return;
            MoveTo(target.position);
        }

        public void MoveTo(GameObject target)
        {
            if (!target) return;
            MoveTo(target.transform.position);
        }

        public void MoveTo(Vector2 target)
        {
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            Vector2 velocity = RB.linearVelocity;
            velocity.x =  dir.x;

            MoveEnemy(velocity);
        }



        public void CheckForLeftOrRightFacing(Vector2 velocity)
        {
            if (IsFacingRight && velocity.x < 0f)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                IsFacingRight = !IsFacingRight;
            }
            else if (!IsFacingRight && velocity.x > 0f)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
                IsFacingRight = !IsFacingRight;
            }
        }

        #endregion
    }
}