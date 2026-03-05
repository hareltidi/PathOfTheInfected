namespace PathOfTheInfected.Combat
{
    public struct HitResult
    {
        /// <summary>
        /// Represents the result of a hit event in combat, indicating the outcome of the interaction.
        /// Possible values include None, Damaged, Blocked, and Invincible.
        /// This allows for detailed assessment of the impact of the hit within the combat system.
        /// </summary>
        public HitOutcome Outcome;

        /// <summary>
        /// Indicates whether the propagation of a hit event has been stopped.
        /// When set to true, it prevents further handling or processing of the hit event.
        /// </summary>
        public bool PropagationStopped;


        /// <summary>
        /// Represents the total amount of damage dealt after all calculations, including modifiers, resistances,
        /// and other adjustments, have been applied to the base damage value.
        /// </summary>
        public int FinalDamage;


        /// <summary>
        /// Merges the provided hit response into the current hit result by determining the outcome and whether the propagation should continue.
        /// </summary>
        /// <param name="response">The hit response to merge into the current hit result.</param>
        public void Merge(HitResponse response)
        {
            // If something already decided the hit is terminal, don't let later responders change it.
            if (PropagationStopped) return;

            // Map responder's response to a public-facing outcome.
            HitOutcome incomingOutcome = response.Response switch
            {
                Response.None => HitOutcome.None,
                Response.GenericHit => HitOutcome.Damaged,
                Response.DamageEnemy => HitOutcome.Damaged,
                Response.DamagePlayer => HitOutcome.Damaged,
                Response.Invincible => HitOutcome.Invincible,
                Response.Blocked => HitOutcome.Blocked,
                _ => HitOutcome.None
            };

            // Merge by priority/strength: None < Damaged < Blocked < Invincible
            if (GetOutcomePriority(incomingOutcome) > GetOutcomePriority(Outcome))
            {
                Outcome = incomingOutcome;
            }

            // Only incorporate damage when the responder explicitly dealt damage.
            if (response.Response == Response.DamageEnemy || response.Response == Response.DamagePlayer)
            {
                FinalDamage += response.FinalDamage;
            }

            // Terminal outcomes stop propagation (based on merged outcome).
            if (Outcome == HitOutcome.Invincible || Outcome == HitOutcome.Blocked)
            {
                PropagationStopped = true;
            }
        }

        /// <summary>
        /// Determines the priority level of the specified hit outcome.
        /// </summary>
        /// <param name="outcome">The hit outcome whose priority is to be determined.</param>
        /// <returns>An integer representing the priority of the hit outcome, where higher values indicate higher priority.</returns>
        private static int GetOutcomePriority(HitOutcome outcome)
        {
            return outcome switch
            {
                HitOutcome.None => 0,
                HitOutcome.Damaged => 1,
                HitOutcome.Blocked => 2,
                HitOutcome.Invincible => 3,
                _ => 0
            };
        }
    }
}