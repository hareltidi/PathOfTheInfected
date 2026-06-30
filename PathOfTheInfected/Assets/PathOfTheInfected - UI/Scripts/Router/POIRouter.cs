using System.Linq;
using GlobalMessages;
using TidiGameplayMessaging.Core;
using TidiModularUISystem.Scripts.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace PathOfTheInfected.UI.Scripts.Router
{
    public class POIRouter : UIRouter
    {
        protected override void RouteAwake()
        {
            InputManager.OnInputMethodChanged += OnInputMethodChanged;
            base.RouteAwake();
        }


        public override void CallInitOnScreens()
        {
        }

        public override void CallStartScreen()
        {
            TidiGameplayMessagingSubsystem.Instance.Broadcast<UIReady_PlayerHealth>();
        }

        public override void CallTickScreen(float deltaTime)
        {
        }

        public override void CallFixedTickScreen(float fixedDeltaTime)
        {
        }

        public override void CallDisposeScreen()
        {
        }

        protected void OnInputMethodChanged(InputMethod newMethod)
        {
            Root.EnableInClassList("ui-root__controller-mode", newMethod == InputMethod.Controller);
            Root.EnableInClassList("ui-root__keyboard-mode", newMethod == InputMethod.Keyboard);
        }
    }
}