using UnityEngine;

public class CapFramerate : MonoBehaviour
{
    void Awake()
    {
        // Disable Unity's default VSync so your custom limit takes over
        QualitySettings.vSyncCount = 0;

        // Cap the game to a silky-smooth, standard desktop refresh rate
        Application.targetFrameRate = 144;
    }
}
