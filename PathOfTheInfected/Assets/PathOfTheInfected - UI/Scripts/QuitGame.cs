using TidiModularUISystem.Core.Examples;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace PathOfTheInfected.UI.Scripts
{
    public class QuitGame : MonoBehaviour
    {
        public UIDocument uiDocument;
        public bool canQuit = true;
        private GameObject _chicken;
        private AudioSource _chickenSound;
       [SerializeField] private AudioClip chickenSound;
        private void Start()
        {
            var root = uiDocument.rootVisualElement;
            var lockOnIcon = root.Q<UIButtonComponent>("Quit");

            if (canQuit)
            {
                lockOnIcon.clicked += Application.Quit;
            }
            else
            {
                lockOnIcon.clicked += PreventQuit;
            }
        }

        private void PreventQuit()
        {
            if (!_chicken)
            {
                _chicken = new GameObject("Chicken");
                _chickenSound = _chicken.AddComponent<AudioSource>();
                _chickenSound.clip = chickenSound;
            }
            _chickenSound.Play();
        }
    }
}