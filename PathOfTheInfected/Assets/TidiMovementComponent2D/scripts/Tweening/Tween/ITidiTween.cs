using System;
using UnityEngine;

namespace TidiTweening
{
    public interface ITidiTween
    {
        void Update();
        void OnCompleteKill();
        void FullKill();
        bool IsTargetDestroyed();
        void Pause();
        void Resume();
        object Target { get; set; }
        bool IsComplete { get; set; }
        bool WasKilled { get; set; }
        bool IsPaused { get; set; }
        bool IgnoreTimeScale { get; set; }
        string Identifier { get; set; }
        float DelayTime { get; set; }
        Action OnComplete { get; set; }
    }
}
