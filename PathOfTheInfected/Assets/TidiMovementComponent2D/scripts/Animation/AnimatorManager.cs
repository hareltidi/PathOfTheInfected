using TidiMovementComponent2D.Misc;
using UnityEngine;
using UnityEngine.Serialization;

namespace TidiMovementComponent2D.Animation
{
    public class AnimatorManager : TidiAnimInstance
    {
        #region States

        public StandingAnimState StandingState { get; private set; }
        public CrouchingAnimState CrouchingState { get; private set; }

        #endregion

        #region AnimClipsRefs - Standing

        [Header("Animation clips references")] 
        public AnimationClip idleAnimClip;
        public AnimationClip walkAnimClip;
        public AnimationClip runAnimClip;
        public AnimationClip landAnimClip;
        public AnimationClip slideAnimClip;
        public AnimationClip jumpAnimClip;
        public AnimationClip dashAnimClip;
        public AnimationClip takeOffAnimClip;
        public AnimationClip wallSlideAnimClip;
        public AnimationClip fallingAnimClip;

        #endregion

        #region AnimClipsRefs - Crouching

        public AnimationClip crouchIdleAnimClip;
        public AnimationClip crouchWalkAnimClip;

        #endregion

        #region AnimationFlags - Crouching

        [Header("Animation flags")]
        public bool isCrouching;

        [FormerlySerializedAs("CROUCHING_isWalking")]
        public bool crouchingIsWalking;

        #endregion

        #region AnimHashes - Crouching

        public int CrouchingIdleAnim { get; private set; }
        public int CrouchingWalkAnim { get; private set; }

        #endregion

        #region AnimationFlags - Standing


        public bool standingIsRunning;


        public bool standingIsWalking;


        public bool standingIsWallSliding;


        public bool standingIsDashing;


        public bool standingIsAirDashFalling;


        public bool standingIsSliding;


        public bool standingIsJumping;


        public bool standingIsInAir;

        public bool standingIsLanding = false;

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

        #region AnimHashes - Setter

        protected override void SetAnimHashes()
        {
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
            // crouching
            CrouchingIdleAnim = Animator.StringToHash(crouchIdleAnimClip.name);
            CrouchingWalkAnim = Animator.StringToHash(crouchWalkAnimClip.name);
        }

        #endregion

        #region Animation Parameters

        protected override void SetAnimationFlags()
        {
            // Standing states
            standingIsRunning = Mathf.Abs(InputManager.Movement.x) > Player.moveStats.MoveThreshold &&
                                Player.IsRunning;
            standingIsWalking = Mathf.Abs(InputManager.Movement.x) > Player.moveStats.MoveThreshold &&
                                !Player.IsRunning && !Player.IsCrouching;
            standingIsWallSliding = Player.StateMachine.CurrentState == Player.WallSlideState;
            standingIsDashing = Player.IsDashing;
            standingIsAirDashFalling = Player.IsDashFastFalling;
            standingIsSliding = Player.IsSliding;
            standingIsJumping = Player.IsJumping;
            standingIsInAir = !Player.IsGrounded && !Player.IsJumping;
            standingIsLanding = CurrentAnimationHash == StandingLandAnim;

            // crouching states
            isCrouching = Player.IsCrouching;
            crouchingIsWalking = isCrouching && Mathf.Abs(InputManager.Movement.x) > Player.moveStats.MoveThreshold;
        }

        #endregion


        protected override void AnimationInitialize()
        {
            // Initialize states
            StandingState = new StandingAnimState(this, StateMachine);
            CrouchingState = new CrouchingAnimState(this, StateMachine);
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
