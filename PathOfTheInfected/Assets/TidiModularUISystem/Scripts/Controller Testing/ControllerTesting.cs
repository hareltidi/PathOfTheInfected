using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class ControllerNavigationStressTest : MonoBehaviour
{
    private UIDocument _uiDocument;

    private VisualElement root;

    private void Awake()
    {

        _uiDocument = GetComponent<UIDocument>();

        root = _uiDocument.rootVisualElement;

        // DO NOT touch UI structure anymore
        Debug.Log("Navigation stress test attached to existing UI");

        ForceInitialFocus();
    }

    private void Update()
    {
        Vector2 move = Vector2.zero;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            move = Vector2.up;
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            move = Vector2.down;
        }
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            move = Vector2.left;
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            move = Vector2.right;
        }

        if (move != Vector2.zero)
        {
            SimulateNavigation(move);
        }
    }

    private void SimulateNavigation(Vector2 direction)
    {
        var focused = root.panel.focusController.focusedElement;

        Debug.Log($"MOVE: {direction}");
        Debug.Log($"BEFORE: {(focused != null ? focused : "NULL")}");

        // Send navigation event into current focused element
        var evt = NavigationMoveEvent.GetPooled(direction);
        evt.target = focused;
        focused?.SendEvent(evt);

        var after = root.panel.focusController.focusedElement;
        Debug.Log($"AFTER: {(after != null ? after : "NULL")}");
    }

    private void ForceInitialFocus()
    {
        // Try to find your first real button in YOUR UI
        var first = root.Q<UIButtonComponent>("Play");

        if (first != null)
        {
            first.Focus();
            Debug.Log("Initial focus set to: " + first.name);
        }
        else
        {
            Debug.LogWarning("No Button found in UI");
        }
    }
}