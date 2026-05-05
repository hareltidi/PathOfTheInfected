using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathOfTheInfected.Combat
{
    public abstract class PerkData : ScriptableObject
    {
        public PlayerPerks perkType;
        public Sprite icon;

        public List<PerkTier> tiers;
    }

    [Serializable]
    public struct PerkTier
    {
        public int cost;
        public float cooldown;
        public float duration;

        // generic "strength" value
        public float magnitude;
    }
}