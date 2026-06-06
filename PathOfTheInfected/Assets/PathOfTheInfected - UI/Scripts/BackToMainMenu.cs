using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathOfTheInfected___UI.Scripts
{
    public class BackToMainMenu : MonoBehaviour
    {
        public UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var lockOnIcon = root.Q<UIButtonComponent>("Play");
            lockOnIcon.clicked += () =>
            {
                // Load the main game scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            };
        }
    }
}