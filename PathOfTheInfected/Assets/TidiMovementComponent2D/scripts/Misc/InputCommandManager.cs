using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TidiMovementComponent2D.Misc
{
    public class InputCommandManager : MonoBehaviour
    {
        private static PlayerInput _playerInput;

        private InputAction _crouchAction;

        private InputAction _dashAction;

        private InputAction _jumpAction;

        public static Queue<InputCommand> CommandQueue { get; } = new();

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _jumpAction = _playerInput.actions["Jump"];
            _dashAction = _playerInput.actions["Dash"];
            _crouchAction = _playerInput.actions["Crouch"];
        }

        private void OnEnable()
        {
            _jumpAction.performed += OnJumpPerformed;
            _jumpAction.canceled += OnJumpCanceled;
            _dashAction.performed += OnDashPerformed;
            _crouchAction.performed += OnCrouchPerformed;
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpPerformed;
            _jumpAction.canceled -= OnJumpCanceled;
            _dashAction.performed -= OnDashPerformed;
            _crouchAction.performed -= OnCrouchPerformed;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.JumpPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.JumpReleased,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.DashPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            var item = new InputCommand
            {
                type = CommandType.CrouchPressed,
                timestamp = (float)context.time
            };
            CommandQueue.Enqueue(item);
        }
    }
}