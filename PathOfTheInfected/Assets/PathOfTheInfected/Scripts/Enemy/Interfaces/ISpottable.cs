using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Interface for objects that can be spotted.
    /// </summary>
    public interface ISpottable
    {
        Transform Transform { get; }
    }
}