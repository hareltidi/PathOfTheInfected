using TidiModularUISystem.Scripts.Core;
using TidiTweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathOfTheInfected.UI.PlayerUI.HealthBar.HealthBar
{
    [UxmlElement]
    public partial class HealthBarView : UIView
    {
        private ProgressBar _healthBar;
        private float _maxHealth;
        private float _currentHealth;
        private TidiTween<float> _healthTween;
        [UxmlAttribute]
        public EaseType TweenType;
        [UxmlAttribute]
        public float TweenDuration;

        public override void Initialize()
        {
            base.Initialize();
            _healthBar = this.Q<ProgressBar>("HealthBar");
        }

        public void SetHealth(float newHealth, float maxHealth)
        {
            // 1. Update internal state
            _currentHealth = newHealth;
            _maxHealth = maxHealth;

            // 2. Prevent division by zero errors
            if (_maxHealth <= 0)
            {
                _healthBar.value = 0f;
                _healthBar.title = "0%";
                return;
            }

            // 3. Calculate percentage (0 to 100 range for UI Toolkit ProgressBar)
            float healthPercentage = (_currentHealth / _maxHealth) * 100f;

            // 4. Clamp the value between 0 and 100 to prevent layout breaking
            healthPercentage = Mathf.Clamp(healthPercentage, 0f, 100f);

            // 5. Update the UI element
            _healthBar.value = healthPercentage;

            // Optional: Format to 0 decimal places (e.g., "75%") or 1 decimal place ("75.5%")
            _healthBar.title = $"{Mathf.RoundToInt(healthPercentage)}%";
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            _maxHealth =  newMaxHealth;
            SetHealth(_currentHealth, _maxHealth);
        }

        public void SetHealthTween(float newHealth, float maxHealth)
        {
            // 1. Update internal state
            _currentHealth = newHealth;
            _maxHealth = maxHealth;

            _healthTween?.FullKill(); // Kill any existing tween to prevent conflicts

            // 2. Prevent division by zero errors
            if (_maxHealth <= 0)
            {
                _healthBar.value = 0f;
                _healthBar.title = "0%";
                return;
            }

            // 3. Calculate percentage (0 to 100 range for UI Toolkit ProgressBar)
            float healthPercentage = (_currentHealth / _maxHealth) * 100f;

            // 4. Clamp the value between 0 and 100 to prevent layout breaking
            healthPercentage = Mathf.Clamp(healthPercentage, 0f, 100f);

            _healthTween = TidiTweenManager.TweenFloat(_healthBar, _healthBar.value, healthPercentage, TweenDuration,
                value => { _healthBar.value = value; }).SetEase(TweenType);

            // Optional: Format to 0 decimal places (e.g., "75%") or 1 decimal place ("75.5%")
            _healthBar.title = $"{Mathf.RoundToInt(healthPercentage)}%";
        }
    }
}