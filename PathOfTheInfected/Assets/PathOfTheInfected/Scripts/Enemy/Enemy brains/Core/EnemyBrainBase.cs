using System.Collections.Generic;
using PathOfTheInfected.Damagable;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{


    public class EnemyBrainBase : MonoBehaviour, IDamageable, IEnemyMoveable
    {
        #region Interface Varibles

        #region IDamageble
        public bool IsDead { get; set; }
        [field: SerializeField] public int CurrentHealth { get; set; }

        public GameObject GameObject { get; set; }

        public int MaxHealth { get; set; }
        #endregion

        #region IEnemyMoveable

        public Rigidbody2D RB { get; set; }
        [field: SerializeField] public bool IsFacingRight { get; set; } = true;

        #endregion

        #endregion

        #region Damage

        public void TakeDamage(DamageData damageaData)
        {
            CurrentHealth -= damageaData.Damage;
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
        [field: SerializeField] public EnemyBaseState CurrentState { get; protected set; }
        #endregion

        #region Serialized Members

        [Header("Box cast - general")]
        [field:SerializeField] public LayerMask SpottableMask { get; protected set;}
        [field: SerializeField]public float CurrentPoise { get; set; }
        public Transform max;
        public Transform min;

        [Header("Line of sight")]
        [SerializeField] protected bool requiresLOS = false;
        [SerializeField] protected LayerMask losBlockMask;




        [Header("State switch conditions")]
        public bool isSpottableDetected;
        public bool isSpottableInAttackRange;

        [Header("Debugging - track ranges")]
        public bool trackSpotRange;
        public bool trackAttackRange;

        [Header("Spottable Detection")]
        public float maxSpotRange = 10f;

        [Header("Attack")]
        public AttackSOBase attack;
        public float maxPoise = 10f;

        #endregion

        #region Protected and non-serialized members
        public List<ISpottable> VisibleSpottables { get; protected set; } = new();

        public Vector3 InitialPosition { get; protected set; }

        public ISpottable AttackTarget { get; protected set; }

        protected bool CurrentHasLos = false;
        #endregion

        #region Virtual logic gate Methods

        protected virtual void EnemyAwake()
        {
            RB = GetComponent<Rigidbody2D>();
            StateMachine = new EnemyStateMachine();
            noSpottableDetectedState = Instantiate(noSpottableDetectedState);
            spottableDetectedState = Instantiate(spottableDetectedState);
            spottableInAttackRangeState = Instantiate(spottableInAttackRangeState);
        }

        protected virtual void EnemyStart()
        {
            noSpottableDetectedState.StateInit(this, StateMachine);
            spottableDetectedState.StateInit(this, StateMachine);
            spottableInAttackRangeState.StateInit(this, StateMachine);
            CurrentPoise = maxPoise;
            InitialPosition = transform.position;
            StateMachine?.InitializeDefaultState(noSpottableDetectedState);
        }

        protected virtual void DrawGizmosOnSelected()
        {
            DrawStatesGizmos();
            DrawingSpottingRange();
            DrawAttackRange();
        }

        protected virtual void EnemyUpdate()
        {
            CheckForSpottablesInAttackRange();
            DetectVisibleSpottables();
            StateMachine?.ApplyQueuedStateChange();
            CurrentState = StateMachine?.CurrentState;
            StateMachine?.CurrentState.StateUpdate();
        }

        protected virtual void EnemyFixedUpdate()
        {
            StateMachine?.CurrentState.StateFixedUpdate();
        }

        #endregion
        private void OnEnable()
        {
            InitialPosition = transform.position;
        }
        private void Awake()
        {
           EnemyAwake();
        }

        private void Start()
        {
            EnemyStart();
        }

        private void Update()
        {
            EnemyUpdate();
        }

        private void FixedUpdate()
        {
           EnemyFixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmosOnSelected();
        }

        #region Detection and drawing detection zones
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
                SpottableMask
            );

            foreach (Collider2D hit in hits)
            {
                if (requiresLOS)
                {
                    bool hasLineOfSight = HasLineOfSight(hit.transform);
                    UpdateLineOfSightFlag(hasLineOfSight);
                    if (hit.TryGetComponent(out ISpottable spottable) && hasLineOfSight)
                    {
                        VisibleSpottables.Add(spottable);

                    }
                }
                else
                {
                    if (hit.TryGetComponent(out ISpottable spottable))
                    {
                        VisibleSpottables.Add(spottable);
                    }
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

            float range = attack.MaxAttackRange;
            int facingDirection = IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }

        protected virtual void CheckForSpottablesInAttackRange()
        {
            Vector2 baseCenter = (min.position + max.position) * 0.5f;
            Vector2 baseSize = new Vector2(
                Mathf.Abs(max.position.x - min.position.x),
                Mathf.Abs(max.position.y - min.position.y)
            );

            float range = attack.MaxAttackRange;
            int facingDirection = IsFacingRight ? 1 : -1;
            float forwardOffset = range * 0.5f * facingDirection;

            Vector2 center = baseCenter + Vector2.right * forwardOffset;
            Vector2 size = new Vector2(baseSize.x + range, baseSize.y);


            Collider2D[] hits = Physics2D.OverlapBoxAll(
                center,
                size,
                0f,
                SpottableMask
            );

            bool test = false;
            ISpottable testTarget = null;
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<ISpottable>(out var spottable) && hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    if (VisibleSpottables.Contains(spottable) &&  damageable != null)
                    {
                        testTarget = spottable;
                        test = true;
                        break;
                    }
                }
            }
            isSpottableInAttackRange = test;
            AttackTarget = testTarget;
        }

        protected virtual void DrawStatesGizmos()
        {
            noSpottableDetectedState?.DrawGizmosOnSelected(this);
            spottableDetectedState?.DrawGizmosOnSelected(this);
            spottableInAttackRangeState?.DrawGizmosOnSelected(this);
        }

        protected virtual bool HasLineOfSight(Transform target)
        {
            if (!target || !transform) return false;

            Vector2 origin = transform.position;
            Vector2 dir = (target.position - transform.position);
            float dist = dir.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                dir.normalized,
                dist,
                losBlockMask
            );

            // LOS is valid if we hit nothing OR we hit the target
            return !hit || hit.transform == target;
        }


        protected virtual void UpdateLineOfSightFlag(bool hasLineOfSight)
        {
            CurrentHasLos = hasLineOfSight;
        }
        #endregion

        #region EnemyMovement
        [Header("Movement")]
        public float moveSpeed = 1f;
        public float acceleration = 1f;

        public virtual void MoveEnemy(Vector2 velocity)
        {
            if (!RB) return;

            float targetVx = Mathf.Sign(velocity.x) * moveSpeed;
            float t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);

            float newVx = Mathf.Lerp(RB.linearVelocity.x, targetVx, t);

            Vector2 finalVelocity = new Vector2(newVx, RB.linearVelocity.y);

            CheckForLeftOrRightFacing(finalVelocity);
            RB.linearVelocity = finalVelocity;
        }

        public virtual void MoveTo(Transform target)
        {
            if (!target) return;
            MoveTo(target.position);
        }

        public virtual void MoveTo(GameObject target)
        {
            if (!target) return;
            MoveTo(target.transform.position);
        }

        public virtual void MoveTo(Vector2 target)
        {
            float dx = target.x - transform.position.x;
            float dir = dx > 0 ? 1f : -1f;
            MoveEnemy(new Vector2(dir, 0f));
        }



        public virtual void CheckForLeftOrRightFacing(Vector2 velocity)
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
