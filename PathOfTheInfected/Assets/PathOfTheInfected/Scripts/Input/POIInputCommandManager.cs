using TidiMovementComponent2D.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PathOfTheInfected.Input
{
    public class POIInputCommandManager : InputCommandManager
    {

        protected InputAction PunchAction;


        protected override void InputAwake()
        {
            base.InputAwake();
            PunchAction = PlayerInput.actions["Punch"];
        }

        protected override void OnInputEnabled()
        {
            base.OnInputEnabled();
            PunchAction.performed += OnPunchPerformed;
        }


        protected override void OnInputDisabled()
        {
            base.OnInputDisabled();
            PunchAction.performed -= OnPunchPerformed;
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
    }
}