using UnityEngine;
namespace PathOfTheInfected.Gameplay.Scripts.Envirroment
{
    public class Paralax : MonoBehaviour
    {
        private Camera _targetCamera;

        [Header("Parallax")] [SerializeField, Range(0f, 1f)]
        private float parallaxX = 0.5f;

        [SerializeField, Range(0f, 1f)] private float parallaxY = 0.5f;
        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;

        private Vector3 _startObjectPosition;
        private Vector3 _startCameraPosition;
        private bool _initialized;

        private void Awake()
        {
            InitializeIfNeeded();
            _targetCamera = Camera.main;
        }

        private void OnEnable()
        {
            InitializeIfNeeded();
        }

        private void LateUpdate()
        {
            if (!InitializeIfNeeded()) return;

            Vector3 cameraDelta = _targetCamera.transform.position - _startCameraPosition;
            Vector3 nextPosition = _startObjectPosition;

            if (followX)
            {
                nextPosition.x += cameraDelta.x * parallaxX;
            }

            if (followY)
            {
                nextPosition.y += cameraDelta.y * parallaxY;
            }

            nextPosition.z = transform.position.z;
            transform.position = nextPosition;
        }

        public void Recenter()
        {
            if (!InitializeIfNeeded()) return;
            _startObjectPosition = transform.position;
            _startCameraPosition = _targetCamera.transform.position;
        }

        private bool InitializeIfNeeded()
        {
            if (!_targetCamera)
            { 
                _targetCamera = Camera.main;
            }

            if (!_targetCamera)
            {
                return false;
            }


            if (_initialized)
            {
                return true;
            }
            _startObjectPosition = transform.position;
            _startCameraPosition = _targetCamera.transform.position;
            _initialized = true;
            return true;
        }
    }
}
