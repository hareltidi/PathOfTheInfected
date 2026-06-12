using System;
using GlobalMessages;
using TidiGameplayMessaging.Core;
using TidiModularUISystem.Scripts.Core;

namespace PathOfTheInfected.UI.PlayerUI.HealthBar.HealthBar
{
    public class HealthBarScreen : UIScreen<HealthBarView>
    {
        private IDisposable _playerHealthSubscription;
        public override void ScreenInit(HealthBarView view)
        {
            base.ScreenInit(view);
            _playerHealthSubscription = TidiGameplayMessagingSubsystem.Instance.Listen<OnPlayerHealthChangedUI,
                PlayerHealthChangedPayloadUI>(OnPlayerHealthChanged);
        }

        private void OnPlayerHealthChanged(PlayerHealthChangedPayloadUI payloadUI)
        {
            AttachedElement.SetHealthTween(payloadUI.NewHealth, payloadUI.MaxHealth);
        }

        public override void ScreenDispose()
        {
            base.ScreenDispose();
            _playerHealthSubscription?.Dispose();
        }
    }
}