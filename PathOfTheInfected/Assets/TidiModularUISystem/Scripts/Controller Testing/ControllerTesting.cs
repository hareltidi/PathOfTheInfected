using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;

public class ControllerTesting : MonoBehaviour
{
    private UIDocument _uiDocument;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        var root = _uiDocument.rootVisualElement;

        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            Debug.Log($"KEY DOWN: {evt.keyCode}");
        });

        root.RegisterCallback<NavigationMoveEvent>(evt =>
        {
            Debug.Log($"NAV MOVE: {evt.direction}");
        });

        var button = root.Q<UIButtonComponent>("Play");

        root.schedule.Execute(() =>
        {
            button?.Focus();
        });
    }
}