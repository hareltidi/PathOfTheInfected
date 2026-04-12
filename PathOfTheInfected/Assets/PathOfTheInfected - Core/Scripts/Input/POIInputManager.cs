using TidiMovementComponent2D.Misc;
using UnityEngine.InputSystem;

public class POIInputManager : InputManager
{
    #region InputActions
    private InputAction _punchAction;
    #endregion

    #region InputValues
    public static bool PunchPressed;
    #endregion


    protected override void InputAwake()
    {
        base.InputAwake();
        _punchAction = PlayerInput.actions["Punch"];
    }


    protected override void ConsumeInput()
    {
        base.ConsumeInput();
        PunchPressed = _punchAction.WasPressedThisFrame();
    }
}