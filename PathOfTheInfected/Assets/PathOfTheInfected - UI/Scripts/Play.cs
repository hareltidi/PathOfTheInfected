using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathOfTheInfected.UI.Scripts
{
    public class Play : MonoBehaviour
    {
        public UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var lockOnIcon = root.Q<UIButtonComponent>("Play");
                lockOnIcon.clicked += () =>
                {
                    // Load the main game scene
                    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                };
        }
    }
}