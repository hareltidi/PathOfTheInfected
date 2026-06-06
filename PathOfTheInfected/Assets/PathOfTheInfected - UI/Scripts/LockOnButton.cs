using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathOfTheInfected___UI.Scripts
{
    public class LockOnButton : MonoBehaviour
    {
        public UIDocument uiDocument;
        public string buttonToLockOn;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var lockOnIcon = root.Q<UIButtonComponent>(buttonToLockOn);
            lockOnIcon.Focus();
        }
    }
}