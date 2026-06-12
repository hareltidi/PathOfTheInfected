using System;
using UnityEngine;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// The Hit Dispatcher is responsible for Telling any GameObject that was
    /// </summary>
    public static class HitDispatcher
    {
        /// <summary>
        /// Processes combat hit data, calculates damage, and applies effects through
        /// responders attached to the target object in the hit data.
        /// </summary>
        /// <param name="hitData">The <see cref="HitData"/> describing the hit, including source, target, and attack details.</param>
        /// <returns>A HitResult containing information about the result of the hit, such as final damage and outcome.</returns>
        public static HitResult ProcessHit(ref HitData hitData)
        {
            HitResult hitResult = new();
            if (!hitData.target) return hitResult; // Validate hit data

            var responders = hitData.target.GetComponents<IHitResponder>(); // find all our hit responders

            // sort al of them so we can process them one at a time...
            Array.Sort(responders, (a, b) => b.HitPriority.CompareTo(a.HitPriority));

            foreach (var responder in responders)
            {
                var response = responder.OnHit(ref hitData); // process the hit and get the response

                hitResult.Merge(response);
                hitResult.Target = hitData.target;

                if (hitResult.PropagationStopped) break; // make sure we stop if we're stopped.
            }

            return hitResult;
        }

        /// <summary>
        /// Processes a hit based on the provided attack definition by creating the required hit data,
        /// dispatching it to the target, and calculating the resulting effects.
        /// </summary>
        /// <param name="attackDefinition">The <see cref="AttackDefinition"/> of the attack that includes damage, type, and other properties.</param>
        /// <returns>A HitResult containing the outcome of the hit, including the final damage and whether propagation was stopped.</returns>
        public static HitResult ProcessHit(ref AttackDefinition attackDefinition)
        {
            HitData hitData = new HitData()
            {
                attackDefinition = attackDefinition,
                isFirstHit = false,
                isPlayerDamage = false,
                isAttackerInAir = false,
                source = null,
                timeStamp = Time.timeSinceLevelLoad
            };

            return ProcessHit(ref hitData);
        }
    }
}
