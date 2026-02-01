using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace TidiMovementComponent2D.Misc
{
    public static class FrameTraceLogger
    {
        private static readonly StringBuilder LOGBuilder = new();

        private static readonly bool IsEnabled = true;

        private static int _frameCount;

        private static string _screenshotPath = "";

        public static void StartSession()
        {
            if (IsEnabled)
            {
                LOGBuilder.Clear();
                _frameCount = 0;
                _screenshotPath = Path.Combine(Application.persistentDataPath,
                    $"TraceSession_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
                Directory.CreateDirectory(_screenshotPath);
                LOGBuilder.AppendLine("--- MOVEMENT TRACE SESSION START ---");
                LOGBuilder.AppendLine($"Timestamp: {DateTime.Now}");
                LOGBuilder.AppendLine("Log Path: " + Path.Combine(_screenshotPath, "MovementTrace.txt"));
                LOGBuilder.AppendLine("------------------------------------");
            }
        }

        public static void Tick()
        {
            _frameCount++;
        }

        public static void Log(string message)
        {
            if (IsEnabled) LOGBuilder.AppendLine($"[F:{_frameCount}] {message}");
        }

        public static void CaptureScreenshot(string suffix)
        {
            if (IsEnabled)
            {
                var text = $"Frame_{_frameCount}_{suffix}.png";
                ScreenCapture.CaptureScreenshot(Path.Combine(_screenshotPath, text));
                Log("<color=orange>!!! CAPTURE: Saved screenshot '" + text + "' !!!</color>");
            }
        }

        public static void EndSessionAndWriteFile()
        {
            if (IsEnabled)
            {
                var path = Path.Combine(_screenshotPath, "MovementTrace.txt");
                try
                {
                    File.WriteAllText(path, LOGBuilder.ToString());
                    Debug.Log(
                        "<color=lime><b>[FrameTraceLogger]</b> Trace session complete. Files saved to:</color>\n" +
                        _screenshotPath);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[FrameTraceLogger] FAILED to write trace file: " + ex.Message);
                }

                LOGBuilder.Clear();
            }
        }
    }
}