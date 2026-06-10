using UnityEngine;

namespace PathOfTheInfected.UI.Scripts
{
    public class CursorLock : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}