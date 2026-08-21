using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TidiMovementComponent2D.Misc
{
    public class InputCommandManager : MonoBehaviour
    {
        protected static PlayerInput PlayerInput;

        protected InputAction CrouchAction;

        protected InputAction DashAction;

        protected InputAction JumpAction;

        public static Queue<InputCommand> CommandQueue { get; } = new();

        private void Awake()
        {
           InputAwake();
        }

        private void OnEnable()
        {
            OnInputEnabled();
        }

        private void OnDisable()
        {
            OnInputDisabled();
        }

        protected virtual void OnJumpPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.JumpPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        protected virtual void OnJumpCanceled(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.JumpReleased,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        protected virtual void OnDashPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.DashPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        protected virtual void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.CrouchPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        protected virtual void OnInputDisabled()
        {
            JumpAction.performed -= OnJumpPerformed;
            JumpAction.canceled -= OnJumpCanceled;
            DashAction.performed -= OnDashPerformed;
            CrouchAction.performed -= OnCrouchPerformed;
        }

        protected virtual void OnInputEnabled()
        {
            JumpAction.performed += OnJumpPerformed;
            JumpAction.canceled += OnJumpCanceled;
            DashAction.performed += OnDashPerformed;
            CrouchAction.performed += OnCrouchPerformed;
        }

        protected virtual void InputAwake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            JumpAction = PlayerInput.actions["Jump"];
            DashAction = PlayerInput.actions["Dash"];
            CrouchAction = PlayerInput.actions["Crouch"];
        }
    }
}