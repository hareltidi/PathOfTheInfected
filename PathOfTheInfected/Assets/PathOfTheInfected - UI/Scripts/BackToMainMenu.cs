using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace PathOfTheInfected___UI.Scripts
{
    public class BackToMainMenu : MonoBehaviour
    {
        public UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var lockOnIcon = root.Q<UIButtonComponent>("BackToMenu");

            lockOnIcon.clicked += () =>
            {
                // Load the main game scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                Debug.Log(".clicked");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            };
        }
    }
}