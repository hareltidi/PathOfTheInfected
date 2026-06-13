using System.Collections.Generic;
using PathOfTheInfected.Combat;
using PathOfTheInfected.Core.Scripts.Boss.States;
using PathOfTheInfected.Enemy;
using TidiMovementComponent2D.Core;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Core.Scripts.Boss
{
    public class BossBrain : MonoBehaviour, IEnemyMoveable, IAttackOwnerable
    {

        #region Interface Variables
        #region IAttackOwnerable
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }

        [Header("Poise settings")]
        [field: SerializeField] public float MaxPoise { get; set; } = 10;

        [field: SerializeField] public float CurrentPoise { get; set; }
        #endregion



        #region IEnemyMoveable
        public Rigidbody2D RB { get; set; }

        [field: SerializeField] public bool IsFacingRight { get; set; } = true;
        #endregion

        #endregion

        #region Serialized Members
        public BossHealth Health {get; private set;}

        [Header("Phases and state")]
        [SerializeField] protected List<BossPhase> phases;
        [SerializeField] protected BossState bossState;


        [Header("State Machines - Damaged state (optional)")]
        public bool damageSwitchesStates;
        public float damageStateDuration = 0.5f;


        [Header("State Machines - Touch attack (optional)")]
        public AttackDefinition touchAttackDef;
        public float touchAttackRadius = 1f;
        public Vector2 touchAttackOffset = Vector2.zero;
        public bool hasTouchAttackState;
        public float knockbackStrength = 10f;
        [field: SerializeField] public LayerMask SpottableMask { get; set; }


        [Header("Movement")]
        [SerializeField] protected MovementPersonality movementPersonality;
        [SerializeField] protected float facingFlipDeadzone = 1;

        [Header("Box cast - general")]
        public Transform max;
        public Transform min;
        [SerializeField] private Transform feetPos;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float feetCheckRadius = 0.2f;

        public int CurrentAttackIndex { get; set; } = 0;
        public AttackSOBase CurrentAttack => CurrentPhase.AllowedAttacks[CurrentAttackIndex];

        /// <summary>
        /// Persistent attack context that survives across state transitions.
        /// This ensures recovery time is maintained even if the boss temporarily leaves the attack state.
        /// </summary>
        public AttackContext AttackContext { get; set; }
        #endregion

        #region Protected and non-serialized members

        protected int CurrentPhaseIndex;

        public BossPhase CurrentPhase => phases[CurrentPhaseIndex];


        /// <summary>
        /// The player's transform that our boss fights with.
        /// </summary>
        public Transform PlayerTarget { get; protected set; }

        /// <summary>
        /// The initial position of the boss
        /// </summary>
        public Vector3 InitialPosition { get; protected set; }

        public Vector2 BossVel { get; private set; }

        public bool IsGrounded => Physics2D.OverlapCircle(feetPos.position, feetCheckRadius, groundLayer);

        protected bool Touched;

        protected float TouchedRecoveryTimer = 0;

        protected bool IsBossDamaged;
        #endregion

        #region Virtual logic gate Methods

        /// <summary>
        /// Awake method for our boss (can be overridden so use the base.BossAwake at the start of each override)
        /// </summary>
        protected virtual void BossAwake()
        {
            RB = GetComponent<Rigidbody2D>();
            Health = GetComponent<BossHealth>();
            Health.BossDamaged += OnBossDamaged;
            GameObject = gameObject;
            Transform = transform;

            if (hasTouchAttackState && touchAttackDef != null)
            {
                touchAttackDef = Instantiate(touchAttackDef);
            }

            for (int i = 0; i < phases.Count; i++)
            {
                phases[i] = Instantiate(phases[i]);
            }
            bossState = Instantiate(bossState);
            bossState.StateInit(this);
            CurrentPhase.PhaseInit(this);

        }

        /// <summary>
        /// Start method for our boss (can be overridden, so use the base.BossStart at the start of each override)
        /// </summary>
        protected virtual void BossStart()
        {
            CurrentPoise = MaxPoise;
            InitialPosition = transform.position;

            PlayerTarget = PlayerSm.Instance.transform;

            CurrentPhase.PhaseEnter();
            bossState.StateEnter();
        }

        /// <summary>
        /// Draws gizmos for the boss when we select it in unity (can be overridden so use the base.DrawGizmosOnSelected at the start of each override)
        /// </summary>
        protected virtual void DrawGizmosOnSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere((Vector2) transform.position + touchAttackOffset, touchAttackRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(feetPos.position, feetCheckRadius);
        }

        protected virtual void EvaluatePhaseTransitions()
        {
            if (CurrentPhase && CurrentPhase.ShouldTransition() && CurrentPhaseIndex + 1 <  phases.Count)
            {
                CurrentPhase?.PhaseExit();
                CurrentPhaseIndex++;
                Debug.Log($"Transitioning to phase {CurrentPhaseIndex + 1}");
                CurrentPhase?.PhaseInit(this);
            }
        }

        /// <summary>
        /// Update method for our boss (can be overridden so use the base.BossUpdate at the start of each override)
        /// </summary>
        protected virtual void BossUpdate()
        {
           EvaluatePhaseTransitions();
           CheckForLeftOrRightFacing();

           CurrentPhase.PhaseUpdate();
           bossState.StateUpdate();
           BossVel = RB ? RB.linearVelocity : Vector2.zero;
        }

        /// <summary>
        /// FixedUpdate method for our boss (can be overridden so use the base.BossFixedUpdate at the start of each override)
        /// </summary>
        protected virtual void BossFixedUpdate()
        {
            TickRecoveryOutsideAttackState();
            TouchCheck();
            bossState.StateFixedUpdate();
            CurrentPhase.PhaseFixedUpdate();
        }

        /// <summary>
        /// method for processing the attack recovery for our boss (can be overridden so use the base.TickRecoveryOutsideAttackState at the start of each override)
        /// </summary>
        protected virtual void TickRecoveryOutsideAttackState()
        {
            if (!CurrentAttack || AttackContext == null || AttackContext.IsFinished) return;
            CurrentAttack.TickRecovery(AttackContext);
        }

        /// <summary>
        /// OnDestroy method for our boss (can be overridden so use the base.OnDestroy at the start of each override)
        /// </summary>
        protected virtual void OnBossDestroyed()
        {
            Health.BossDamaged -= OnBossDamaged;
            bossState.StateExit();
            CurrentPhase.PhaseExit();
            Debug.Log("Boss Destroyed");
        }

        #endregion

        #region EnemyMovement

        public virtual void MoveEnemy(Vector2 dir, bool instant = false)
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
        /// Moves the boss towards a specified target Transform.
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
        /// Moves the boss towards the specified target position.
        /// </summary>
        /// <param name="target">The target position as a Vector2.</param>
        public virtual void MoveTo(Vector2 target)
        {
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            MoveEnemy(new Vector2(Mathf.Sign(dir.x), 0f));
        }

        /// <summary>
        /// Helper method for the boss to check for facing
        /// </summary>
        /// <param name="velocity">The velocity to check on</param>
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

        /// <summary>
        /// Helper method for the boss to check for facing using the current velocity of the boss
        /// </summary>
        public virtual void CheckForLeftOrRightFacing()
        {
            if (Mathf.Abs(BossVel.x) < facingFlipDeadzone) return;

            if (IsFacingRight && BossVel.x < 0f)
            {
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, 180f, euler.z);
                IsFacingRight = false;
            }
            else if (!IsFacingRight && BossVel.x > 0f)
            {
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, 0f, euler.z);
                IsFacingRight = true;
            }
        }

        public void FlipFacing()
        {
            if (IsFacingRight)
            {
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, 180f, euler.z);
            }
            else
            {
                var euler = transform.eulerAngles;
                transform.eulerAngles = new Vector3(euler.x, 0f, euler.z);
            }
            IsFacingRight = !IsFacingRight;
        }


        /// <summary>
        /// Makes the boss stop all movement instantly
        /// </summary>
        public virtual void StopAllMovementInstantly()
        {
            if (!RB) return;
            RB.linearVelocity = Vector2.zero;
            RB.angularVelocity = 0f;
        }

        #endregion

        private void OnEnable()
        {
            InitialPosition = transform.position;
        }

        private void Awake()
        {
            BossAwake();
        }

        private void Start()
        {
            BossStart();
        }

        private void Update()
        {
            BossUpdate();
        }

        private void FixedUpdate()
        {
            BossFixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmosOnSelected();
        }

        private void OnDestroy()
        {
            OnBossDestroyed();
        }

        protected virtual void TouchCheck()
        {
            if (!hasTouchAttackState || !touchAttackDef || AttackContext.HasHit) return;

            bool isRecovering = TouchedRecoveryTimer > 0;
            if (isRecovering)
            {
                TouchedRecoveryTimer -= Time.fixedDeltaTime;
                return; // Wait for recovery time to finish
            }

            Collider2D hit = Physics2D.OverlapCircle((Vector2) transform.position + touchAttackOffset, touchAttackRadius, SpottableMask);
            Touched = hit;
            if (Touched && !IsBossDamaged)
            {
                HitData data = new HitData()
                {
                    attackDefinition = touchAttackDef,
                    source = gameObject,
                    target = hit.gameObject,
                    isPlayerDamage = false,
                    isFirstHit = false,
                    firstHitDamageBoost = 0,
                    comboDamageScalingLevel = 1,
                    timeStamp = Time.timeSinceLevelLoad,
                    knockbackStrength = knockbackStrength,
                    attackDir = IsFacingRight ? Vector2.right : Vector2.left,
                };

                HitDispatcher.ProcessHit(ref data);

                TouchedRecoveryTimer = touchAttackDef.recoveryTime;
            }

        }

        public void OnBossDamaged()
        {
            ToggleDamaged(true);
            Invoke(nameof(DamagedToFalse), damageStateDuration);
        }

        private void DamagedToFalse()
        {
            ToggleDamaged(false);
        }
        private void ToggleDamaged(bool isDamaged)
        {
            IsBossDamaged = isDamaged;
        }

    }
}
