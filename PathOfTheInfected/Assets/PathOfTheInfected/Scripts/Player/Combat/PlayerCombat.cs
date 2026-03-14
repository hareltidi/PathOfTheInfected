using System.Collections.Generic;
using PathOfTheInfected.Animation;
using PathOfTheInfected.Combat;
using PathOfTheInfected.Player.Combat.Attacks;
using TidiMovementComponent2D.Core;
using UnityEngine;

namespace PathOfTheInfected.Player.Combat
{
    /// <summary>
    ///     Combat manager of the player.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        private void Start()
        {
            PlayerOwner = PlayerSm.Instance;
            _standingPunchAnim = Animator.StringToHash(standingPunchAnim.name);
            _inAirPunchAnim = Animator.StringToHash(inAirPunchAnim.name);
        }

        private void Update()
        {
            CaptureInput();
            TickTimers();
            ResolveAttackIntents();
            CallUpdateOnSubsystems();
            _punchAnim = PlayerOwner.IsGrounded ? _standingPunchAnim : _inAirPunchAnim;
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
                ActivateCombatIntentState(CombatIntentFlags.WantsToPunch);
                _bufferTimer = punchAttack.attackDef.attackBufferTime;
            }
        }

        #endregion

        #region Serialized Members

        [field: SerializeField] public PlayerPunchHitBox PlayerPunchHitBox { get; private set; }
        public PlayerAttackSoBase punchAttack;

        [Header("Animation")] [SerializeField] private AnimationClip standingPunchAnim;

        [SerializeField] private AnimationClip inAirPunchAnim;

        [Header("Subsystems")] [Tooltip("Enables the debug mode for the combo subsystem")]
        public bool debugComboSubsystem;

        [Tooltip("Enables the debug mode for the RBC (Reset based combat) subsystem")]
        public bool debugRbcSubsystem;

        #endregion

        #region Non-Serialized Members

        private float _recoveryTimer;
        private float _bufferTimer;
        private int _standingPunchAnim;
        private int _inAirPunchAnim;
        private int _punchAnim;
        public HitResult LastHitResult;
        public PlayerAttackSoBase CurrentAttack { get; private set; }

        private List<CombatSubsystem> _subsystems;

        public PlayerSm PlayerOwner { get; private set; }

        #region Combat flags

        public CombatIntentFlags CombatIntentFlags { get; private set; }
        public CombatFlags CombatFlags { get; private set; }

        #endregion

        #endregion

        #region Subsystems

        private void CallUpdateOnSubsystems()
        {
            if (_subsystems == null || _subsystems.Count <= 0) return;

            foreach (var subsystem in _subsystems) subsystem.Update(Time.deltaTime);
        }

        private void CallFixedUpdateOnSubsystems()
        {
            if (_subsystems == null || _subsystems.Count <= 0) return;

            foreach (var subsystem in _subsystems) subsystem.FixedUpdate(Time.fixedDeltaTime);
        }

        private void InitializeSubsystems()
        {
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
                ActivateCombatState(CombatFlags.Punching);
                POIAnimInstance.Instance.PlayAnimationIfNotCurrent(_punchAnim, 0, 0,
                    true, true);
            }
        }

        private void ActivateAttack(PlayerAttackSoBase attack, CombatFlags flag)
        {
            CurrentAttack = attack;
            CurrentAttack.InitAttack(this, flag);
            _bufferTimer = 0f;
        }


        public void StartRecovery(float time)
        {
            _recoveryTimer = time;
        }

        /// <summary>
        ///     A method for checking if the player can perform a punch
        /// </summary>
        /// <returns>True if the player can perform a punch, otherwise, False</returns>
        private bool CanPunch()
        {
            return !HasCombatStateActive(CombatFlags.Punching) && _recoveryTimer <= 0f;
        }

        /// <summary>
        ///     This method is responsible for decrementing delta time from our timers
        /// </summary>
        private void TickTimers()
        {
            _recoveryTimer = Mathf.Max(0, _recoveryTimer - Time.deltaTime);
            _bufferTimer = Mathf.Max(0, _bufferTimer - Time.deltaTime);
        }


        public void OnAnimationAttackMessage(AnimationAttackMessageType messageType)
        {
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