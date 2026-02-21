using System.Collections;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Hit stop manager.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the <see cref="HitStopManager"/> class.
        /// </summary>
        public static HitStopManager Instance;

        private Coroutine _hitStopRoutine;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void Initialize()
        {
            if (Instance == null)
            {
                new GameObject("HitStopManager").AddComponent<HitStopManager>();
            }
        }

        public void HitStop(float duration)
        {
            if (_hitStopRoutine != null)
            {
                StopCoroutine(_hitStopRoutine);
            }
            _hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float originalTimeScale = Time.timeScale;

            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalTimeScale;
        }
    }
}