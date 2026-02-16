using System.Collections.Generic;
using PathOfTheInfected.Damagable;
using TidiPathFinding;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class EnemyBrainBase : MonoBehaviour, IDamageable, IEnemyMoveable
    {
        #region Interface Variables

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
        [field: SerializeField]
        public EnemyBaseState CurrentState { get; protected set; }

        #endregion

        #region Serialized Members
        [Header("Movement")]
        [SerializeField] protected MovementPersonality movementPersonality;
        [SerializeField] protected float waypointTolerance = 0.1f;
        [SerializeField] protected float repathInterval = 1f;


        [Header("Box cast - general")]
        [field: SerializeField]
        public LayerMask SpottableMask { get; protected set; }
        public Transform max;
        public Transform min;

        [Header("Line of sight")]
        [SerializeField]
        protected bool requiresLos = false;

        [SerializeField] protected LayerMask losBlockMask;


        [Header("State switch conditions")] public bool isSpottableDetected;
        public bool isSpottableInAttackRange;

        [Header("Debugging - track ranges")] public bool trackSpotRange;
        public bool trackAttackRange;

        [Header("Spottable Detection")] public float maxSpotRange = 10f;

        [Header("Attack")]
        [field: SerializeField] public float CurrentPoise { get; set; }
        public AttackSOBase attack;
        public float maxPoise = 10f;

        #endregion

        #region Protected and non-serialized members

        /// <summary>
        /// A list of currently detected entities that implement the <see cref="ISpottable"/> interface
        /// and are visible to the enemy.
        /// This property is updated during the detection process and is used to determine
        /// potential targets within the enemy's spotting range.
        /// </summary>
        public List<ISpottable> VisibleSpottables { get; protected set; } = new();

        /// <summary>
        /// The initial position of the enemy (used to calculate spot and attack ranges, as well as drawing them in the gizmos)
        /// </summary>
        public Vector3 InitialPosition { get; protected set; }

        /// <summary>
        /// Represents the current target that the enemy intends to attack.
        /// </summary>
        /// <remarks>
        /// This property identifies an object that implements the <see cref="ISpottable"/> interface,
        /// and serves as the attack target for the enemy. It is determined based on the enemy's detection
        /// logic and attack range conditions.
        /// </remarks>
        public ISpottable AttackTarget { get; protected set; }

        protected bool CurrentHasLos = false;
        public Transform ClosestTarget { get; protected set; }
        public Vector3 LastKnownTargetPosition { get; protected set; }
        public bool HasLastKnownTarget { get; set; } = false;
        protected float BestDistSq = float.MaxValue;

        protected List<Vector2> currentPath;
        protected int currentIndex;
        protected float nextRepath;
        protected Vector2 lastTargetPosition;

        protected BoxCollider2D boxCollider;

        protected Vector2 currentTargetVelocity;
        private TidiTween<float> _velocityTween;

        #endregion


        #region Virtual logic gate Methods

        /// <summary>
        /// Awake method for our enemy (can be overridden so use the base.EnemyAwake at the start of each override)
        /// </summary>
        protected virtual void EnemyAwake()
        {
            RB = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            StateMachine = new EnemyStateMachine();
            noSpottableDetectedState = Instantiate(noSpottableDetectedState);
            spottableDetectedState = Instantiate(spottableDetectedState);
            spottableInAttackRangeState = Instantiate(spottableInAttackRangeState);
        }

        /// <summary>
        /// Start method for our enemy (can be overridden so use the base.EnemyStart at the start of each override)
        /// </summary>
        protected virtual void EnemyStart()
        {
            noSpottableDetectedState.StateInit(this, StateMachine);
            spottableDetectedState.StateInit(this, StateMachine);
            spottableInAttackRangeState.StateInit(this, StateMachine);
            CurrentPoise = maxPoise;
            InitialPosition = transform.position;
            StateMachine?.InitializeDefaultState(noSpottableDetectedState);
        }

        /// <summary>
        /// Draws gizmos for the enemy when we select it in unity (can be overridden so use the base.DrawGizmosOnSelected at the start of each override)
        /// </summary>
        protected virtual void DrawGizmosOnSelected()
        {
            DrawStatesGizmos();
            DrawingSpottingRange();
            DrawAttackRange();
        }

        /// <summary>
        /// Update method for our enemy (can be overridden so use the base.EnemyUpdate at the start of each override)
        /// </summary>
        protected virtual void EnemyUpdate()
        {
            CheckForSpottablesInAttackRange();
            DetectVisibleSpottables();
            StateMachine?.ApplyQueuedStateChange();
            CurrentState = StateMachine?.CurrentState;
            StateMachine?.CurrentState.StateUpdate();
        }

        /// <summary>
        /// FixedUpdate method for our enemy (can be overridden so use the base.EnemyFixedUpdate at the start of each override)
        /// </summary>
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

        /// <summary>
        /// This method detects spottables that are in our spotting range and sets isSpottableDetected boolean to drive the state transitions
        /// </summary>
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

            ClosestTarget = null;
            BestDistSq = float.MaxValue;
            foreach (Collider2D hit in hits)
            {
                if (requiresLos)
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

                FindClosestTarget();
            }

            isSpottableDetected = VisibleSpottables.Count > 0 && !isSpottableInAttackRange;
        }

        /// <summary>
        /// Draws the spotting range gizmos (Can be overridden. But if there's no base.DrawingSpottingRange, this enemy uses a different shape for spot range)
        /// </summary>
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

        /// <summary>
        /// Draws the Attacking range gizmos (Can be overridden. But if there's no base.DrawAttackRange, this enemy uses a different shape for attack range)
        /// </summary>
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

        /// <summary>
        /// This method checks for enemies in our attacking range and sets isSpottableInAttackRange boolean and AttackTarget Transform to drive the state transitions
        /// </summary>
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
                if (hit.TryGetComponent<ISpottable>(out var spottable) &&
                    hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    if (VisibleSpottables.Contains(spottable) && damageable != null)
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

        /// <summary>
        /// Draws gizmos for the current state of the enemy. This method delegates the gizmo drawing
        /// responsibility to the `DrawGizmosOnSelected` method of the associated states
        /// (e.g., noSpottableDetectedState, spottableDetectedState, and spottableInAttackRangeState).
        /// It provides a visual debugging utility to represent the enemy's state in the Unity editor.
        /// </summary>
        protected virtual void DrawStatesGizmos()
        {
            noSpottableDetectedState?.DrawGizmosOnSelected(this);
            spottableDetectedState?.DrawGizmosOnSelected(this);
            spottableInAttackRangeState?.DrawGizmosOnSelected(this);
        }


        /// <summary>
        /// Determines if the enemy has a clear line of sight to the specified target.
        /// </summary>
        /// <param name="target">The target transform to check line of sight for.</param>
        /// <returns>True if there is a clear line of sight to the target, otherwise false.</returns>
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


        /// <summary>
        /// Updates the internal flag indicating whether the enemy has a line of sight to the target.
        /// </summary>
        /// <param name="hasLineOfSight">A boolean value determining whether the line of sight to the target is unobstructed.</param>
        protected virtual void UpdateLineOfSightFlag(bool hasLineOfSight)
        {
            CurrentHasLos = hasLineOfSight;
        }

        /// <summary>
        /// Determines the closest visible target to the enemy from the list of detected spottables.
        /// Updates the closest target's position, distance, and line of sight status accordingly.
        /// </summary>
        protected void FindClosestTarget()
        {
            Vector2 enemyPos = transform.position;

            foreach (var spottable in VisibleSpottables)
            {
                if (spottable == null) continue;

                float distSq =
                    (spottable.Transform.position - (Vector3)enemyPos).sqrMagnitude;

                if (distSq < BestDistSq)
                {
                    BestDistSq = distSq;
                    ClosestTarget = spottable.Transform;
                }
            }

            if (ClosestTarget)
            {
                LastKnownTargetPosition = ClosestTarget.position;
                HasLastKnownTarget = true;
            }
        }

        #endregion

        #region EnemyMovement

        public virtual void MoveEnemy(Vector2 dir, bool instant = false)
        {
            if (!RB) return;

            float targetVx = Mathf.Sign(dir.x) * movementPersonality.maxSpeed;
            float t = Mathf.Clamp01(movementPersonality.acceleration * Time.fixedDeltaTime);
            float easedT = TidiTween<float>.Ease(movementPersonality.movementEase, t);

            float newVx;
            if (!instant)
            {
                newVx = Mathf.Lerp(RB.linearVelocity.x, targetVx, easedT);
            }
            else
            {
                newVx = targetVx;
            }

            Vector2 finalVelocity = new Vector2(newVx, RB.linearVelocity.y);

            CheckForLeftOrRightFacing(finalVelocity);
            RB.linearVelocity = finalVelocity;
        }

        /// <summary>
        /// Moves the enemy towards a specified target Transform.
        /// </summary>
        /// <param name="target">The target Transform to move towards. If null, the method exits early.</param>
        public virtual void MoveTo(Transform target)
        {
            if (!target) return;
            MoveTo(target.position);
        }

        /// <summary>
        /// Moves the enemy towards the specified target position.
        /// </summary>
        /// <param name="target">The target position to move towards.</param>
        public virtual void MoveTo(GameObject target)
        {
            if (!target) return;
            MoveTo(target.transform.position);
        }

        /// <summary>
        /// Moves the enemy towards the specified target position.
        /// </summary>
        /// <param name="target">The target position as a Vector2.</param>
        public virtual void MoveTo(Vector2 target)
        {
            if (AStarPathFinder.CurrentGraph == null || !RB) return;

            // --- Recalculate path on timer ---
            if (Time.timeSinceLevelLoad >= nextRepath)
            {
                List<Vector2> newPath =
                    AStarPathFinder.FindPath_CurrentGraph(transform.position, target);

                nextRepath = Time.timeSinceLevelLoad + repathInterval;

                if (newPath != null && newPath.Count > 0)
                {
                    currentPath = newPath;

                    float bestDistance = float.MaxValue;
                    int bestIndex = 0;

                    for (int i = 0; i < currentPath.Count; i++)
                    {
                        float dist = Vector2.Distance(transform.position, currentPath[i]);
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            bestIndex = i;
                        }
                    }

                    currentIndex = bestIndex;
                }
            }

            if (currentPath == null || currentIndex >= currentPath.Count)
            {
                MoveEnemy(Vector2.zero);
                return;
            }

            // --- Look-ahead ---
            int targetIndex = Mathf.Min(currentIndex + 1, currentPath.Count - 1);
            Vector2 waypoint = currentPath[targetIndex];

            Vector2 toWaypoint = waypoint - (Vector2)transform.position;

            // Ground enemy moves only horizontally
            Vector2 horizontalDir = new Vector2(Mathf.Sign(toWaypoint.x), 0f);

            MoveEnemy(horizontalDir);

            // Advance waypoint if close enough (horizontal check only because this is a grounded enemy)
            if (Mathf.Abs(toWaypoint.x) <= waypointTolerance)
            {
                currentIndex++;
            }
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
