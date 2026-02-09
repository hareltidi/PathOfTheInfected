using UnityEngine;

namespace PathOfTheInfected.Enemy
{
    /// <summary>
    /// Represents a contract for enemy movement functionality, defining movement behavior and direction handling.
    /// </summary>
    public interface IEnemyMoveable
    {
        /// <summary>
        /// The rigidbody (2d) of the object who implements the <see cref="IEnemyMoveable"/> interface.
        /// </summary>
        Rigidbody2D RB { get; set; }

        /// <summary>
        /// Is the object that implements the <see cref="IEnemyMoveable"/> interface facing right?
        /// </summary>
        bool IsFacingRight { get; set; }

        /// <summary>
        /// Adjusts the enemy's movement based on the specified direction vector.
        /// Updates the enemy's Rigidbody2D velocity while considering movement properties such as speed and acceleration.
        /// Handles enemy orientation by determining the direction it is facing.
        /// </summary>
        /// <param name="dir">The direction vector for enemy movement, where x represents horizontal movement
        /// and y represents vertical movement.</param>
        void MoveEnemy(Vector2 dir);

        /// <summary>
        /// Checks the direction of movement and adjusts the enemy's facing direction accordingly.
        /// Ensures the enemy's orientation matches the horizontal direction of velocity.
        /// </summary>
        /// <param name="velocity">The velocity vector of the enemy, used to determine the current direction of movement.</param>
        void CheckForLeftOrRightFacing(Vector2 velocity);
    }
}