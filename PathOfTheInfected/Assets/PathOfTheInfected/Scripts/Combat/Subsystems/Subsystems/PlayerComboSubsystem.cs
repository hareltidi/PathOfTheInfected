using System;
using PathOfTheInfected.Damagable.Messages;
using PathOfTheInfected.Player.Combat;
using TidiGameplayMessaging.Core;
using UnityEngine;

namespace PathOfTheInfected.Combat
{
    public class PlayerComboSubsystem : CombatSubsystem
    {
        public int ComboCount { get; set; }

        public float MaxComboHitMultiplier { get; set; }
        public float MaxComboSpeedMultiplier { get; set; }

        public float ComboHitMultiplier { get; private set; } = 1f;
        public float ComboSpeedMultiplier { get; private set; } = 1f;

        public int FullResetComboThreshold { get; set; }

        public ComboType ComboType { get; set; } = ComboType.Grounded;

        /// <summary>
        /// We set _comboResetTime to this value since at the start of the game we set it to -1
        /// so the combo doesn't reset until the first hit is registered, after that we want it to reset after
        /// ComboResetTime seconds of no hits being registered
        /// </summary>
        public float ComboResetTime { get; set; } = 3f;

        private IDisposable _playerHitMessageSubscription;

        private bool _isComboActive;


        private float _comboTimer;
        private float _comboResetTime = -1f; // Time in seconds to reset the combo if no new hits are registered

        ~PlayerComboSubsystem()
        {
            _playerHitMessageSubscription?.Dispose();
        }

        public override void Initialize(PlayerCombat owner, bool isInDebugMode)
        {
            base.Initialize(owner, isInDebugMode);
            _playerHitMessageSubscription =
                TidiGameplayMessagingSubsystem.Instance.Listen<PlayerHitChannel>(OnPlayerHit);
        }

        public override void Update(float deltaTime)
        {
            ComboType = PlayerOwner.IsGrounded ? ComboType.Grounded : ComboType.Aerial;
            if (_isComboActive)
            {
                TickTimer(deltaTime);
                if (_comboTimer >= _comboResetTime)
                {
                    ClearStates();
                }
            }

            if (ComboCount == FullResetComboThreshold && ComboType == ComboType.Aerial)
            {
                GrantFullReset();
            }
        }

        protected override void OnRegisterHit(in CombatHitContext context)
        {
            _isComboActive = true;
            ComboCount++;
            _comboTimer = 0f;
            ComboHitMultiplier = Mathf.Min(ComboHitMultiplier + 0.25f, MaxComboHitMultiplier);
            ComboSpeedMultiplier = Mathf.Min(ComboSpeedMultiplier + 0.15f, MaxComboSpeedMultiplier);
            PlayerOwner.ComboSpeedMultiplier = ComboSpeedMultiplier;

            _comboResetTime = ComboResetTime; // Reset the combo timer to the defined reset time after each hit

            if (IsInDebugMode)
            {
                Debug.Log($"Hit detected! Combo count: {ComboCount}, Combo speed multiplier: {ComboSpeedMultiplier}, " +
                          $"Combo hit multiplier: {ComboHitMultiplier}");
            }
        }

        public override void ClearStates()
        {
            if (AreComboPerksZero()) return;
            _isComboActive = false;
            _comboTimer = 0f;
            ComboCount = 0;
            ComboSpeedMultiplier = 1f;
            ComboHitMultiplier = 1f;
            _comboResetTime =
                -1f; // Set to -1 to indicate that the combo should not reset automatically until the next hit
            PlayerOwner.ComboSpeedMultiplier = ComboSpeedMultiplier;
            StripFullReset();
            if (IsInDebugMode)
            {
                Debug.Log("Clearing Combo Perks!");
            }
        }

        private void TickTimer(float deltaTime)
        {
            if (_comboTimer < _comboResetTime)
            {
                _comboTimer += deltaTime;
            }
        }

        private void OnPlayerHit()
        {
            ClearStates();
            if (IsInDebugMode)
            {
                Debug.Log("OnPlayerHit");
            }
        }

        private bool AreComboPerksZero()
        {
            return !_isComboActive && _comboTimer == 0f && ComboCount == 0 &&
                   Mathf.Approximately(ComboHitMultiplier, 1f)
                   && Mathf.Approximately(ComboSpeedMultiplier, 1f);
        }

        private void GrantFullReset()
        {
            if (IsFullResetGranted()) return;
            Owner.RbcSubsystem.HasFullReset = true;
            Debug.Log("Full Reset Granted!");
        }

        private void StripFullReset()
        {
            if (!IsFullResetGranted()) return;
            Owner.RbcSubsystem.HasFullReset = false;
        }

        private bool IsFullResetGranted()
        {
            return Owner.RbcSubsystem.HasFullReset;
        }
    }
}