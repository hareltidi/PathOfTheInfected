using PathOfTheInfected.Enemy;
using UnityEngine;

/// <summary>
/// A component that allows the player to be spotted by enemies or other
/// systems implementing the <see cref="PathOfTheInfected.Enemy.ISpottable"/> interface.
/// </summary>
/// <remarks>
/// Implements the <see cref="ISpottable"/> interface, providing access
/// to the player's transform.
/// </remarks>
public class PlayerSpot : MonoBehaviour, ISpottable
{
    /// <summary>
    /// Gets the <see cref="Transform"/> of the object implementing the <see cref="ISpottable"/> interface.
    /// </summary>
    /// <remarks>
    /// This property allows the retrieval of the object's spatial transformation information, including
    /// position, rotation, and scale. Primarily used to identify or interact with the object's position
    /// in the game world.
    /// </remarks>
    public Transform Transform => transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
}
