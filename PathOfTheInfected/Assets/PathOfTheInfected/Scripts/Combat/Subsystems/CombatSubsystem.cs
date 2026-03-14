using PathOfTheInfected.Player.Combat;

namespace PathOfTheInfected.Combat
{
    /// <summary>
    /// Provides a base class for all combat subsystems.
    /// </summary>
    public abstract class CombatSubsystem
    {
        #region Properties

        protected PlayerCombat Owner;
        public bool IsInDebugMode { get; protected set; }

        #endregion

        /// <summary>
        /// Initializes the combat subsystem with the specified owner and debug mode setting.
        /// </summary>
        /// <param name="owner">The <see cref="PlayerCombat"/> instance representing the owner of this combat subsystem.</param>
        /// <param name="isInDebugMode">A boolean value indicating whether the subsystem should run in debug mode.</param>
        public virtual void Initialize(PlayerCombat owner, bool isInDebugMode)
        {
            Owner = owner;
            IsInDebugMode = isInDebugMode;
        }

        /// <summary>
        /// Updates the combat subsystem with the given time delta to handle any time-sensitive logic.
        /// </summary>
        /// <param name="deltaTime">The amount of time, in seconds, since the previous update.</param>
        public virtual void Update(float deltaTime)
        {

        }

        /// <summary>
        /// Updates the combat subsystem within a fixed time step to handle any time-independent logic.
        /// </summary>
        /// <param name="fixedDeltaTime">The amount of time, in seconds, since the previous fixed update.</param>
        public virtual void FixedUpdate(float fixedDeltaTime)
        {

        }

        /// <summary>
        /// Registers a confirmed combat hit with the combat subsystem.
        /// </summary>
        /// <param name="context">The <see cref="CombatHitContext"/> instance containing details about the combat hit.</param>
        public virtual void RegisterHit(CombatHitContext context)
        {
            if (IsInDebugMode)
            {
                CombatHitContextPrinter.LogCombatHitContext(context);
            }
            OnRegisterHit(context);
        }

        /// <summary>
        /// Invoked when a combat hit is registered, allowing subclasses to handle specific logic related to the hit.
        /// </summary>
        /// <param name="context">The <see cref="CombatHitContext"/> instance containing details about the combat hit.</param>
        protected virtual void OnRegisterHit(CombatHitContext context)
        {
        }


        /// <summary>
        /// Resets and clears all states maintained by the combat subsystem.
        /// </summary>
        /// <remarks>
        /// This method is intended for use in scenarios where all active or temporary states within the subsystem
        /// should be nullified or reset to their default values. This can be useful when reinitializing or
        /// preparing the subsystem for reuse.
        /// </remarks>
        public virtual void ClearStates()
        {

        }
    }
}