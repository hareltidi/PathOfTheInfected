using System;
using PathOfTheInfected.Damagable;
using PathOfTheInfected.Player.Combat;
using UnityEngine;

namespace PathOfTheInfected.Combat
{


    public enum PlayerPerks
    {
        SwiftStep,
        PackAPunch,
        HealthBurst
    }


    [Serializable]
    public struct PlayerPerkData
    {
        public float duration;
        public PlayerPerks perk;
        public float barPrice;
    }


    public class PlayerPerksSubsystem : CombatSubsystem
    {
        private IDisposable _playerHitMessageSubscription;


        private bool _startPerkTimer = false;

        private float _currentBarValue;

        private float _perkTimer;

        private  PlayerPerkData _perkData;

        private PlayerHealth _playerHealth;

        public float HitMultiplier;

        public override void Initialize(PlayerCombat owner, bool isInDebugMode)
        {
            base.Initialize(owner, isInDebugMode);
            _currentBarValue = 0f;
            _perkData = Owner.perkData;
            _playerHealth = PlayerOwner.GetComponent<PlayerHealth>();
            HitMultiplier = 1f;
        }

        ~PlayerPerksSubsystem()
        {
            _playerHitMessageSubscription?.Dispose();
        }

        private void GrantPerks()
        {
            switch (_perkData.perk)
            {
                case PlayerPerks.SwiftStep:
                    SwiftStep(true);
                    break;
                case PlayerPerks.PackAPunch:
                    PackAPunch(true);
                    break;
                case PlayerPerks.HealthBurst:
                    HealthBurst(true);
                    break;
            }

            _startPerkTimer = true;
        }

        private void ClearPerks()
        {
            switch (_perkData.perk)
            {
                case PlayerPerks.SwiftStep:
                    SwiftStep(false);
                    break;
                case PlayerPerks.PackAPunch:
                    PackAPunch(false);
                    break;
                case PlayerPerks.HealthBurst:
                    HealthBurst(false);
                    break;
            }

            _startPerkTimer = false;
        }

        public override void Update(float deltaTime)
        {
            if (_startPerkTimer)
            {
                _perkTimer = _perkData.duration;
                _perkTimer -= deltaTime;
                if (_perkTimer <= 0f)
                {
                    ClearPerks();
                }
            }
        }


        private void SwiftStep(bool endPerk)
        {
            PlayerOwner.ComboSpeedMultiplier = endPerk ? 1f : 1.25f;
        }

        private void PackAPunch(bool endPerk)
        {
            HitMultiplier = endPerk ? 1f : 1.5f;
        }

        private void HealthBurst(bool endPerk)
        {
            _playerHealth.CurrentHealth += endPerk ? 0f : _perkData.barPrice;
        }

        private void OnPerkButtonActivated()
        {
            if (_perkData.barPrice <= _currentBarValue)
            {
                _currentBarValue -= _perkData.barPrice;
                GrantPerks();
            }
        }

        protected override void OnRegisterHit(in CombatHitContext context)
        {
            base.OnRegisterHit(in context);
           _currentBarValue += 1f * 1.25f;
        }
    }
}
