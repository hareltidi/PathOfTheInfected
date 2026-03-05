using UnityEngine;
using UnityEngine.InputSystem;

namespace TidiMovementComponent2D.Misc
{
    public class InputManager : MonoBehaviour
    {
        public static PlayerInput PlayerInput;

        private void Awake()
        {
            InputAwake();
        }

        private void Update()
        {
            ConsumeInput();
        }

        protected virtual void ConsumeInput()
        {
            Movement = MoveAction.ReadValue<Vector2>();
            JumpWasPressed = JumpAction.WasPressedThisFrame();
            JumpIsHeld = JumpAction.IsPressed();
            JumpWasReleased = JumpAction.WasReleasedThisFrame();
            RunIsHeld = RunAction.IsPressed();
            DashWasPressed = DashAction.WasPressedThisFrame();
            CrouchIsHeld = CrouchAction.IsPressed();
        }

        protected virtual void InputAwake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            MoveAction = PlayerInput.actions["Move"];
            JumpAction = PlayerInput.actions["Jump"];
            RunAction = PlayerInput.actions["Run"];
            DashAction = PlayerInput.actions["Dash"];
            CrouchAction = PlayerInput.actions["Crouch"];
        }


        #region Input Actions

        protected InputAction MoveAction;

        protected InputAction JumpAction;

        protected InputAction RunAction;

        protected InputAction DashAction;
        protected InputAction CrouchAction;

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