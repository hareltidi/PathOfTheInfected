using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace TidiGC
{
    public class TidiGCManager : MonoBehaviour
    {
        private const int CachedAutoGCTimes = 5;
        private const double StutterThresholdTime = 10.0;
        private static double _lastGCTime;
        private static readonly List<double> LastAutoGCTimes = new();
        private bool _pauseGCAttempts;

        public static bool DisabledManualCollect { get; set; }

        public static double HeapUsageThreshold { get; private set; }

        private void Awake()
        {
            GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
        }

        private void Update()
        {
            if (GarbageCollector.GCMode == GarbageCollector.Mode.Enabled ||
                Time.realtimeSinceStartupAsDouble - _lastGCTime < 1.0)  return;
            var num = GetMonoHeapUsage() / 1024.0 / 1024.0;
            if (num > HeapUsageThreshold)
            {
                if (_pauseGCAttempts)
                    return;
                ForceCollect();
                HandleGCStutter();
                if (num <= HeapUsageThreshold) return;
                _pauseGCAttempts = true;
            }
            else
            {
                _pauseGCAttempts = false;
            }
        }

        private void OnDestroy()
        {
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
        }

        private static event Action OnGCStutter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (GarbageCollector.isIncremental)
            {
                Debug.LogError(
                    "TidiGCManager Initialization Failed: Incremental GC is enabled in Project Settings. " +
                    "TidiGC requires Manual GC Control to prevent performance stutters. " +
                    "Please go to Project Settings > Player and UNCHECK 'Incremental GC' to enable this manager.");
                return;
            }

            var num = 0.12102111566341002 * SystemInfo.systemMemorySize;
            if (num < 384.0)
            {
                num = 384.0;
            }
            else if (num > 1024.0)
            {
                num = 1024.0;
            }
            HeapUsageThreshold = num;
            if (Application.isEditor || GarbageCollector.isIncremental) return;
            var target = new GameObject(nameof(TidiGCManager), typeof(TidiGCManager));
            target.hideFlags |= HideFlags.HideAndDontSave;
            DontDestroyOnLoad(target);
        }

        public static void Collect()
        {
            if (DisabledManualCollect)  return;
            ForceCollect();
        }

        public static void ForceCollect(bool blocking = true, bool compacting = false)
        {
            var gcMode = (int)GarbageCollector.GCMode;
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            Resources.UnloadUnusedAssets();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking, compacting);
            GarbageCollector.GCMode = (GarbageCollector.Mode)gcMode;
            _lastGCTime = Time.realtimeSinceStartupAsDouble;
        }

        public static long GetMemoryUsage()
        {
            return GetMemoryTotal() - Profiler.GetTotalUnusedReservedMemoryLong();
        }

        public static long GetMemoryTotal()
        {
            return Profiler.GetTotalReservedMemoryLong();
        }

        public static long GetMonoHeapUsage()
        {
            return Profiler.GetMonoUsedSizeLong();
        }

        public static long GetMonoHeapTotal()
        {
            return Profiler.GetMonoHeapSizeLong();
        }

        private static void HandleGCStutter()
        {
            LastAutoGCTimes.Add(Time.realtimeSinceStartupAsDouble);
            while (LastAutoGCTimes.Count > 5)
            {
                LastAutoGCTimes.RemoveAt(0);
            }
            if (!IsGCStutterDetected())  return;

            float num = 0.1f;
            HeapUsageThreshold *= 1.0 + num;
            Debug.LogError($"GC stutter was detected! Heap size was increased by {num * 100.0}%. Current heap threshold is: " +
                           $"{HeapUsageThreshold}MB");
            var onGcStutter = OnGCStutter;
            if (onGcStutter != null)
            {
                onGcStutter();
            }
            LastAutoGCTimes.Clear();
        }

        private static bool IsGCStutterDetected()
        {
            if (LastAutoGCTimes.Count < CachedAutoGCTimes)
            {
                return false;
            }
            double num = 0.0;
            for (var index = 0; index < LastAutoGCTimes.Count - 1; ++index)
            {
                num += Math.Abs(LastAutoGCTimes[index] - LastAutoGCTimes[index + 1]);
            }

            // CHANGED: Checks if collections are happening on average less than 10 seconds apart
            return (num / (LastAutoGCTimes.Count - 1)) < StutterThresholdTime;
        }
    }
}