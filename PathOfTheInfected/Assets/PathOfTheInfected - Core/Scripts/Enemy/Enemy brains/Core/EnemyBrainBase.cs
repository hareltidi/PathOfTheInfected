using System;
using System.Collections.Generic;
using PathOfTheInfected.Combat;
using PathOfTheInfected.Damagable;
using PathOfTheInfected.Enemy.Health;
using TidiGameplayMessaging.Core;
using TidiPathFinding;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Provides a base implementation for the behavior and functionality of an enemy entity in the game.
    /// This class manages the enemy's movement, state transitions, health, and interactions with other game entities.
    /// It also includes functionality for detecting targets, handling line-of-sight checks, and managing attack behaviors.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D)),  RequireComponent(typeof(EnemyHealth))]
    public class EnemyBrainBase : MonoBehaviour, IEnemyMoveable, IAttackOwnerable
    {
        #region Interface Variables

        #region IAttackOwnerable
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }

        public bool IsGrounded => Physics2D.OverlapCircle(feetPos.position, 0.2f, groundLayer);

        [field: Header("Poise settings")]
        [field: SerializeField] public float MaxPoise { get; set; } = 10;
        [field: SerializeField] public float CurrentPoise { get; set; }
        #endregion


        #region IEnemyMoveable
        public Rigidbody2D RB { get; set; }
        [field: SerializeField] public bool IsFacingRight { get; set; } = true;

        #endregion

        #endregion

        #region Plug-In States

        [Header("State Machines")]
        public EnemyStateMachine StateMachine;
        public EnemyBaseState noSpottableDetectedState;
        public EnemyBaseState spottableDetectedState;
        public EnemyBaseState spottableInAttackRangeState;

        [Header("State Machines - Damaged state (optional)")]
        public bool damageSwitchesStates;
        public EnemyBaseState damagedState;

        [Header("State Machines - Touch attack (optional)")]
        public bool hasTouchAttackState;
        public EnemyBaseState touchState;

        [field: Header("State Debugging")]
        [field: SerializeField] public EnemyBaseState CurrentState { get; protected set; }

        #endregion

        #region Serialized Members
        [Header("Movement")]
        [SerializeField] protected MovementPersonality movementPersonality;
        [SerializeField] protected float waypointTolerance = 0.1f;
        [SerializeField] protected float repathInterval = 1f;
        [SerializeField] protected float facingFlipDeadzone = 1;

        [Header("Debugging - Path")]
        [SerializeField] public bool drawPathGizmos = false;

        [Header("Box cast - general")]
        [field: SerializeField]
        public LayerMask SpottableMask { get; protected set; }
        public Transform max;
        public Transform min;
        public Transform feetPos;
        public LayerMask groundLayer;
        public bool requireObjectsToBeInCameraView = true;

        [Header("Line of sight")]
        [SerializeField]
        protected bool requiresLos = false;

        [SerializeField] protected LayerMask losBlockMask;


        [Header("State switch conditions")]
        public bool isSpottableDetected;
        public bool isSpottableInAttackRange;
        public bool isEnemyDamaged;

        [Header("Debugging - track ranges")] public bool trackSpotRange;
        public bool trackAttackRange;

        [Header("Spottable Detection")] public float maxSpotRange = 10f;

        [Header("Attacks")]
        public AttackSOBase attack;

        /// <summary>
        /// Persistent attack context that survives across state transitions.
        /// This ensures recovery time is maintained even if the enemy temporarily leaves the attack state.
        /// </summary>
        public AttackContext AttackContext { get; set; }
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

        protected List<Vector2> CurrentPath;
        protected int CurrentIndex;
        protected float NextRepath;
        protected Vector2 LastTargetPosition;

        protected BoxCollider2D BoxCollider;

        protected Vector2 CurrentTargetVelocity;

        protected EnemyHealth EnemyHealth;

        public Vector2 EnemyVel { get; private set; }

        protected IDisposable DamagedSubscription;
        #endregion

        #region Virtual logic gate Methods

        /// <summary>
        /// Awake method for our enemy (can be overridden so use the base.BossAwake at the start of each override)
        /// </summary>
        protected virtual void EnemyAwake()
        {
            RB = GetComponent<Rigidbody2D>();
            BoxCollider = GetComponent<BoxCollider2D>();
            StateMachine = new EnemyStateMachine();
            noSpottableDetectedState = Instantiate(noSpottableDetectedState);
            spottableDetectedState = Instantiate(spottableDetectedState);
            spottableInAttackRangeState = Instantiate(spottableInAttackRangeState);
            if (hasTouchAttackState)
            {
                touchState = Instantiate(touchState);
            }
            damagedState = Instantiate(damagedState);

            GameObject = gameObject;
            Transform = transform;
        }

        /// <summary>
        /// Start method for our enemy (can be overridden, so use the base.BossStart at the start of each override)
        /// </summary>
        protected virtual void EnemyStart()
        {
            noSpottableDetectedState.StateInit(this, StateMachine);
            spottableDetectedState.StateInit(this, StateMachine);
            spottableInAttackRangeState.StateInit(this, StateMachine);
            if (hasTouchAttackState)
            {
                touchState.StateInit(this, StateMachine);
            }
            damagedState.StateInit(this, StateMachine);
            CurrentPoise = MaxPoise;
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

            // Draw current path for debugging
            if (drawPathGizmos && CurrentPath != null && CurrentPath.Count > 0)
            {
                Gizmos.color = Color.magenta;
                Vector3 prev = transform.position;
                for (int i = 0; i < CurrentPath.Count; i++)
                {
                    Vector3 p = (Vector3)CurrentPath[i];
                    Gizmos.DrawLine(prev, p);
                    prev = p;
                }

                // highlight current index
                if (CurrentIndex >= 0 && CurrentIndex < CurrentPath.Count)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere((Vector3)CurrentPath[CurrentIndex], 0.12f);
                }
            }
        }

        /// <summary>
        /// Update method for our enemy (can be overridden so use the base.BossUpdate at the start of each override)
        /// </summary>
        protected virtual void EnemyUpdate()
        {
            CheckForSpottablesInAttackRange();
            DetectVisibleSpottables();
            if (hasTouchAttackState)
            {
                touchState?.StateUpdate();
            }
            StateMachine?.ApplyQueuedStateChange();
            CurrentState = StateMachine?.CurrentState;
            StateMachine?.CurrentState.StateUpdate();
            damagedState?.StateUpdate();
        }

        /// <summary>
        /// FixedUpdate method for our enemy (can be overridden so use the base.BossFixedUpdate at the start of each override)
        /// </summary>
        protected virtual void EnemyFixedUpdate()
        {
            StateMachine?.CurrentState.StateFixedUpdate();
            damagedState?.StateFixedUpdate();
            if (hasTouchAttackState)
            {
                touchState?.StateFixedUpdate();
            }
            TickRecoveryOutsideAttackState();
        }

        protected void TickRecoveryOutsideAttackState()
        {
            if (!attack || AttackContext == null || AttackContext.IsFinished) return;
            if (CurrentState == spottableInAttackRangeState) return;

            attack.TickRecovery(AttackContext);
        }

        #endregion

        private void OnEnable()
        {
            InitialPosition = transform.position;
        }

        private void Awake()
        {
            EnemyAwake();
            DamagedSubscription = TidiGameplayMessagingSubsystem.Instance.Listen<OnEnemyDamaged>(OnEnemyDamaged);
            EnemyHealth = GetComponent<EnemyHealth>();
        }

        private void Start()
        {
            EnemyStart();
        }

        private void Update()
        {
            EnemyUpdate();
            EnemyVel = RB ? RB.linearVelocity : Vector2.zero;
        }

        private void FixedUpdate()
        {
            EnemyFixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmosOnSelected();
        }

        private void OnDestroy()
        {
            DamagedSubscription?.Dispose();
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

            if (requireObjectsToBeInCameraView)
            {
                isSpottableDetected = VisibleSpottables.Count > 0 && !isSpottableInAttackRange &&
                                      IsObjectInCameraView(gameObject, Camera.main);
            }
            else
            {
                isSpottableDetected = VisibleSpottables.Count > 0 && !isSpottableInAttackRange;
            }
        }

        /// <summary>
        /// Draws the spotting range gizmos (Can be overridden. But if there's no
        /// base.DrawingSpottingRange, this enemy uses a different shape for spot range)
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
                    hit.TryGetComponent<IHitResponder>(out var hitResponder))
                {
                    if (VisibleSpottables.Contains(spottable) && hitResponder != null)
                    {
                        testTarget = spottable;
                        if (attack && attack.RequireDistanceFromEnemyToSpottable)
                        {
                            test = (Mathf.Abs(Vector2.Distance(transform.position, testTarget.Transform.position))
                                    < attack.DistanceThreshold);
                        }
                        else if (attack)
                        {
                            test = true;
                        }
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

        public virtual void MoveBoss(Vector2 dir, bool instant = false)
        {
            if (!RB) return;

            float targetVx = Mathf.Sign(dir.x) * movementPersonality.maxSpeed;

            float velocityDiff = Mathf.Abs(targetVx - RB.linearVelocity.x);
            float progress = Mathf.Clamp01(velocityDiff / movementPersonality.maxSpeed);

            // Invert so easing works naturally
            float eased = TidiEasing.Ease(
                movementPersonality.movementEase,
                1f - progress
            );

            // Make sure acceleration never dies
            float accelMultiplier = Mathf.Lerp(0.3f, 1f, eased);

            float adjustedAccel = movementPersonality.acceleration * accelMultiplier;

            float newVx;
            if (!instant)
            {
                newVx = Mathf.MoveTowards(RB.linearVelocity.x, targetVx, adjustedAccel * Time.fixedDeltaTime);
            }
            else
            {
                newVx = targetVx;
            }
            var newVelocity = new Vector2(newVx, RB.linearVelocity.y);
            CheckForLeftOrRightFacing(newVelocity);
            RB.linearVelocity = newVelocity;
        }

        /// <summary>
        /// Compute the total path length from a given start index to the end of the path.
        /// Returns 0 for null/empty path or if startIndex out of range.
        /// </summary>
        protected float PathTotalLength(List<Vector2> path, int startIndex)
        {
            if (path == null || path.Count == 0 || startIndex >= path.Count) return 0f;
            float len = 0f;
            Vector2 prev = (Vector2)transform.position;
            // if startIndex > 0 use distance from prev to path[startIndex]
            for (int i = startIndex; i < path.Count; i++)
            {
                len += Vector2.Distance(prev, path[i]);
                prev = path[i];
            }
            return len;
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
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            MoveBoss(new Vector2(Mathf.Sign(dir.x), 0f));
        }

        public virtual void CheckForLeftOrRightFacing(Vector2 velocity)
        {
            if (Mathf.Abs(velocity.x) < facingFlipDeadzone) return;

            if (IsFacingRight && velocity.x < 0f)
            {
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, 180f, euler.z);
                IsFacingRight = false;
            }
            else if (!IsFacingRight && velocity.x > 0f)
            {
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, 0f, euler.z);
                IsFacingRight = true;
            }
        }

        public virtual void StopAllMovementInstantly ()
        {
            if (!RB) return;
            RB.linearVelocity = Vector2.zero;
            RB.angularVelocity = 0f;
        }

        #endregion

        #region Misc methods
        public static bool IsObjectInCameraView(GameObject obj, Camera cam)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            Renderer r = obj.GetComponent<Renderer>();
            return GeometryUtility.TestPlanesAABB(planes, r.bounds);
        }

        public void OnEnemyDamaged()
        {
           ToggleDamaged(true);
           Invoke(nameof(DamagedToFalse), EnemyHealth.flashTime);
        }

        private void DamagedToFalse()
        {
            ToggleDamaged(false);
        }
        private void ToggleDamaged(bool isDamaged)
        {
            isEnemyDamaged = isDamaged && damageSwitchesStates;
        }

        #endregion
    }
}
