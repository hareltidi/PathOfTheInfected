using System.Collections.Generic;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance;

        private readonly List<float> _hitStops = new();

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void Initialize()
        {
            if (!Instance)
            {
                new GameObject("HitStopManager").AddComponent<HitStopManager>();
            }
        }

        public void HitStop(float duration)
        {
            _hitStops.Add(duration);
        }

        private void Update()
        {
            if (_hitStops.Count == 0)
            {
                Time.timeScale = 1f;
                return;
            }

            float dt = Time.unscaledDeltaTime;

            for (int i = _hitStops.Count - 1; i >= 0; i--)
            {
                _hitStops[i] -= dt;

                if (_hitStops[i] <= 0f)
                {
                    _hitStops.RemoveAt(i);
                }
            }

            Time.timeScale = _hitStops.Count > 0 ? 0f : 1f;
        }
    }
}