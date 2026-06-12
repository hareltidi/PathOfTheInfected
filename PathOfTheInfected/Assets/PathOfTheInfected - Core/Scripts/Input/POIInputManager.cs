using TidiMovementComponent2D.Misc;
using UnityEngine.InputSystem;

public class POIInputManager : InputManager
{
    #region InputActions
    private InputAction _punchAction;
    private InputAction _gameplayPauseAction;
    #endregion

    #region InputValues
    public static bool PunchPressed;
    public static bool GameplayPausePressed;
    #endregion


    protected override void InputAwake()
    {
        base.InputAwake();
        _punchAction = PlayerInput.actions["Punch"];
        _gameplayPauseAction = PlayerInput.actions["GameplayPause"];
    }


    protected override void ConsumeInput()
    {
        base.ConsumeInput();
        PunchPressed = _punchAction.WasPressedThisFrame();
        GameplayPausePressed = _gameplayPauseAction.WasPressedThisFrame();
    }
}