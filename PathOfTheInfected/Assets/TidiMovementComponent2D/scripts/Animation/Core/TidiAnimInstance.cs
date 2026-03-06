using System.Collections.Generic;
using TidiMovementComponent2D.Core;
using UnityEngine;

namespace TidiMovementComponent2D.Animation
{
    /// <summary>
    ///   <para>Base class for code-driven animation management. A solid replacement to the animator graph...</para>
    /// </summary>
    public abstract class TidiAnimInstance : MonoBehaviour
    {
        #region InitialProperties
        [field: SerializeField] protected Animator Animator { get; private set; }
        public PlayerSm Player { get; private set; }
        protected bool IsCurrentAnimationLocked { get; set; }
        protected int PreviousAnimationHash { get; set; }

        ///<summary>
        /// <para>The current animation we need to play</para>
        ///</summary>
        protected int CurrentAnimationHash { get; set; }

        protected readonly TidiAnimStateMachine StateMachine = new();

        #endregion

        #region Overridable Methods

        /// <summary>
        ///   <para>In this system, we use hashes using the Animator.StringToHash method and inputting the AnimClip.name. Use this function to set the hashes to the corresponding animClip name</para>
        /// </summary>
        protected abstract void SetAnimHashes();


        /// <summary>
        ///   <para>Native Update loop for the animation instance. This function is called every frame inside the Update method.</para>
        /// </summary>
        protected abstract void AnimationUpdate();

        protected abstract void AnimationFixedUpdate();

        /// <summary>
        /// Called once at the start of the animation instance lifecycle.
        /// </summary>
        protected abstract void AnimationStart();

        /// <summary>
        /// The awake method for initializing any animation related properties.
        /// </summary>
        protected abstract void AnimationInitialize();

        /// <summary>
        ///  <para>Sets the animation boolean flags based on the player states. These flags are then used to determine which animation to play.</para>
        /// </summary>
        protected abstract void SetAnimationFlags();
        #endregion

        #region Playing the animation
        /// <summary>
        /// This function plays the animation with the given hash as long as the hash given
        /// isn't equal to the hash of the current animation being played
        /// </summary>
        /// <param name="hash">The animation to play</param>
        /// <param name="crossfadeDuration">The transition time between the current animation that is playing to the new animation we want to play
        /// (if crossfade duration is less or equal to 0, we use <c>Animator.Play()</c>  so we can play the animation right away without waiting)</param>
        /// <param name="canOverrideLockedAnimations">If true, the new animation will override any locked animations</param>
        /// <param name="isAnimationLocked">If true, the current animation will be locked and cannot be overridden by
        /// other animations unless canOverrideLockedAnimations is true</param>
        public void PlayAnimationIfNotCurrent(int hash, float crossfadeDuration = 0.2f, bool isAnimationLocked = false, bool canOverrideLockedAnimations = false)
        {
            if (IsCurrentAnimationLocked && !canOverrideLockedAnimations && IsAnimationPlaying(CurrentAnimationHash)) return;
            if (hash != CurrentAnimationHash)
            {
                IsCurrentAnimationLocked = isAnimationLocked;
                PreviousAnimationHash = CurrentAnimationHash;
                CurrentAnimationHash = hash;
                if (crossfadeDuration > 0f)
                {
                    Animator?.CrossFade(CurrentAnimationHash, crossfadeDuration, 0);
                }
                else
                {
                    Animator?.Play(CurrentAnimationHash, 0, 0);
                }
            }
        }


        /// <summary>
        /// This function plays the animation with the given hash without checking if the
        /// given hash is equal to the hash of the current animation that is being played
        /// </summary>
        /// <param name="hash">The animation to play</param>
        /// <param name="crossfadeDuration">The transition time between the current animation that is playing to the new animation we want to play
        /// (if crossfade duration is less or equal to 0, we use <c>Animator.Play()</c>  so we can play the animation right away without waiting)</param>
        /// <param name="isAnimationLocked">If true, the new animation will override any locked animations</param>
        /// <param name="canOverrideLockedAnimations">If true, the current animation will be locked and cannot be overridden by
        /// other animations unless canOverrideLockedAnimations is true</param>
        public void PlayAnimationForced(int hash, float crossfadeDuration = 0.2f, bool isAnimationLocked = false,
            bool canOverrideLockedAnimations = false)
        {
            if (IsCurrentAnimationLocked && !canOverrideLockedAnimations && IsAnimationPlaying(CurrentAnimationHash)) return;

            IsCurrentAnimationLocked = isAnimationLocked;
            PreviousAnimationHash = CurrentAnimationHash;
            CurrentAnimationHash = hash;
            if (crossfadeDuration > 0f)
            {
                Animator?.CrossFade(CurrentAnimationHash, crossfadeDuration, 0);
            }
            else
            {
                Animator?.Play(CurrentAnimationHash, 0, 0);
            }
        }


        /// <summary>
        /// Checks if the specified animation, identified by its hash, is currently playing on the given animator layer.
        /// </summary>
        /// <param name="hash">The hash of the animation clip to check, generated using Animator.StringToHash.</param>
        /// <param name="layer">The layer index of the animator to check. Defaults to 0 if not specified.</param>
        /// <returns>True if the specified animation is currently playing on the given layer and if the animation
        /// hasn't finished; otherwise, false.</returns>
        public bool IsAnimationPlaying(int hash, int layer = 0)
        {
            if (Animator.IsInTransition(layer))
            {
                return Animator.GetNextAnimatorStateInfo(layer).shortNameHash == hash;
            }

            return Animator.GetCurrentAnimatorStateInfo(layer).shortNameHash == hash && !IsAnimationFinished(hash, layer);
        }


        /// <summary>
        /// Checks if the specified animation has finished playing on the given layer.
        /// </summary>
        /// <param name="stateHash">The hash of the animation state to check.</param>
        /// <param name="layer">The animator layer where the animation is playing. Default is 0.</param>
        /// <returns>True if the animation has finished playing and the animator is not in a transition; otherwise, false.</returns>
        public bool IsAnimationFinished(int stateHash, int layer = 0)
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(layer);

            return stateInfo.shortNameHash == stateHash &&
                   stateInfo.normalizedTime >= 0.98f &&
                   !Animator.IsInTransition(layer);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// <para>Sets the current animation state to the new state provided.</para>
        /// </summary>
        /// <param name="newState">The new state we should switch to</param>
        public void SetAnimState(TidiAnimBaseState newState)
        {
            StateMachine.RequestStateChange(newState);
        }

        /// <summary>
        /// <para>Gets the current animation state.</para>
        /// </summary>
        /// <returns>The current animation state were in</returns>
        public TidiAnimBaseState GetAnimState()
        {
            return StateMachine.CurrentState;
        }

        public void StopAllPlayingAnimations()
        {
            Animator.Play("EmptyState"); // should stop all playing animations
        }

        #endregion

        private void Awake()
        {
            AnimationInitialize();
        }

        private void Start()
        {
            AnimationStart();
            SetAnimHashes();
            Player = PlayerSm.Instance;
        }

        void Update()
        {
            AnimationUpdate();
        }

        private void FixedUpdate()
        {
            SetAnimationFlags();
            AnimationFixedUpdate();
        }
    }
}

