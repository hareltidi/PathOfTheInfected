using UnityEngine;
using UnityEngine.Serialization;

namespace TidiMovementComponent2D.Misc
{
    public class LoggerObject : MonoBehaviour
    {
        [FormerlySerializedAs("UseLogger")] public bool useLogger;

        [FormerlySerializedAs("TimeScale")] [Range(0f, 1f)] public float timeScale = 1f;

        private void Awake()
        {
            if (useLogger) FrameTraceLogger.StartSession();
        }

        private void OnDisable()
        {
            if (useLogger) FrameTraceLogger.EndSessionAndWriteFile();
        }
    }
}