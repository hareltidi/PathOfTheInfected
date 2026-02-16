using System;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Movement personality data.
    /// </summary>
    [Serializable]
    public class MovementPersonality
    {
        public float maxSpeed;
        public float acceleration;
        public float turnSpeed;
        public float burstMultiplier;
        public EaseType movementEase;
    }
}