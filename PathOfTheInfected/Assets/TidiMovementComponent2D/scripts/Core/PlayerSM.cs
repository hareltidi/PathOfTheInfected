using System;
using TidiMovementComponent2D.Animation;
using TidiMovementComponent2D.Misc;
using TidiMovementComponent2D.MovingPlatforms;
using TidiMovementComponent2D.Physics;
using TidiMovementComponent2D.States;
using UnityEngine;
using UnityEngine.Serialization;

namespace TidiMovementComponent2D.Core
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(BoxCollider2D)),
     RequireComponent(typeof(MovementControllerSm)), RequireComponent(typeof(GhostTrail)),
     RequireComponent(typeof(TidiAnimInstance))]
    public class PlayerSm : MonoBehaviour
    {
        public enum BufferedJumpType
        {
            None,
            Full,
            Cut
        }

        public static PlayerSm Instance;


        public BoxCollider2D BoxCollider { get; private set; }

        public Vector2 StandingBoxSize { get; private set; }
        public Vector2 StandingBoxOffset { get; private set; }

        public Quaternion OriginalRot { get; private set; }


        [FormerlySerializedAs("MoveStats")][Header("References")] public PlayerMovementStatsSm moveStats;

        [FormerlySerializedAs("Coll")] public Collider2D coll;
        [FormerlySerializedAs("VisualsTransform")] public Transform visualsTransform;

        [FormerlySerializedAs("JumpParticles")][Header("FX")] public GameObject jumpParticles;

        public Transform RespawnPoint;

        [FormerlySerializedAs("SecondJumpParticles")] public GameObject secondJumpParticles;
        [FormerlySerializedAs("LandParticles")] public GameObject landParticles;
        [FormerlySerializedAs("ParticleSpawnTransform")] public Transform particleSpawnTransform;
        [FormerlySerializedAs("TrailRenderer")] public TrailRenderer trailRenderer;
        [FormerlySerializedAs("SpeedParticles")] public ParticleSystem speedParticles;
        [FormerlySerializedAs("WallSlideParticles")] public ParticleSystem wallSlideParticles;

        [FormerlySerializedAs("ShowStateTrailLog")][Header("Debug")] public bool showStateTrailLog;

        [FormerlySerializedAs("ShowEnteredStateDebugLog")] public bool showEnteredStateDebugLog;

        [HideInInspector] public Vector2 velocity;

        private bool _isTrackingDashHeight;
        private bool _isTrackingJumpApex;
        private float _peakDashY;
        private float _peakYPosition;
        private Quaternion _targetRotation = Quaternion.identity;
        private bool _wasGroundedLastFrame;

        public Rigidbody2D Rb { get; private set; }

        public Animator Anim { get; private set; }

        public GhostTrail GhostTrail { get; private set; }

        public PlayerStateMachineSm StateMachine { get; private set; }

        public PlayerIdleStateSm IdleState { get; private set; }

        public PlayerWalkStateSm WalkState { get; private set; }

        public PlayerRunStateSm RunState { get; private set; }

        public PlayerWallSlideStateSm WallSlideState { get; private set; }

        public PlayerDashStateSm DashState { get; private set; }

        public PlayerJumpAscendingStateSm JumpAscendingState { get; private set; }

        public PlayerJumpCutStateSm JumpCutState { get; private set; }

        public PlayerJumpFastFallStateSm JumpFastFallState { get; private set; }

        public PlayerFallingStateSm FallingState { get; private set; }

        public PlayerWallJumpAscendingStateSm WallJumpAscendingState { get; private set; }

        public PlayerWallJumpCutStateSm WallJumpCutState { get; private set; }

        public PlayerWallJumpFastFallStateSm WallJumpFastFallState { get; private set; }

        public PlayerDashFastFallStateSm DashFastFallState { get; private set; }

        public PlayerDashCancelVerticalStateSm DashCancelVerticalState { get; private set; }

        public PlayerSlideStateSm SlideState { get; private set; }

        public PlayerCrouchState CrouchState { get; private set; }

        public bool IsFacingRight { get; private set; }

        public int NumberOfAirJumpsUsed { get; set; }

        public float JumpBufferTimer { get; set; }

        public BufferedJumpType BufferedJumpState { get; set; }

        public float CoyoteTimer { get; private set; }

        public float WallJumpCoyoteTimer { get; set; }

        public int LastWallDirection { get; set; }

        public bool IsAirDashing { get; set; }

        public float DashOnGroundTimer { get; set; }

        public float DashBufferTimer { get; set; }

        public int NumberOfDashesUsed { get; set; }

        public Vector2 DashDirection { get; set; }

        public float DashCoyoteTimer { get; set; }

        public MovementControllerSm Controller { get; private set; }

        public float JumpStartY { get; set; }

        public float DashStartY { get; set; }

        public bool SlideFromDash { get; set; }

        public bool DidHeadBumpSlideThisAirborneState { get; set; }

        public float FallStartY { get; private set; }

        public bool IsInAirborneState => IsJumping || IsFalling || IsFastFalling || IsWallJumping ||
                                         IsWallJumpFastFalling || IsAirDashing || IsDashFastFalling || IsWallSliding;

        public Vector2 StoredPlatformVelocity { get; set; }

        public float PlatformMomentumRetentionTimer { get; set; }

        private VisualInterpolator _visuals;

        public bool IsJumping => StateMachine.CurrentState is PlayerJumpAscendingStateSm ||
                                 StateMachine.CurrentState is PlayerJumpCutStateSm && moveStats.movementCapabilities.canJump;

        public bool IsJumpApexHanging => StateMachine.CurrentState is PlayerJumpAscendingStateSm && velocity.y == 0.0;

        public bool IsFalling => StateMachine.CurrentState is PlayerFallingStateSm;

        public bool IsFastFalling => StateMachine.CurrentState is PlayerJumpFastFallStateSm;

        public bool IsWallJumping => StateMachine.CurrentState is PlayerWallJumpAscendingStateSm ||
                                     StateMachine.CurrentState is PlayerWallJumpCutStateSm && moveStats.movementCapabilities.canWallJump;

        public bool IsWallJumpFastFalling => StateMachine.CurrentState is PlayerWallJumpFastFallStateSm && moveStats.movementCapabilities.canWallJump;

        public bool IsDashing => StateMachine.CurrentState is PlayerDashStateSm && moveStats.movementCapabilities.canDash;

        public bool IsDashFastFalling => StateMachine.CurrentState is PlayerDashFastFallStateSm && moveStats.movementCapabilities.canDash;

        public bool IsWallSliding => StateMachine.CurrentState is PlayerWallSlideStateSm && moveStats.movementCapabilities.canWallSlide;

        public bool IsCrouching => StateMachine.CurrentState is PlayerCrouchState && moveStats.movementCapabilities.canCrouch;

        public bool IsSliding => StateMachine.CurrentState is PlayerSlideStateSm;

        public bool IsLanding;

        public bool IsRunning => InputManager.RunIsHeld && moveStats.movementCapabilities.canRun;

        public bool BumpedHead => Controller.State.IsHittingCeiling;

        public bool IsGrounded => Controller.State.IsGrounded;

        public bool IsTouchingWall => Controller.State.IsAgainstWall;

        public float ComboSpeedMultiplier { get; set; } = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            _visuals = GetComponentInChildren<VisualInterpolator>();
            StateMachine = new PlayerStateMachineSm();
            IdleState = new PlayerIdleStateSm(this, StateMachine);
            WalkState = new PlayerWalkStateSm(this, StateMachine);
            RunState = new PlayerRunStateSm(this, StateMachine);
            WallSlideState = new PlayerWallSlideStateSm(this, StateMachine);
            DashState = new PlayerDashStateSm(this, StateMachine);
            JumpAscendingState = new PlayerJumpAscendingStateSm(this, StateMachine);
            JumpCutState = new PlayerJumpCutStateSm(this, StateMachine);
            JumpFastFallState = new PlayerJumpFastFallStateSm(this, StateMachine);
            FallingState = new PlayerFallingStateSm(this, StateMachine);
            WallJumpAscendingState = new PlayerWallJumpAscendingStateSm(this, StateMachine);
            WallJumpCutState = new PlayerWallJumpCutStateSm(this, StateMachine);
            WallJumpFastFallState = new PlayerWallJumpFastFallStateSm(this, StateMachine);
            DashCancelVerticalState = new PlayerDashCancelVerticalStateSm(this, StateMachine);
            DashFastFallState = new PlayerDashFastFallStateSm(this, StateMachine);
            SlideState = new PlayerSlideStateSm(this, StateMachine);
            CrouchState = new PlayerCrouchState(this, StateMachine);
            IsFacingRight = true;
            OriginalRot = transform.rotation;
        }

        private void Start()
        {
            Rb = GetComponent<Rigidbody2D>();
            BoxCollider = GetComponent<BoxCollider2D>();
            StandingBoxSize = BoxCollider.size;
            StandingBoxOffset = BoxCollider.offset;
            Anim = GetComponentInChildren<Animator>();
            GhostTrail = GetComponent<GhostTrail>();
            Controller = GetComponent<MovementControllerSm>();
            Controller.OnCrush += HandleCrush;
            wallSlideParticles.gameObject.SetActive(false);
            StateMachine.InitializeDefaultState(IdleState);
        }

        private void Update()
        {
            StateMachine.CurrentState.StateUpdate();
            HandleSpeedParticles();
        }

        private void FixedUpdate()
        {
            Controller.PollSensors(velocity * Time.fixedDeltaTime);
            HandleInputCommands();
            StateMachine.CurrentState.StateFixedUpdate();
            HandleFallThresholdTracking();
            HandleDebugHeightTracking();
            CalculateTargetRotation();
            ApplyConstantForce();
            PreventWallStick();
            PreventCeilingStick();
            CalculateRunOffMomentum();
            ApplyVelocity();
            StateMachine.ApplyQueuedStateChange();
            _visuals.UpdatePhysicsState();
        }

        private void LateUpdate()
        {
            if (!moveStats.MatchVisualsToSlope) return;

            RotateVisualTarget(Time.deltaTime);
        }

        private void OnEnable()
        {
           
        }

        private void OnDisable()
        {
            Controller.OnCrush -= HandleCrush;
        }

        private void OnDrawGizmos()
        {
            if (moveStats.ShowWalkJumpArc)
            {
                DrawJumpArc(moveStats.MaxWalkSpeed, Color.white);
            }
            if (!moveStats.ShowRunJumpArc) return;

            DrawJumpArc(moveStats.MaxRunSpeed, Color.red);
        }

        private void HandleFallThresholdTracking()
        {
            bool flag1 = Controller.State.SlopeAngle >= (double)moveStats.MinAngleForWallSlide && Controller.State.SlopeAngle <= (double)moveStats.MaxAngleForWallSlide;


            bool flag2 = Controller.State.IsGrounded && !flag1;
            if (_wasGroundedLastFrame && !flag2)
            {
                FallStartY = Rb.position.y;
            }

            _wasGroundedLastFrame = flag2;
        }

        public void ApplyVelocity()
        {
            if (Controller.IsSliding)
            {
                velocity.y = Mathf.Clamp(velocity.y, -moveStats.SlideSpeed, 50f);
            }
            else if (IsDashing)
            {
                velocity.y = Mathf.Clamp(velocity.y, -50f, 50f);
            }
            else
            {
                float num = moveStats.MaxFallSpeed;
                if (PlatformMomentumRetentionTimer > 0.0 && StoredPlatformVelocity.y < -num)
                    num = Mathf.Abs(StoredPlatformVelocity.y);
                velocity.y = Mathf.Clamp(velocity.y, -num, 100f);
            }
            Controller.Move(velocity * Time.fixedDeltaTime);
        }

        public void ChangeVerticalVelocity(float changeAmount)
        {
            velocity.y = changeAmount;
        }

        public void IncrementVerticalVelocity(float incrementAmount)
        {
            velocity.y += incrementAmount;
        }

        public void IncrementHorizontalVelocity(float incrementAmount)
        {
            velocity.x += incrementAmount;
        }

        public void ChangeWholeVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }

        private void HandleInputCommands()
        {
            while (InputCommandManager.CommandQueue.Count > 0)
            {
                StateMachine.OnInputCommand(InputCommandManager.CommandQueue.Dequeue());
                StateMachine.ApplyQueuedStateChange();
            }
        }

        private void HandleSpeedParticles()
        {
            if (Mathf.Abs(velocity.x) >= moveStats.MaxRunSpeed - 2.0)
            {
                if (speedParticles.isPlaying) return;
                speedParticles.Play();
            }
            else
            {
                if (!speedParticles.isPlaying) return;
                speedParticles.Stop();
            }
        }

        public void Move(float acceleration, float deceleration, Vector2 moveInput, float speedOverride = 0.0f)
        {
            if (IsDashing) return;

            float num = speedOverride > 0 ? speedOverride : moveStats.MaxWalkSpeed;
            num *= ComboSpeedMultiplier;
            if (Mathf.Abs(moveInput.x) >= (double)moveStats.MoveThreshold)
            {
                TurnCheck(moveInput);
                var b = Mathf.Sign(moveInput.x) * num;
                var t = Mathf.Clamp01(acceleration * Time.fixedDeltaTime);
                velocity.x = Mathf.Lerp(velocity.x, b, t);
                if (Mathf.Abs(velocity.x - b) > 0.0099999997764825821) return;
                velocity.x = b;
            }
            else
            {
                velocity.x = Mathf.Lerp(velocity.x, 0.0f, Mathf.Clamp01(deceleration * Time.fixedDeltaTime));
                if (Mathf.Abs(velocity.x) > 0.0099999997764825821)
                    return;
                velocity.x = 0.0f;
            }
        }

        public void TurnCheck(Vector2 moveInput)
        {
            if (IsFacingRight && moveInput.x < 0.0)
            {
                Turn(false);
            }
            else
            {
                if (IsFacingRight || moveInput.x <= 0.0)  return;
                Turn(true);
            }
        }

        private void PreventWallStick()
        {
            if (!Controller.IsTouchingWall() || ((velocity.x <= 0.0 || Controller.State.WallDirection != 1) &&
                                                 (velocity.x >= 0.0 || Controller.State.WallDirection != -1))) return;
            if (Controller.IsStep(velocity)) return;

            velocity.x = 0.0f;
        }

        private void PreventCeilingStick()
        {
            if (!Controller.BumpedHead() || (double)velocity.y <= 0.0)
                return;
            bool flag = moveStats.JumpFollowSlopesWhenHeadTouching && (double)Controller.State.CeilingAngle > 0.0;
            if (IsDashing || flag)
                return;
            ChangeVerticalVelocity(0.0f);
        }

        private void CalculateRunOffMomentum()
        {
            if (Controller.IsGrounded() || !Controller.State.WasGroundedLastFrame || IsJumping || IsWallJumping || Controller.PlatformFromLastFrame == null || (double)PlatformMomentumRetentionTimer <= 0.0)
                return;
            Vector2 platformVelocity = StoredPlatformVelocity;
            velocity.x += platformVelocity.x * moveStats.PlatformHorizontalMomentumMultiplier;
            if ((double)platformVelocity.y < 0.0)
            {
                velocity.y += platformVelocity.y;
            }
            else
            {
                if (!Controller.PlatformFromLastFrame.LaunchVerticallyOnExit)
                    return;
                velocity.y += platformVelocity.y * moveStats.VerticalLaunchMultiplierOnLaunchExit;
            }
        }

        private void Turn(bool turnRight)
        {
            IsFacingRight = turnRight;
            var num = IsFacingRight ? 1 : -1;
            visualsTransform.localScale = new Vector3(Mathf.Abs(visualsTransform.localScale.x) * num,
                visualsTransform.localScale.y, visualsTransform.localScale.z);
        }

        private void CalculateTargetRotation()
        {
            if (!moveStats.MatchVisualsToSlope)
                return;
            _targetRotation =
                Quaternion.AngleAxis(
                    Mathf.Clamp(Vector2.SignedAngle(Vector2.up, (Vector3)Controller.State.AveragedVisualNormal),
                        -moveStats.MaxVisualRotationAngle, moveStats.MaxVisualRotationAngle), Vector3.forward);
        }

        private void RotateVisualTarget(float timeStep)
        {
            visualsTransform.rotation = Quaternion.Slerp(visualsTransform.rotation, _targetRotation, moveStats.SlopeRotationSpeed * Time.deltaTime);
            float z = visualsTransform.rotation.eulerAngles.z;
            if ((double)z > 180.0)
                z -= 360f;
            float f = Mathf.Abs(z * ((float)Math.PI / 180f));
            double num1 = (double)coll.bounds.size.x / 2.0;
            float num2 = coll.bounds.size.y / 2f;
            double num3 = (double)Mathf.Sin(f);
            _visuals.PivotOffset = new Vector3(0.0f, -((float)(num1 * num3 + (double)num2 * (double)Mathf.Cos(f)) - num2), 0.0f);
        }

        public bool HasLanded()
        {
            if (!Controller.State.IsGrounded ||
                (Controller.State.SlopeAngle < (double)moveStats.MinAngleForWallSlide ? 0 :
                    Controller.State.SlopeAngle <= (double)moveStats.MaxAngleForWallSlide ? 1 : 0) != 0 ||
                !IsInAirborneState || velocity.y > 0.0) return false;

            StopWallSliding();
            ResetDashes();
            ResetDashValues();
            ReplenishJumps();
            DidHeadBumpSlideThisAirborneState = false;
            trailRenderer.emitting = false;
            if (moveStats.ResetAirJumpsOnMaxSlopeLand |
                (Controller.State.SlopeAngle <= (double)moveStats.MaxSlopeAngle) | Controller.State.IsAgainstWall)
                ReplenishJumps();
            if (FallStartY - (double)Rb.position.y >= moveStats.MinFallDistanceForLand)
            {
                if (IsDashFastFalling && Controller.State.IsGrounded)
                {
                    return true;
                }

                SpawnJumpParticles(landParticles);
                return true;
            }

            return false;
        }

        public void ApplyConstantForce()
        {
            if (Controller.IsSliding || !Controller.State.IsGrounded || IsSlideableSlope(Controller.State.SlopeAngle) ||
                velocity.y > 0.0)
                return;
            ChangeVerticalVelocity(-2f);
        }

        public bool CanJump()
        {
            if (!moveStats.movementCapabilities.canJump) return false;
            if (IsCrouching)
            {
                if (!CanUnCrouch()) return false;
            }
            var flag = moveStats.CanJumpOnMaxSlopes || Controller.State.SlopeAngle <= (double)moveStats.MaxSlopeAngle;
            return ((IsJumping ? 0 : Controller.State.IsGrounded ? 1 : CoyoteTimer > 0.0 ? 1 : 0) & (flag ? 1 : 0)) != 0;
        }

        public void ConsumeCoyoteTime()
        {
            CoyoteTimer = 0.0f;
        }

        public void ReplenishJumps()
        {
            NumberOfAirJumpsUsed = 0;
        }

        public bool ShouldWallSlide()
        {
            if (!moveStats.movementCapabilities.canWallSlide) return false;
            var flag1 = Controller.State.IsAgainstWall;
            var flag2 = Controller.State.WallAngle >= (double)moveStats.MinAngleForWallSlide &&
                        Controller.State.WallAngle <= (double)moveStats.MaxAngleForWallSlide;
            if (flag1 && !moveStats.CanWallSlideFacingAwayFromWall &&
                (IsFacingRight ? 1 : -1) != Controller.State.WallDirection)
                flag1 = false;
            return !IsDashing & flag1 & flag2 && !Controller.State.IsGrounded && velocity.y < 0.0 && !IsWallSliding;
        }

        public bool ShouldStopWallSliding()
        {
            if (!IsWallSliding || Controller.State.IsAgainstWall)
                return false;
            if ((double)PlatformMomentumRetentionTimer > 0.0)
            {
                IncrementVerticalVelocity(StoredPlatformVelocity.y);
                velocity.x += StoredPlatformVelocity.x;
            }
            return true;
        }

        public void StopWallSliding()
        {
            if (!IsWallSliding)
                return;
            Anim.SetBool("isWallSliding", false);
        }

        public bool AttemptWallJumpBuffer()
        {
            if (moveStats.WallJumpInputBufferDistance <= 0.0) return false;

            int[] numArray = moveStats.CanWallSlideFacingAwayFromWall ? new[] { 1, -1 } : new[] { IsFacingRight ? 1 : -1 };
            foreach (float num1 in numArray)
            {
                var direction = Vector2.right * num1;
                var bounds = coll.bounds;
                var center = (Vector2)bounds.center;
                bounds = coll.bounds;
                var size = (Vector2)bounds.size;
                var raycastHit2D = Physics2D.BoxCast(center, size, 0.0f, direction,
                    moveStats.WallJumpInputBufferDistance, moveStats.GroundLayer);
                if (moveStats.DebugShowWallJumpBufferBox)
                {
                    var duration = 0.5f;
                    var num2 = raycastHit2D ? raycastHit2D.distance : moveStats.WallJumpInputBufferDistance;
                    var flag = false;
                    if (raycastHit2D)
                    {
                        var num3 = Mathf.Round(Vector2.Angle(raycastHit2D.normal, Vector2.up));
                        if (num3 >= (double)moveStats.MinAngleForWallSlide &&
                            num3 <= (double)moveStats.MaxAngleForWallSlide)
                            flag = true;
                    }

                    var color = flag ? Color.cyan : Color.red;
                    DrawDebugBox(center + direction * num2, size, color, duration);
                    Debug.DrawLine(center, center + direction * num2, color, duration);
                }

                if (raycastHit2D)
                {
                    var num4 = Mathf.Round(Vector2.Angle(raycastHit2D.normal, Vector2.up));
                    if ((num4 < (double)moveStats.MinAngleForWallSlide ? 0 : num4 <= (double)moveStats.MaxAngleForWallSlide ? 1 : 0) != 0)
                    {
                        LastWallDirection = -(int)Mathf.Sign(raycastHit2D.normal.x);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CanStandardWallJump()
        {
            if (!moveStats.movementCapabilities.canWallJump) return false;
            var flag = Controller.State.WallAngle >= (double)moveStats.MinAngleForWallSlide &&
                       Controller.State.WallAngle <= (double)moveStats.MaxAngleForWallSlide;
            return Controller.State.IsAgainstWall && (flag || IsWallSliding) && JumpBufferTimer > 0.0;
        }

        public bool CanPostBufferWallJump()
        {
            return WallJumpCoyoteTimer > 0.0 && JumpBufferTimer > 0.0 && moveStats.movementCapabilities.canWallJump;
        }

        public void ResetDashes()
        {
            NumberOfDashesUsed = 0;
        }

        public void HalfResetDashes()
        {
            NumberOfDashesUsed = Mathf.Max(0, NumberOfDashesUsed - 1);
        }

        public bool CanUnCrouch()
        {
            float crouchTopY = moveStats.CrouchedBoxOffset.y + (moveStats.CrouchedBoxSize.y / 2);
            float standTopY = StandingBoxOffset.y + (StandingBoxSize.y / 2);
            float castDistance = standTopY - crouchTopY;
            if (castDistance <= 0) return true;
            Vector2 centerOfCrouchedCollider = (Vector2)transform.position + moveStats.CrouchedBoxOffset;
            Vector2 origin = centerOfCrouchedCollider + (Vector2.up * (moveStats.CrouchedBoxSize.y / 2f));
            float skinWidthMargin = 0.02f;
            Vector2 boxSize = new(StandingBoxSize.x - (skinWidthMargin * 2), skinWidthMargin);
            bool hit = Physics2D.BoxCast(origin, boxSize, 0f, Vector2.up, castDistance, moveStats.GroundLayer);
            return IsCrouching && !hit;
        }

        public void ResetDashValues()
        {
            DashOnGroundTimer = -0.01f;
            DashDirection = Vector2.zero;
        }

        public void DashLand()
        {
            DashOnGroundTimer = -0.01f;
        }

        public bool CanDash()
        {
            if (!moveStats.movementCapabilities.canDash || IsCrouching && !CanUnCrouch())
            {
                return false;
            }
            return (Controller.State.IsGrounded ? 1 : DashCoyoteTimer > 0.0 ? 1 : 0) != 0 && DashOnGroundTimer < 0.0 &&
                   !IsDashing;
        }

        public bool CanAirDash()
        {
            if (Controller.State.IsGrounded || IsDashing || NumberOfDashesUsed >= moveStats.NumberOfDashes || !moveStats.movementCapabilities.canDash)
                return false;
            IsAirDashing = true;
            return true;
        }

        public void JumpTimers()
        {
            JumpBufferTimer -= Time.fixedDeltaTime;
            if (JumpBufferTimer < -(double)Time.fixedDeltaTime)
                BufferedJumpState = BufferedJumpType.None;
            HandleCoyoteTimer(Time.fixedDeltaTime);
        }

        private void HandleCoyoteTimer(float timeStep)
        {
            if (Controller.State.IsGrounded && !Controller.IsSliding)
            {
                CoyoteTimer = moveStats.JumpCoyoteTime;
            }
            else
            {
                CoyoteTimer -= timeStep;
            }
        }

        public void HandleWallJumpDirectionAndPostBufferTimer()
        {
            if (!Controller.State.IsGrounded && Controller.State.IsAgainstWall)
            {
                LastWallDirection = Controller.State.WallDirection;
            }
            WallJumpCoyoteTimer -= Time.fixedDeltaTime;
        }

        public void DashTimers()
        {
            HandleDashOnGroundTimer(Time.fixedDeltaTime);
            DashBufferTimer -= Time.fixedDeltaTime;
        }

        private void HandleDashOnGroundTimer(float timeStep)
        {
            if (!Controller.State.IsGrounded || Controller.IsSliding) return;
            DashOnGroundTimer -= timeStep;
        }


        public void ManagePlatformMomentumTimer()
        {
            if (this.Controller.LastKnownPlatform != null)
            {
                StoredPlatformVelocity = this.Controller.LastKnownPlatform.GetVelocity();
                PlatformMomentumRetentionTimer = moveStats.PlatformMomentumRetentionTime;
            }
            else
            {
                if ((double)PlatformMomentumRetentionTimer <= 0.0)
                    return;
                PlatformMomentumRetentionTimer -= Time.fixedDeltaTime;
                if ((double)PlatformMomentumRetentionTimer > 0.0)
                    return;
                StoredPlatformVelocity = Vector2.zero;
            }
        }

        public void SpawnJumpParticles(GameObject particlesToSpawn)
        {
            Instantiate(particlesToSpawn, particleSpawnTransform.position, Quaternion.identity);
        }

        public void HandleCrush(Transform pusher)
        {
            _visuals.ForceTeleport(RespawnPoint.position);
        }

        public bool IsSlideableSlope(float slopeAngle)
        {
            return slopeAngle >= (double)moveStats.MaxSlopeAngle && slopeAngle < (double)moveStats.MinAngleForWallSlide;
        }

        public bool IsWalkableSlope(float angle)
        {
            return angle <= (double)moveStats.MaxSlopeAngle && angle < (double)moveStats.MinAngleForWallSlide;
        }

        public bool IsWallSlideable(float angle)
        {
            return angle >= (double)moveStats.MinAngleForWallSlide && angle <= (double)moveStats.MaxAngleForWallSlide;
        }

        private void DrawJumpArc(float moveSpeed, Color gizmoColor)
        {
            Vector2 vector21 = default;
            ref var local = ref vector21;
            var bounds = coll.bounds;
            var x1 = (double)bounds.center.x;
            bounds = coll.bounds;
            var y1 = (double)bounds.min.y;
            local = new Vector2((float)x1, (float)y1);
            var vector22 = vector21;
            var x2 = !moveStats.DrawRight ? -moveSpeed : moveSpeed;
            var vector23 = new Vector2(x2, moveStats.InitialJumpVelocity);
            Gizmos.color = gizmoColor;
            var num1 = 2f * moveStats.TimeTillJumpApex / moveStats.ArcResolution;
            for (var index = 0; index < moveStats.VisualizationSteps; ++index)
            {
                var num2 = index * num1;
                var num3 = moveStats.Gravity * moveStats.GravityOnReleaseMultiplier;
                Vector2 vector24;
                if (num2 < (double)moveStats.TimeTillJumpApex)
                {
                    vector24 = vector23 * num2 + 0.5f * new Vector2(0.0f, moveStats.Gravity) * num2 * num2;
                }
                else if (num2 < moveStats.TimeTillJumpApex + (double)moveStats.ApexHangTime)
                {
                    var num4 = num2 - moveStats.TimeTillJumpApex;
                    vector24 = vector23 * moveStats.TimeTillJumpApex +
                                0.5f * new Vector2(0.0f, moveStats.Gravity) * moveStats.TimeTillJumpApex *
                                moveStats.TimeTillJumpApex + new Vector2(x2, 0.0f) * num4;
                }
                else
                {
                    var num5 = num2 - (moveStats.TimeTillJumpApex + moveStats.ApexHangTime);
                    var vector25 = vector23 * moveStats.TimeTillJumpApex +
                                    0.5f * new Vector2(0.0f, moveStats.Gravity) * moveStats.TimeTillJumpApex *
                                    moveStats.TimeTillJumpApex + new Vector2(x2, 0.0f) * moveStats.ApexHangTime;
                    var y2 = num3 * (num5 * num5);
                    vector24 = vector25 + (new Vector2(x2, 0.0f) * num5 + 0.5f * new Vector2(0.0f, y2));
                }

                var vector26 = vector21 + vector24;
                if (moveStats.StopOnCollision)
                {
                    var raycastHit2D = Physics2D.Raycast(vector22, vector26 - vector22,
                        Vector2.Distance(vector22, vector26), moveStats.GroundLayer);
                    if (raycastHit2D.collider != null)
                    {
                        Gizmos.DrawLine(vector22, raycastHit2D.point);
                        break;
                    }
                }

                Gizmos.DrawLine(vector22, vector26);
                vector22 = vector26;
            }
        }

        private void DrawDebugBox(Vector2 center, Vector2 size, Color color, float duration)
        {
            var vector21 = size / 2f;
            var vector22 = center + new Vector2(-vector21.x, vector21.y);
            var vector23 = center + new Vector2(vector21.x, vector21.y);
            var vector24 = center + new Vector2(-vector21.x, -vector21.y);
            var vector25 = center + new Vector2(vector21.x, -vector21.y);
            Debug.DrawLine(vector22, vector23, color, duration);
            Debug.DrawLine(vector23, vector25, color, duration);
            Debug.DrawLine(vector25, vector24, color, duration);
            Debug.DrawLine(vector24, vector22, color, duration);
        }

        private void HandleDebugHeightTracking()
        {
            if (_isTrackingJumpApex)
            {
                if (Rb.position.y > (double)_peakYPosition)
                    _peakYPosition = Rb.position.y;
                if (velocity.y <= 0.0)
                {
                    StopJumpTracking("Apex Reached");
                }
            }

            if (Rb.position.y <= (double)_peakDashY) return;
            _peakDashY = Rb.position.y;
        }

        public void StartJumpTracking()
        {
            if (!moveStats.TrackJumpHeight) return;

            _isTrackingJumpApex = true;
            _peakYPosition = JumpStartY;
        }

        public void StopJumpTracking(string reason)
        {
            if (!_isTrackingJumpApex) return;
            _isTrackingJumpApex = false;
            var num = _peakYPosition - JumpStartY;
            Debug.Log($"[Jump Debugger] {reason}! Max height was: {num} units.");
        }

        public void StartDashTracking()
        {
            if (!moveStats.TrackDashHeight) return;

            _isTrackingDashHeight = true;
            _peakDashY = DashStartY;
        }

        public void StopDashTracking(string reason)
        {
            if (!_isTrackingDashHeight) return;

            _isTrackingDashHeight = false;
            var num = _peakDashY - DashStartY;
            Debug.Log($"[Dash Debugger] {reason}! Max dash height: {num} units.");
        }

        public void DisplayCurrentDashHeight()
        {
            Debug.Log($"[Dash Debugger]! Max dash height: {_peakDashY - DashStartY} units.");
        }
    }
}