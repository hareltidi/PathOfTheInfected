using UnityEngine;
using UnityEngine.InputSystem;

namespace TidiMovementComponent2D.Misc
{
    public class InputManager : MonoBehaviour
    {
        public static PlayerInput PlayerInput;

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            _moveAction = PlayerInput.actions["Move"];
            _jumpAction = PlayerInput.actions["Jump"];
            _runAction = PlayerInput.actions["Run"];
            _dashAction = PlayerInput.actions["Dash"];
            _crouchAction = PlayerInput.actions["Crouch"];
        }

        private void Update()
        {
            ConsumeInput();
        }

        private void ConsumeInput()
        {
            Movement = _moveAction.ReadValue<Vector2>();
            JumpWasPressed = _jumpAction.WasPressedThisFrame();
            JumpIsHeld = _jumpAction.IsPressed();
            JumpWasReleased = _jumpAction.WasReleasedThisFrame();
            RunIsHeld = _runAction.IsPressed();
            DashWasPressed = _dashAction.WasPressedThisFrame();
            CrouchIsHeld = _crouchAction.IsPressed();
        }


        #region Input Actions

        private InputAction _moveAction;

        private InputAction _jumpAction;

        private InputAction _runAction;

        private InputAction _dashAction;
        private InputAction _crouchAction;

        #endregion

        #region Input Values

        public static Vector2 Movement;

        public static bool JumpWasPressed;

        public static bool JumpIsHeld;

        public static bool JumpWasReleased;

        public static bool RunIsHeld;

        public static bool CrouchIsHeld;

        public static bool DashWasPressed;

        #endregion
    }
}