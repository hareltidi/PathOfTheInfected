using UnityEngine;
using UnityEngine.InputSystem;

public class QuitHandler : MonoBehaviour
{
    private bool _canQuit = false;
    private GameObject _chicken;
    private AudioSource _chickenSound;
    [SerializeField] private AudioClip chickenSound;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.wantsToQuit += OnWantsToQuit;
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= OnWantsToQuit;
    }

    private void Update()
    {
        bool rShitPressed = Keyboard.current.rightShiftKey.isPressed;
        bool lShiftPressed = Keyboard.current.leftShiftKey.isPressed;
        bool delKeyPressed =  Keyboard.current.backspaceKey.isPressed ||
                              Keyboard.current.deleteKey.isPressed;

        if (rShitPressed && lShiftPressed && delKeyPressed)
        {
            ExitShowcaseMode();
            Debug.Log("Secret Shortcut pressed. Exiting showcase mode.");
        }
    }

    private bool OnWantsToQuit()
    {
        if (_canQuit)
        {
            return true;
        }


        if (!_chicken)
        {
            _chicken = new GameObject("Chicken");
            _chickenSound = _chicken.AddComponent<AudioSource>();
            _chickenSound.clip = chickenSound;
            DontDestroyOnLoad(_chicken);
        }
        _chickenSound.Play();

        Debug.Log("Alt+F4 blocked to prevent players from exiting.");
        return false;
    }

    private void ExitShowcaseMode()
    {
        _canQuit = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
