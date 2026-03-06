using System;

namespace PathOfTheInfected.Player.Combat
{

    [Flags]
    public enum CombatIntentFlags
    {
        WantsToPunch = 1 << 0,
    }
}