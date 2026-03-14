using System;

namespace PathOfTheInfected.Player.Combat
{
    /// <summary>
    /// Represents flags for various combat states or actions that can be active in the player's combat system.
    /// This enumeration supports bitwise operations due to its <see cref="FlagsAttribute"/>.
    /// Useful for tracking and managing active combat-related states during gameplay.
    /// </summary>
    [Flags]
    public enum CombatFlags : uint
    {
        Punching =  1 << 0,
    }
}