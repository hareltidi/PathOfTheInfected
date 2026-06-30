using TidiMovementComponent2D.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PathOfTheInfected.Input
{
    public class POIInputCommandManager : InputCommandManager
    {

        protected InputAction PunchAction;
        protected InputAction GameplayPauseAction;

        protected override void InputAwake()
        {
            base.InputAwake();
            PunchAction = PlayerInput.actions["Punch"];
            GameplayPauseAction = PlayerInput.actions["GameplayPause"];
        }

        protected override void OnInputEnabled()
        {
            base.OnInputEnabled();
            PunchAction.performed += OnPunchPerformed;
            GameplayPauseAction.performed += OnGameplayPausePerformed;
        }


        protected override void OnInputDisabled()
        {
            base.OnInputDisabled();
            PunchAction.performed -= OnPunchPerformed;
            GameplayPauseAction.performed -= OnGameplayPausePerformed;
        }


        private void OnPunchPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.PunchPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        private void OnGameplayPausePerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.GameplayPausePressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }
    }
}