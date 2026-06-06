using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathOfTheInfected.Core.Scripts.Player
{
    public class BackToMenuInput : MonoBehaviour
    {
        private void Update()
        {
            if (POIInputManager.GameplayPausePressed && SceneManager.GetActiveScene().buildIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}