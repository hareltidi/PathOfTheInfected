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

        /// <summary>
        /// We set _comboResetTime to -1f to indicate that the combo should never
        /// reset automatically. It will only reset when the player is hit.
        /// </summary>
        public float ComboResetTime { get; set; } = 3f;

        private IDisposable _playerHitMessageSubscription;

        private bool _isComboActive = false;


        private float _comboTimer;
        private float _comboResetTime = -1f; // Time in seconds to reset the combo if no new hits are registered
        public override void Initialize(PlayerCombat owner, bool isInDebugMode)
        {
            base.Initialize(owner, isInDebugMode);
            _playerHitMessageSubscription = TidiGameplayMessagingSubsystem.Instance.Subscribe<PlayerHitChannel>(OnPlayerHit);
        }

        public override void Update(float deltaTime)
        {
            if (_isComboActive)
            {
                TickTimer(deltaTime);
                if (_comboTimer >= _comboResetTime)
                {
                    ClearStates();
                }
            }
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            base.FixedUpdate(fixedDeltaTime);
        }

        protected override void OnRegisterHit(in CombatHitContext context)
        {
            _isComboActive = true;
            ComboCount++;
            _comboTimer = 0f;
            ComboHitMultiplier = Mathf.Min(ComboHitMultiplier + 0.25f, MaxComboHitMultiplier);
            ComboSpeedMultiplier = Mathf.Min(ComboSpeedMultiplier + 0.15f,  MaxComboSpeedMultiplier);
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
            _comboResetTime = -1f; // Set to -1 to indicate that the combo should not reset automatically until the next hit
            PlayerOwner.ComboSpeedMultiplier = ComboSpeedMultiplier;
            if (IsInDebugMode)
            {
                Debug.Log($"Clearing Combo Perks!");
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
            Debug.Log("OnPlayerHit");
            ClearStates();
        }

        private bool AreComboPerksZero()
        {
            return !_isComboActive && _comboTimer == 0f && ComboCount == 0 && Mathf.Approximately(ComboHitMultiplier, 1f)
                   && Mathf.Approximately(ComboSpeedMultiplier, 1f);
        }

    }
}