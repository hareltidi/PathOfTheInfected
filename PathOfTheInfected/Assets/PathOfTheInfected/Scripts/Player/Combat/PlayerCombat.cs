using System;
using PathOfTheInfected.Animation;
using PathOfTheInfected.Combat;
using PathOfTheInfected.Player.Combat.Attacks;
using TidiMovementComponent2D.Core;
using UnityEngine;

namespace PathOfTheInfected.Player.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        #region Serialized Members
        [SerializeField] private PlayerPunchHitBox playerPunchHitBox;
        public PlayerAttackSoBase punchAttack;

        [Header("Animation")]
        [SerializeField] private AnimationClip standingPunchAnim;
        [SerializeField] private AnimationClip inAirPunchAnim;
        #endregion

        #region Combat flags
        public CombatIntentFlags CombatIntentFlags { get; private set; }
        public CombatFlags CombatFlags { get; private set; }

        #endregion

        private float _recoveryTimer;
        private float _bufferTimer;
        private int _standingPunchAnim;
        private int _inAirPunchAnim;
        private int _punchAnim;
        public HitResult LastHitResult;
        public PlayerAttackSoBase CurrentAttack { get; private set; }

        private void Start()
        {
            _standingPunchAnim = Animator.StringToHash(standingPunchAnim.name);
            _inAirPunchAnim = Animator.StringToHash(inAirPunchAnim.name);
        }

        private void Update()
        {
           CaptureInput();
           TickTimers();
           ResolveAttackIntents();
           _punchAnim = PlayerSm.Instance.IsGrounded ? _standingPunchAnim : _inAirPunchAnim;
        }

        private void FixedUpdate()
        {

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
                POIAnimInstance.Instance.PlayAnimationIfNotCurrent(_punchAnim, 0, true, true);
                return;
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
        /// This method captures the attack-related inputs from the player and also applies buffering
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

        /// <summary>
        /// This method is responsible for decrementing delta time from our timers
        /// </summary>
        private void TickTimers()
        {
            _recoveryTimer = Mathf.Max(0, _recoveryTimer - Time.deltaTime);
            _bufferTimer = Mathf.Max(0, _bufferTimer - Time.deltaTime);
        }


        /// <summary>
        /// A method for checking if the player can perform a punch
        /// </summary>
        /// <returns>True if the player can perform a punch, otherwise, False</returns>
        private bool CanPunch()
        {
            return !HasCombatStateActive(CombatFlags.Punching) && _recoveryTimer <= 0f;
        }


        #region BitFlag Helpers - Intent
        /// <summary>
        /// Determines whether a specific combat state, identified by the given <c>CombatIntentFlags</c>,
        /// is currently active for the player.
        /// </summary>
        /// <param name="flag">The combat intent state flag to check, represented as a <c>CombatIntentFlags</c> enumeration value.</param>
        /// <returns>Returns <c>true</c> if the specified combat intent state is active; otherwise, <c>false</c>.</returns>
        public bool HasCombatIntentStateActive(CombatIntentFlags flag)
        {
            return (CombatIntentFlags & flag) != 0;
        }


        /// <summary>
        /// Activates the specified combat intent state by setting the corresponding flag in the <c>CombatIntentFlags</c> bitmask.
        /// </summary>
        /// <param name="flag">The <see cref="CombatIntentFlags"/> value that represents the combat intent state to activate.</param>
        public void ActivateCombatIntentState(CombatIntentFlags flag)
        {
            CombatIntentFlags |= flag;
        }

        /// <summary>
        /// Clears the specified combat intent state by clearing the corresponding flag in the <c>CombatIntentFlags</c> bitmask.
        /// </summary>
        /// <param name="flag">The <see cref="CombatIntentFlags"/> value that represents the combat intent state to clear.</param>
        public void ClearCombatIntentState(CombatIntentFlags flag)
        {
            CombatIntentFlags &= ~flag;
        }
        #endregion

        #region BitFlag Helpers - Actual states

        /// <summary>
        /// Determines whether a specific combat state, identified by the given <c>CombatFlags</c>,
        /// is currently active for the player.
        /// </summary>
        /// <param name="flag">The combat state flag to check, represented as a <c>CombatFlags</c> enumeration value.</param>
        /// <returns>Returns <c>true</c> if the specified combat state is active; otherwise, <c>false</c>.</returns>
        public bool HasCombatStateActive(CombatFlags flag)
        {
            return (CombatFlags & flag) != 0;
        }

        /// <summary>
        /// Activates the specified combat state by setting the corresponding flag in the <c>CombatFlags</c> bitmask.
        /// This enables the player to enter a combat behavior, such as punching, and allows other systems to respond
        /// to the new combat state.
        /// </summary>
        /// <param name="flag">The <see cref="CombatFlags"/> value that represents the combat state to activate.</param>
        public void ActivateCombatState(CombatFlags flag)
        {
            CombatFlags |= flag;
        }


        /// <summary>
        /// Clears the specified combat state by clearing the corresponding flag in the <c>CombatFlags</c> bitmask.
        /// </summary>
        /// <param name="flag">The <see cref="CombatFlags"/> value that represents the combat state to clear.</param>
        public void ClearCombatState(CombatFlags flag)
        {
            CombatFlags &= ~flag;
        }
        #endregion
    }
}