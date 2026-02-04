using System.Collections.Generic;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{


    public class Enemy : MonoBehaviour, IDamageable, IEnemyMoveable
    {
        #region Interface Varibles

        #region IDamageble
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

        [Header("State Debugging")]
        public EnemyBaseState CurrentState;
        #endregion

        #region Serialized Members

        [Header("Box cast - general")]
        [SerializeField] private LayerMask spottableMask;
        public Transform max;
        public Transform min;



        [Header("State switch conditions")]
        public bool isSpottableDetected = false;
        public bool isSpottableInAttackRange = false;

        [Header("Debugging - track ranges")]
        public bool trackSpotRange = false;
        public bool trackAttackRange = false;

        [Header("Spottable Detection")]
        public float maxSpotRange = 10f;

        [Header("Attack")]
        public List<EnemyAttackDataSO> attackDatas = new();
        #endregion

        #region Private and non-serialized members
        public List<ISpottable> VisibleSpottables { get; private set; } = new();

        public Vector3 InitialPosition { get; private set; }

        private ISpottable _attackTarget;

        private int _currentAttackIndex = 0;
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

            InitialPosition = transform.position;
            StateMachine?.InitializeDefaultState(noSpottableDetectedState);

        }

        private void Update()
        {
            DetectVisibleSpottables();
            //AttackCheck();
            StateMachine?.ApplyQueuedStateChange();
            CurrentState = StateMachine?.CurrentState;
            StateMachine?.CurrentState.StateUpdate();
        }

        private void FixedUpdate()
        {
            StateMachine?.CurrentState.StateFixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            DrawStatesGizmos();
            DrawingSpottingRange();
            DrawAttackRange();
        }

        protected virtual void DetectVisibleSpottables()
        {
            VisibleSpottables.Clear();
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
                    VisibleSpottables.Add(spottable);
                }
            }
            isSpottableDetected = VisibleSpottables.Count > 0 && !isSpottableInAttackRange;
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

        protected virtual void DrawAttackRange()
        {
            if (!trackAttackRange) return;
            Vector2 baseCenter = (min.position + max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(max.position.x - min.position.x),
                Mathf.Abs(max.position.y - min.position.y)
            );

            float range = attackDatas[_currentAttackIndex].maxAttackRange;
            int facingDirection = IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }

        protected virtual void AttackCheck()
        {
            Vector2 baseCenter = (min.position + max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(max.position.x - min.position.x),
                Mathf.Abs(max.position.y - min.position.y)
            );

            float range = attackDatas[_currentAttackIndex].maxAttackRange;
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
                    _attackTarget = spottable;
                    isSpottableInAttackRange = (VisibleSpottables.Contains(spottable));
                    return;
                }
            }
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

            float targetVx = Mathf.Sign(velocity.x) * moveSpeed;
            float t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);

            float newVx = Mathf.Lerp(RB.linearVelocity.x, targetVx, t);

            Vector2 finalVelocity = new Vector2(newVx, RB.linearVelocity.y);

            CheckForLeftOrRightFacing(finalVelocity);
            RB.linearVelocity = finalVelocity;
        }

        public void MoveTo(Transform target)
        {
            if (!target)
            {
                Debug.Log("target null");
                return;
            }
            MoveTo(target.position);
        }

        public void MoveTo(GameObject target)
        {
            if (!target) return;
            MoveTo(target.transform.position);
        }

        public void MoveTo(Vector2 target)
        {
            float dx = target.x - transform.position.x;
            float dir = dx > 0 ? 1f : -1f;
            MoveEnemy(new Vector2(dir, 0f));
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
