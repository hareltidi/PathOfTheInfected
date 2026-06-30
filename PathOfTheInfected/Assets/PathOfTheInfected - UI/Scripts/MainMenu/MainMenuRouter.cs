using PathOfTheInfected.UI.Scripts.MainMenu.Buttons;
using PathOfTheInfected.UI.Scripts.Router;
using UnityEngine.UIElements;

namespace PathOfTheInfected.UI.Scripts.MainMenu
{
    public class MainMenuRouter : POIRouter
    {
        private ButtonScreen _buttonScreen;
        protected override void RouteAwake()
        {
            _buttonScreen = new ButtonScreen();
            base.RouteAwake();
        }

        #region Screens lifecycle
        public override void CallInitOnScreens()
        {
            base.CallInitOnScreens();
            ButtonContainerView buttonContainerView = Root.Q<ButtonContainerView>();
            _buttonScreen.ScreenInit(buttonContainerView);
        }

        public override void CallStartScreen()
        {
            base.CallStartScreen();
            _buttonScreen.ScreenStart();
        }

        public override void CallTickScreen(float deltaTime)
        {
            base.CallTickScreen(deltaTime);
            _buttonScreen.ScreenTick(deltaTime);
        }

        public override void CallFixedTickScreen(float fixedDeltaTime)
        {
            base.CallFixedTickScreen(fixedDeltaTime);
            _buttonScreen.ScreenFixedTick(fixedDeltaTime);
        }

        public override void CallDisposeScreen()
        {
            base.CallDisposeScreen();
            _buttonScreen.ScreenDispose();
        }
        #endregion
    }
}