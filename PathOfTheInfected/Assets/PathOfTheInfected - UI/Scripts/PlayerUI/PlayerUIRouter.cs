using PathOfTheInfected.UI.PlayerUI.HealthBar.HealthBar;
using PathOfTheInfected.UI.Scripts.Router;
using TidiModularUISystem.Scripts.Core;
using UnityEngine.UIElements;

namespace PathOfTheInfected.UI.PlayerUI.HealthBar
{
    public class PlayerUIRouter : POIRouter
    {
        private HealthBarScreen  _healthBarScreen;

        public override void CallInitOnScreens()
        {
            _healthBarScreen = new HealthBarScreen();
            _healthBarScreen.ScreenInit(Root.Q<HealthBarView>("HealthBar-Container"));
        }

        public override void CallStartScreen()
        {
            base.CallStartScreen();
            _healthBarScreen.ScreenStart();
        }

        public override void CallTickScreen(float deltaTime)
        {
            _healthBarScreen.ScreenTick(deltaTime);
        }

        public override void CallFixedTickScreen(float fixedDeltaTime)
        {
            _healthBarScreen.ScreenFixedTick(fixedDeltaTime);
        }

        public override void CallDisposeScreen()
        {
            _healthBarScreen.ScreenDispose();
        }
    }
}