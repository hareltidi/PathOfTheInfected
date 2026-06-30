using PathOfTheInfected.Player.Combat;
using TidiMovementComponent2D.Animation;
using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.Misc;
using UnityEngine;

namespace PathOfTheInfected.Animation
{
    /// <summary>
    /// The animation instance for the player. Used to handle animations and playing them.
    /// </summary>
    public class POIAnimInstance : TidiAnimInstance
    {
        [Tooltip("The combat component that will be used for animation.")]
        public PlayerCombat playerCombat;

        /// <summary>
        /// The singleton instance of the animation manager.
        /// </summary>
        public static POIAnimInstance Instance;

        #region States

        public POIStandingAnimState StandingState { get; private set; }
        public POIInAirAnimState InAirState { get; private set; }

        #endregion

        #region Animation Data

        #region Standing Anim Data

        #region AnimClipsRefs - Standing

        [Header("Animation clips references - Standing")]
        public AnimationClip idleAnimClip;

        public AnimationClip walkAnimClip;
        public AnimationClip runAnimClip;
        public AnimationClip landAnimClip;
        public AnimationClip slideAnimClip;
        public AnimationClip jumpAnimClip;
        public AnimationClip dashAnimClip;
        public AnimationClip takeOffAnimClip;
        public AnimationClip wallSlideAnimClip;

        #endregion

        #region AnimationFlags - Standing

        public bool standingIsRunning;


        public bool standingIsWalking;


        public bool standingIsWallSliding;


        public bool standingIsDashing;

        public bool standingIsSliding;


        public bool standingIsJumping;


        public bool standingIsInAir;

        #endregion

        #region AnimHashes - Standing

        public int StandingJumpAnim { get; private set; }
        public int StandingDashAnim { get; private set; }
        public int StandingIdleAnim { get; private set; }
        public int StandingWalkAnim { get; private set; }
        public int StandingRunAnim { get; private set; }
        public int StandingLandAnim { get; private set; }
        public int StandingSlideAnim { get; private set; }
        public int StandingTakeoffAnim { get; private set; }
        public int StandingWallslideAnim { get; private set; }

        #endregion

        #endregion

        #region In Air Anim Data

        #region AnimClipRefs - InAir

        [Header("Animation clips references - In Air")]
        public AnimationClip inAirFallClip;

        public AnimationClip inAirDashFallClip;
        public AnimationClip inAirJumpClip;
        public AnimationClip inAirPunchClip;

        #endregion

        #region AnimHashes - InAir

        public int InAirJumpAnim { get; private set; }
        public int InAirFallAnim { get; private set; }
        public int InAirDashFallAnim { get; private set; }
        public int InAirPunchAnim { get; private set; }

        #endregion

        #region AnimationFlags - InAir

        public bool inAirIsJumping;
        public bool inAirIsDashing;
        public bool inAirIsAirDashFalling;
        public bool inAirIsWallSliding;

        #endregion

        #endregion

        #endregion

        #region AnimHashes - Setter

        public PlayerSm ownerPlayer;
        protected override void SetAnimHashes()
        {
            ownerPlayer = playerCombat.PlayerOwner;
            // standing state
            StandingJumpAnim = Animator.StringToHash(jumpAnimClip.name);
            StandingDashAnim = Animator.StringToHash(dashAnimClip.name);
            StandingIdleAnim = Animator.StringToHash(idleAnimClip.name);
            StandingWalkAnim = Animator.StringToHash(walkAnimClip.name);
            StandingRunAnim = Animator.StringToHash(runAnimClip.name);
            StandingLandAnim = Animator.StringToHash(landAnimClip.name);
            StandingSlideAnim = Animator.StringToHash(slideAnimClip.name);
            StandingTakeoffAnim = Animator.StringToHash(takeOffAnimClip.name);
            StandingWallslideAnim = Animator.StringToHash(wallSlideAnimClip.name);
            // InAir
            InAirFallAnim = Animator.StringToHash(inAirFallClip.name);
            InAirJumpAnim = Animator.StringToHash(inAirJumpClip.name);
            InAirPunchAnim = Animator.StringToHash(inAirPunchClip.name);
            InAirDashFallAnim = Animator.StringToHash(inAirDashFallClip.name);
        }

        #endregion

        #region Animation Parameters

        protected override void SetAnimationFlags()
        {
            // Standing states
            standingIsRunning = (Mathf.Abs(InputManager.Movement.x) > ownerPlayer.moveStats.MoveThreshold &&
                                 ownerPlayer.IsRunning) || ownerPlayer.CurrentMovementSpeed >=
                ownerPlayer.moveStats.MaxRunSpeed;
            standingIsWalking = Mathf.Abs(InputManager.Movement.x) > ownerPlayer.moveStats.MoveThreshold &&
                                !ownerPlayer.IsRunning && !ownerPlayer.IsCrouching;
            standingIsWallSliding = ownerPlayer.IsWallSliding;
            standingIsDashing = ownerPlayer.IsDashing;
            standingIsSliding = ownerPlayer.IsSliding;
            standingIsJumping = ownerPlayer.IsJumping;
            standingIsInAir = !ownerPlayer.IsGrounded && !ownerPlayer.IsJumping;
            // InAir states
            inAirIsJumping = ownerPlayer.IsJumping;
            inAirIsDashing = ownerPlayer.IsDashing;
            inAirIsAirDashFalling = ownerPlayer.IsDashFastFalling && !ownerPlayer.IsDashing;
            inAirIsWallSliding = ownerPlayer.IsWallSliding;
        }

        #endregion


        protected override void AnimationInitialize()
        {
            // Initialize states
            StandingState = new POIStandingAnimState(this, StateMachine);
            InAirState = new POIInAirAnimState(this, StateMachine);
            if (!Instance)
            {
                Instance = this;
            }
        }

        protected override void AnimationStart()
        {
            // Set default state
            StateMachine.InitializeDefaultState(StandingState);
        }


        protected override void AnimationUpdate()
        {
            StateMachine.CurrentState.StateUpdate();
        }

        protected override void AnimationFixedUpdate()
        {
            StateMachine.CurrentState.StateFixedUpdate();
            StateMachine.ApplyQueuedStateChange();
            StateMachine.CurrentState.EvaluateStateAnimations();
        }
    }
}
