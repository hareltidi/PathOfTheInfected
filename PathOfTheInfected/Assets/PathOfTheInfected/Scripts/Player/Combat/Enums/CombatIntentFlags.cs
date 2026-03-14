using System;

namespace PathOfTheInfected.Player.Combat
{
    /// <summary>
    /// Represents various intent flags used in the combat system to denote player combat actions or states.
    /// This enumeration uses the <see cref="FlagsAttribute"/> to allow bitwise combination of its member values.
    /// </summary>
    [Flags]
    public enum CombatIntentFlags
    {
        WantsToPunch = 1 << 0,
    }
}