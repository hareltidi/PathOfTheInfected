using TidiMovementComponent2D.Animation;
using TidiMovementComponent2D.Misc;
using UnityEngine;
using UnityEngine.Serialization;

namespace PathOfTheInfected.Animation
{
    public class POIAnimInstance : TidiAnimInstance
    {
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
                public AnimationClip fallingAnimClip;
                public AnimationClip standingPunchClip;

                #endregion

          #region AnimationFlags - Standing


                public bool standingIsRunning;


                public bool standingIsWalking;


                public bool standingIsWallSliding;


                public bool standingIsDashing;

                public bool standingIsSliding;


                public bool standingIsJumping;


                public bool standingIsInAir;

                public bool standingIsLanding = false;

                public bool standingIsPunching = false;

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

                public int StandingPunchAnim { get; private set; }

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
        public bool inAirIsPunching;
        public bool inAirIsAirDashFalling;
        public bool inAirIsWallSliding;
        #endregion
        #endregion
        #endregion

        private bool _jumpAnimationRestartRequested;
        private bool _wasJumpingOrWallJumping;
        private int _lastAirJumpsUsed;

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
            StandingPunchAnim = Animator.StringToHash(standingPunchClip.name);
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
            standingIsRunning = Mathf.Abs(InputManager.Movement.x) > Player.moveStats.MoveThreshold &&
                                Player.IsRunning;
            standingIsWalking = Mathf.Abs(InputManager.Movement.x) > Player.moveStats.MoveThreshold &&
                                !Player.IsRunning && !Player.IsCrouching;
            standingIsWallSliding = Player.IsWallSliding;
            standingIsDashing = Player.IsDashing;
            standingIsSliding = Player.IsSliding;
            standingIsJumping = Player.IsJumping;
            standingIsInAir = !Player.IsGrounded && !Player.IsJumping;
            standingIsLanding = CurrentAnimationHash == StandingLandAnim;

            // InAir states
            inAirIsJumping = Player.IsJumping;
            inAirIsDashing = Player.IsDashing;
            inAirIsAirDashFalling = Player.IsDashFastFalling && !Player.IsDashing;
            inAirIsWallSliding =  Player.IsWallSliding;

            // Request jump animation restart only on actual jump starts.
            bool isJumpingOrWallJumping = Player.IsJumping || Player.IsWallJumping;
            bool startedNewJumpState = isJumpingOrWallJumping && !_wasJumpingOrWallJumping;
            bool consumedAirJump = Player.NumberOfAirJumpsUsed > _lastAirJumpsUsed;
            if (startedNewJumpState || consumedAirJump)
            {
                _jumpAnimationRestartRequested = true;
            }

            _wasJumpingOrWallJumping = isJumpingOrWallJumping;
            _lastAirJumpsUsed = Player.NumberOfAirJumpsUsed;
        }

        #endregion


        protected override void AnimationInitialize()
        {
            // Initialize states
            StandingState = new POIStandingAnimState(this, StateMachine);
            InAirState = new POIInAirAnimState(this, StateMachine);
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

        public bool ConsumeJumpAnimationRestartRequest()
        {
            if (!_jumpAnimationRestartRequested)
            {
                return false;
            }

            _jumpAnimationRestartRequested = false;
            return true;
        }
    }
}
