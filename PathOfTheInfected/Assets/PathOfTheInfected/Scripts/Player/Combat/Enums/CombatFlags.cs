using System;

namespace PathOfTheInfected.Player.Combat
{
    [Flags]
    public enum CombatFlags : uint
    {
        Punching =  1 << 0,
    }
}