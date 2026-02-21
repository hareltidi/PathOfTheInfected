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
        /// <summary>
        /// The max speed of the enemy
        /// </summary>
        public float maxSpeed;
        /// <summary>
        /// The acceleration of our enemy
        /// </summary>
        public float acceleration;

        /// <summary>
        /// Defines the easing function to be applied to enemy movement transitions.
        /// This controls how the enemy speeds up or decelerates based on predefined easing types.
        /// </summary>
        public EaseType movementEase;
    }
}