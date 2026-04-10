using System.Collections.Generic;
using PathOfTheInfected.Animation;
using PathOfTheInfected.Animation.BlendSpaces;
using PathOfTheInfected.Combat;
using PathOfTheInfected.Player.Combat.Attacks;
using TidiMovementComponent2D.Animation;
using TidiMovementComponent2D.Animation.BlendSpaces;
using TidiMovementComponent2D.Animation.BlendSpaces.Playables;
using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace PathOfTheInfected.Player.Combat
{
    /// <summary>
    /// Combat manager of the player.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack Timeline")]
        [SerializeField] private bool useTimeBasedAttackWindows = true;

        private void Start()
        {
            PlayerOwner = PlayerSm.Instance;
            AnimInstance = POIAnimInstance.Instance;

            if (animationDriver == null)
                animationDriver = GetComponent<TidiAnimationDriver>();

            InitializeSubsystems();

            if (!useTimeBasedAttackWindows)
            {
                AnimInstance.OnAnimationEnded += OnAnimationEnded;
            }
        }

        private void OnDestroy()
        {
            if (!useTimeBasedAttackWindows && AnimInstance != null)
            {
                AnimInstance.OnAnimationEnded -= OnAnimationEnded;
            }

            ResumeBaseAnimation();
        }


        private void Update()
        {
            CaptureInput();
            ResolveAttackIntents();
            TickAttackTimeline();
            TickTimers();
            CallUpdateOnSubsystems();
        }

        private void FixedUpdate()
        {
            CallFixedUpdateOnSubsystems();
        }

        #region Input

        /// <summary>
        ///     This method captures the attack-related inputs from the player and also applies buffering
        /// </summary>
        private void CaptureInput()
        {
            // Punch attack Input consumption
            if (POIInputManager.PunchPressed)
            {
                if (punchAttack == null || punchAttack.attackDef == null)
                {
                    Debug.LogWarning("Punch attack or attack definition is missing on PlayerCombat.", this);
                    return;
                }

                ActivateCombatIntentState(CombatIntentFlags.WantsToPunch);
                _bufferTimer = Mathf.Max(_bufferTimer, punchAttack.attackDef.attackBufferTime);
            }
        }

        #endregion

        #region Serialized Members

        [field: SerializeField] public PlayerPunchHitBox PlayerPunchHitBox { get; private set; }
        public PlayerAttackSoBase punchAttack;

        [Header("Animation")] [SerializeField] private BlendSpace2DVector2 punchBlendSpace;
        [SerializeField] private TidiAnimationDriver animationDriver;

        [Header("Subsystems - General")] [Tooltip("Enables the debug mode for the combo subsystem")]
        public bool debugComboSubsystem;

        [Tooltip("Enables the debug mode for the RBC (Reset based combat) subsystem")]
        public bool debugRbcSubsystem;

        [Space] [Header("Combo system")] public float maxComboHitMultiplier = 2f;
        public float maxComboSpeedMultiplier = 3f;

        [Tooltip("The amount of hits in a combo required to grant a full reset in our RBC system. " +
                 "This only applies to aerial combos, grounded combos will NOT get this perk")]
        public int aerialFullResetComboThreshold;

        [Header("RBC system")] public bool grantFullReset = false;

        #endregion

        #region Non-Serialized Members

        private float _recoveryTimer;
        private float _bufferTimer;
        private int _punchAnim;
        private float _attackStartupTimer;
        private float _attackActiveTimer;
        private bool _hasAttackStarted;
        private bool _attackTimelineRunning;
        private bool _suspendedAnimInstance;
        private BlendApplicationMode _activeBlendMode = BlendApplicationMode.Override;
        public HitResult LastHitResult;
        public PlayerAttackSoBase CurrentAttack { get; private set; }

        private List<CombatSubsystem> _subsystems = new();

        public PlayerSm PlayerOwner { get; private set; }

        public POIAnimInstance AnimInstance { get; private set; }

        public PlayerRBCSubsystem RbcSubsystem { get; private set; }
        public PlayerComboSubsystem ComboSubsystem { get; private set; }

        #region Combat flags

        public CombatIntentFlags CombatIntentFlags { get; private set; }
        public CombatFlags CombatFlags { get; private set; }

        #endregion

        #endregion

        #region Subsystems

        /// <summary>
        /// Invokes the Update method on all registered combat subsystems, enabling them to perform their
        /// per-frame logic. This ensures that each subsystem processes game logic like timers, state
        /// updates, or other functionality during the frame update cycle.
        /// </summary>
        private void CallUpdateOnSubsystems()
        {
            if (_subsystems == null || _subsystems.Count <= 0) return;

            foreach (var subsystem in _subsystems)
            {
                subsystem.Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Invokes the FixedUpdate method on all registered combat subsystems, enabling them to perform their
        /// per-fixed frame logic.
        /// </summary>
        private void CallFixedUpdateOnSubsystems()
        {
            if (_subsystems == null || _subsystems.Count <= 0) return;

            foreach (var subsystem in _subsystems)
            {
                subsystem.FixedUpdate(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Initializes and registers combat-related subsystems for the player.
        /// This includes setting up the Reset Based Combat (RBC) subsystem
        /// and adding it to the internal list of active subsystems.
        /// </summary>
        private void InitializeSubsystems()
        {
            // RBC
            RbcSubsystem = new PlayerRBCSubsystem();
            RbcSubsystem.Initialize(this, debugRbcSubsystem);
            RbcSubsystem.HasFullReset = grantFullReset;
            _subsystems.Add(RbcSubsystem);

            //Combo
            ComboSubsystem = new PlayerComboSubsystem();
            ComboSubsystem.Initialize(this, debugComboSubsystem);
            ComboSubsystem.MaxComboHitMultiplier = maxComboHitMultiplier;
            ComboSubsystem.MaxComboSpeedMultiplier = maxComboSpeedMultiplier;
            ComboSubsystem.FullResetComboThreshold = aerialFullResetComboThreshold;
            _subsystems.Add(ComboSubsystem);
        }


        /// <summary>
        /// Registers the given combat hit context on all active combat subsystems for further processing.
        /// </summary>
        /// <param name="context">
        /// The context of the combat hit, containing details such as the source, target, attack definition, outcome,
        /// and additional information relevant to the hit.
        /// </param>
        public void RegisterHitsOnSubsystems(CombatHitContext context)
        {
            if (_subsystems == null || _subsystems.Count <= 0) return;

            foreach (var subsystem in _subsystems)
            {
                subsystem.RegisterHit(context);
            }
        }

        #endregion

        #region Attack Loop

        private void ResolveAttackIntents()
        {
            if (_bufferTimer <= 0f) return;
            if (_recoveryTimer > 0f) return;

            // Punch intent
            if (HasCombatIntentStateActive(CombatIntentFlags.WantsToPunch) && CanPunch())
            {
                ClearCombatIntentState(CombatIntentFlags.WantsToPunch);
                ActivateAttack(punchAttack, CombatFlags.Punching);
                PlayAnimation();
            }
        }

        private void PlayAnimation()
        {
            if (animationDriver && punchBlendSpace)
            {
                var input = InputManager.Movement;

                // No stick input still needs deterministic directional sampling.
                if (input.sqrMagnitude <= 0.0001f)
                {
                    input = new Vector2(PlayerOwner && PlayerOwner.IsFacingRight ? 1f : -1f, 0f);
                }

                BlendResult result = punchBlendSpace.Evaluate(input.normalized);
                animationDriver.Apply(in result);
                _activeBlendMode = result.Mode;

                if (_activeBlendMode == BlendApplicationMode.Override)
                {
                    SuspendBaseAnimation();
                }
                else
                {
                    ResumeBaseAnimation();
                }
            }
        }

        private void SuspendBaseAnimation()
        {
            if (_suspendedAnimInstance || !AnimInstance)
                return;

            AnimInstance.enabled = false;
            _suspendedAnimInstance = true;
        }

        private void ResumeBaseAnimation()
        {
            if (!_suspendedAnimInstance || !AnimInstance)
                return;

            AnimInstance.enabled = true;
            _suspendedAnimInstance = false;
        }


        /// <summary>
        /// Activates a specific attack for the player and updates the combat state accordingly.
        /// </summary>
        /// <param name="attack">The attack to be activated, represented as a PlayerAttackSoBase instance.</param>
        /// <param name="flag">The combat flag associated with the attack, used to control combat states.</param>
        private void ActivateAttack(PlayerAttackSoBase attack, CombatFlags flag)
        {
            CurrentAttack = attack;
            ActivateCombatState(flag);
            CurrentAttack.InitAttack(this, flag);
            _bufferTimer = 0f;

            if (useTimeBasedAttackWindows)
            {
                StartAttackTimeline(CurrentAttack);
            }
        }

        private void StartAttackTimeline(PlayerAttackSoBase attack)
        {
            _attackTimelineRunning = true;
            _hasAttackStarted = false;

            var attackDef = attack ? attack.attackDef : null;
            _attackStartupTimer = attackDef ? Mathf.Max(0f, attackDef.startupTime) : 0f;
            _attackActiveTimer = attackDef
                ? Mathf.Max(0f, attackDef.activeTime + Mathf.Max(0f, attackDef.animationEndPadding))
                : 0f;

            if (_attackStartupTimer <= 0f)
            {
                BeginAttackActivePhase();
            }
        }

        private void TickAttackTimeline()
        {
            if (!useTimeBasedAttackWindows || !_attackTimelineRunning || CurrentAttack == null)
                return;

            if (!_hasAttackStarted)
            {
                _attackStartupTimer -= Time.deltaTime;
                if (_attackStartupTimer <= 0f)
                {
                    BeginAttackActivePhase();
                }
                return;
            }

            _attackActiveTimer -= Time.deltaTime;
            if (_attackActiveTimer <= 0f)
            {
                EndCurrentAttackFromTimeline();
            }
        }

        private void BeginAttackActivePhase()
        {
            if (_hasAttackStarted || !CurrentAttack)
                return;

            _hasAttackStarted = true;
            CurrentAttack.StartAttack();
        }

        private void EndCurrentAttackFromTimeline()
        {
            if (!_attackTimelineRunning || !CurrentAttack)
                return;

            CurrentAttack.EndAttack();
            CurrentAttack = null;
            _attackTimelineRunning = false;
            _hasAttackStarted = false;
            _attackStartupTimer = 0f;
            _attackActiveTimer = 0f;

            if (animationDriver)
            {
                bool restorePose = _activeBlendMode == BlendApplicationMode.Override;
                animationDriver.Clear(restorePose);
            }

            ResumeBaseAnimation();
            _activeBlendMode = BlendApplicationMode.Override;
        }


        public void StartRecovery(float time)
        {
            _recoveryTimer = time;
        }

        /// <summary>
        /// A method for checking if the player can perform a punch
        /// </summary>
        /// <returns>True if the player can perform a punch, otherwise, False</returns>
        private bool CanPunch()
        {
            return !HasCombatStateActive(CombatFlags.Punching) && _recoveryTimer <= 0f;
        }

        /// <summary>
        /// This method is responsible for decrementing delta time from our timers
        /// </summary>
        private void TickTimers()
        {
            _recoveryTimer = Mathf.Max(0, _recoveryTimer - Time.deltaTime);
            _bufferTimer = Mathf.Max(0, _bufferTimer - Time.deltaTime);

            if (_bufferTimer <= 0f)
            {
                ClearCombatIntentState(CombatIntentFlags.WantsToPunch);
            }
        }


        public void OnAnimationAttackMessage(AnimationAttackMessageType messageType)
        {
            if (useTimeBasedAttackWindows) return;
            
            switch (messageType)
            {
                case AnimationAttackMessageType.Start:
                    CurrentAttack.StartAttack();
                    break;
                case AnimationAttackMessageType.End:
                    CurrentAttack.EndAttack();
                    break;
            }
        }

        private void OnAnimationEnded(AnimationHandle handle, AnimationEndReason reason)
        {
            if (useTimeBasedAttackWindows) return;
            
            if (handle.Hash == _punchAnim)
            {
                OnAnimationAttackMessage(AnimationAttackMessageType.End);
                PlayerOwner.transform.rotation = PlayerOwner.OriginalRot;
            }
        }

        #endregion

        #region BitFlag Helpers - Intent

        /// <summary>
        ///     Determines whether a specific combat state, identified by the given <c>CombatIntentFlags</c>,
        ///     is currently active for the player.
        /// </summary>
        /// <param name="flag">The combat intent state flag to check, represented as a <c>CombatIntentFlags</c> enumeration value.</param>
        /// <returns>Returns <c>true</c> if the specified combat intent state is active; otherwise, <c>false</c>.</returns>
        public bool HasCombatIntentStateActive(CombatIntentFlags flag)
        {
            return (CombatIntentFlags & flag) != 0;
        }


        /// <summary>
        ///     Activates the specified combat intent state by setting the corresponding flag in the <c>CombatIntentFlags</c>
        ///     bitmask.
        /// </summary>
        /// <param name="flag">The <see cref="CombatIntentFlags" /> value that represents the combat intent state to activate.</param>
        public void ActivateCombatIntentState(CombatIntentFlags flag)
        {
            CombatIntentFlags |= flag;
        }

        /// <summary>
        ///     Clears the specified combat intent state by clearing the corresponding flag in the <c>CombatIntentFlags</c>
        ///     bitmask.
        /// </summary>
        /// <param name="flag">The <see cref="CombatIntentFlags" /> value that represents the combat intent state to clear.</param>
        public void ClearCombatIntentState(CombatIntentFlags flag)
        {
            CombatIntentFlags &= ~flag;
        }

        #endregion

        #region BitFlag Helpers - Actual states

        /// <summary>
        ///     Determines whether a specific combat state, identified by the given <c>CombatFlags</c>,
        ///     is currently active for the player.
        /// </summary>
        /// <param name="flag">The combat state flag to check, represented as a <c>CombatFlags</c> enumeration value.</param>
        /// <returns>Returns <c>true</c> if the specified combat state is active; otherwise, <c>false</c>.</returns>
        public bool HasCombatStateActive(CombatFlags flag)
        {
            return (CombatFlags & flag) != 0;
        }

        /// <summary>
        ///     Activates the specified combat state by setting the corresponding flag in the <c>CombatFlags</c> bitmask.
        ///     This enables the player to enter a combat behavior, such as punching, and allows other systems to respond
        ///     to the new combat state.
        /// </summary>
        /// <param name="flag">The <see cref="CombatFlags" /> value that represents the combat state to activate.</param>
        public void ActivateCombatState(CombatFlags flag)
        {
            CombatFlags |= flag;
        }


        /// <summary>
        ///     Clears the specified combat state by clearing the corresponding flag in the <c>CombatFlags</c> bitmask.
        /// </summary>
        /// <param name="flag">The <see cref="CombatFlags" /> value that represents the combat state to clear.</param>
        public void ClearCombatState(CombatFlags flag)
        {
            CombatFlags &= ~flag;
        }

        #endregion
    }
}


