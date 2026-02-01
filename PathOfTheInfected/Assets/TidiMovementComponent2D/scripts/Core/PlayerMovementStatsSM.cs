using UnityEngine;
using UnityEngine.Serialization;

namespace TidiMovementComponent2D.Core
{


    [CreateAssetMenu(menuName = "Player Movement Stats Object")]
    public class PlayerMovementStatsSm : ScriptableObject
    {
        [Header("MovementCapabilities")]
        public NavMovementCapabilities movementCapabilities;


        [FormerlySerializedAs("MoveThreshold")] [Header("Walk")] [Range(0.0f, 1f)] public float MoveThreshold = 0.25f;

        [FormerlySerializedAs("MaxWalkSpeed")] [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;

        [FormerlySerializedAs("GroundAcceleration")] [Range(0.25f, 50f)] public float GroundAcceleration = 5f;

        [FormerlySerializedAs("GroundDeceleration")] [Range(0.25f, 50f)] public float GroundDeceleration = 20f;

        [FormerlySerializedAs("AirAcceleration")] [Range(0.25f, 50f)] public float AirAcceleration = 5f;

        [FormerlySerializedAs("AirDeceleration")] [Range(0.25f, 50f)] public float AirDeceleration = 5f;

        [FormerlySerializedAs("WallJumpMoveAcceleration")] [Range(0.25f, 50f)] public float WallJumpMoveAcceleration = 5f;

        [FormerlySerializedAs("WallJumpMoveDeceleration")] [Range(0.25f, 50f)] public float WallJumpMoveDeceleration = 5f;

        [FormerlySerializedAs("MaxRunSpeed")] [Header("Run")] [Range(1f, 100f)] public float MaxRunSpeed = 20f;

        [Header("Speed Limit")]
        public float SpeedLimit = 100f;

        [Header("Crouch")]

        [Range(1f, 100f)] public float MaxCrouchSpeed = 5f;

        public Vector2 CrouchedBoxOffset;

        public Vector2 CrouchedBoxSize;


        [Header("Platforms")]
        public bool InheritPlatformMomentum = true;
        [Range(0.0f, 0.5f)]
        public float PlatformMomentumRetentionTime = 0.15f;
        [Range(0.0f, 2f)]
        public float PlatformHorizontalMomentumMultiplier = 1f;
        [Range(0.0f, 2f)]
        public float PlatformVerticalMomentumMultiplier = 1f;
        [Range(0.0f, 100f)]
        public float MaxVerticalBoost = 10f;
        [Range(1f, 5f)]
        public float VerticalLaunchMultiplierOnLaunchExit = 2f;



        [FormerlySerializedAs("DashDirectionMatchesSlopeDirection")] [Header("Slopes")] public bool DashDirectionMatchesSlopeDirection = true;

        [FormerlySerializedAs("CanJumpOnMaxSlopes")] public bool CanJumpOnMaxSlopes;
        [FormerlySerializedAs("JumpFollowSlopesWhenHeadTouching")] public bool JumpFollowSlopesWhenHeadTouching = true;
        [FormerlySerializedAs("DashFollowSlopesWhenHeadTouching")] public bool DashFollowSlopesWhenHeadTouching = true;

        [FormerlySerializedAs("MaxSlopeAngle")] [Range(0.0f, 90f)] public float MaxSlopeAngle = 70f;

        [FormerlySerializedAs("SlideSpeed")] [Range(1f, 100f)] public float SlideSpeed = 30f;

        [FormerlySerializedAs("MatchVisualsToSlope")] [Header("Slope Visual Rotation")] public bool MatchVisualsToSlope = true;

        [FormerlySerializedAs("SlopeAveragedNormalsRayLength")] [Range(0.001f, 2f)] public float SlopeAveragedNormalsRayLength = 1f;

        [FormerlySerializedAs("VisualRaycastWidth")] [Range(0.1f, 5f)] public float VisualRaycastWidth = 1.5f;

        [FormerlySerializedAs("SlopeRotationSpeed")] [Range(0.05f, 100f)] public float SlopeRotationSpeed = 20f;

        [FormerlySerializedAs("MaxVisualRotationAngle")] [Range(0.0f, 70f)] public float MaxVisualRotationAngle = 45f;

        [FormerlySerializedAs("GroundLayer")] [Header("Grounded/Collisions Checks")] public LayerMask GroundLayer;

        [FormerlySerializedAs("KillVerticalVelocityOnHeadBump")] [Header("Head Bump")] public bool KillVerticalVelocityOnHeadBump = true;

        [FormerlySerializedAs("EnableCornerCorrection")] [Header("Corner Correction")] public bool EnableCornerCorrection = true;

        [FormerlySerializedAs("CornerCorrectionWidth")] [Range(0.01f, 1f)] public float CornerCorrectionWidth = 0.4f;

        [FormerlySerializedAs("HorizontalCornerCorrectionHeight")] [Range(0.01f, 1f)] public float HorizontalCornerCorrectionHeight = 0.7f;

        [FormerlySerializedAs("LandingGraceTime")] [Header("Land")] public float LandingGraceTime = 0.15f;
        [FormerlySerializedAs("MinFallDistanceForLand")] public float MinFallDistanceForLand = 0.5f;

        [FormerlySerializedAs("JumpHeight")] [Header("Jump")] public float JumpHeight = 6.5f;

        [FormerlySerializedAs("JumpHeightCompensationFactor")] [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;

        [FormerlySerializedAs("TimeTillJumpApex")] public float TimeTillJumpApex = 0.35f;

        [FormerlySerializedAs("GravityOnReleaseMultiplier")] [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;

        [FormerlySerializedAs("MaxFallSpeed")] public float MaxFallSpeed = 26f;

        [FormerlySerializedAs("NumberOfAirJumpsAllowed")] [Range(0.0f, 5f)] public int NumberOfAirJumpsAllowed = 1;

        [FormerlySerializedAs("ResetJumpsOnWallSlide")] [Header("Reset Jump Options")] public bool ResetJumpsOnWallSlide = true;

        [FormerlySerializedAs("ResetAirJumpsOnMaxSlopeLand")] public bool ResetAirJumpsOnMaxSlopeLand;

        [FormerlySerializedAs("TimeForUpwardsCancel")] [Header("Jump Cut")] [Range(0.02f, 0.3f)]
        public float TimeForUpwardsCancel = 0.027f;

        [FormerlySerializedAs("ApexThreshold")] [Header("Jump Apex Hang Time")] [Range(0.5f, 1f)]
        public float ApexThreshold = 0.97f;

        [FormerlySerializedAs("ApexHangTime")] [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

        [FormerlySerializedAs("JumpBufferTime")] [Header("Jump Buffer")] [Range(0.0f, 1f)]
        public float JumpBufferTime = 0.125f;

        [FormerlySerializedAs("JumpCoyoteTime")] [Header("Jump Coyote Time")] [Range(0.0f, 1f)]
        public float JumpCoyoteTime = 0.1f;

        [FormerlySerializedAs("CanWallSlideFacingAwayFromWall")] [Header("Wall Slide")] public bool CanWallSlideFacingAwayFromWall = true;

        [FormerlySerializedAs("WallSlideSpeed")] [Min(0.1f)] public float WallSlideSpeed = 5f;

        [FormerlySerializedAs("WallSlideDecelerationSpeed")] [Range(0.25f, 50f)] public float WallSlideDecelerationSpeed = 50f;

        [FormerlySerializedAs("MinAngleForWallSlide")] [Range(70f, 90f)] public float MinAngleForWallSlide = 85f;

        [FormerlySerializedAs("MaxAngleForWallSlide")] [Range(90f, 135f)] public float MaxAngleForWallSlide = 95f;

        [FormerlySerializedAs("WallJumpDirection")] [Header("Wall Jump")] public Vector2 WallJumpDirection = new(-20f, 6.5f);

        [FormerlySerializedAs("WallJumpCoyoteTime")] [Range(0.0f, 1f)] public float WallJumpCoyoteTime = 0.125f;

        [FormerlySerializedAs("WallJumpInputBufferDistance")] [Range(0.0f, 0.5f)] public float WallJumpInputBufferDistance = 0.3f;

        [FormerlySerializedAs("WallJumpGravityOnReleaseMultiplier")] [Range(0.01f, 5f)] public float WallJumpGravityOnReleaseMultiplier = 1f;

        [FormerlySerializedAs("CancelDashWhenYouHitCeiling")] [Header("Dash")] public bool CancelDashWhenYouHitCeiling;

        [FormerlySerializedAs("DashTime")] [Range(0.0f, 1f)] public float DashTime = 0.11f;

        [FormerlySerializedAs("DashSpeed")] [Range(1f, 200f)] public float DashSpeed = 40f;

        [FormerlySerializedAs("TimeBtwDashesOnGround")] [Range(0.0f, 1f)] public float TimeBtwDashesOnGround = 0.225f;

        [FormerlySerializedAs("ResetDashOnWallSlide")] public bool ResetDashOnWallSlide = true;

        [FormerlySerializedAs("NumberOfDashes")] [Range(0.0f, 5f)] public int NumberOfDashes = 2;

        [FormerlySerializedAs("DashDiagonallyBias")] [Range(0.0f, 0.5f)] public float DashDiagonallyBias = 0.4f;

        [FormerlySerializedAs("DashBufferTime")] [Range(0.0f, 1f)] public float DashBufferTime = 0.125f;

        [FormerlySerializedAs("DashFreezeTime")] [Header("Dash Feel")] [Range(0.01f, 0.5f)]
        public float DashFreezeTime = 0.06f;

        [FormerlySerializedAs("DashCoyoteTime")] [Header("Dash Coyote Time")] [Range(0.0f, 1f)]
        public float DashCoyoteTime = 0.125f;

        [FormerlySerializedAs("DashGravityOnReleaseMultiplier")] [Header("Dash Cancel Time")] [Range(0.01f, 5f)]
        public float DashGravityOnReleaseMultiplier = 1f;

        [FormerlySerializedAs("DashTimeForUpwardsCancel")] [Range(0.02f, 0.3f)] public float DashTimeForUpwardsCancel = 0.027f;

        [Header("Slopes Run Off")]
        [FormerlySerializedAs("MaxAngleDeltaForRunOff")] public float MaxAngleDeltaForRunOff = 10f;
        [FormerlySerializedAs("MinAngleDeltaForRunOff")] public float MinAngleDeltaForRunOff = 5f;
        [FormerlySerializedAs("SpeedForRunOff")] public float SpeedForRunOff = 5f;
        [FormerlySerializedAs("SlopeCurveDecayRate")] public float SlopeCurveDecayRate = 10f;
        [FormerlySerializedAs("MaxSlopeCurveAccumulation")] public float MaxSlopeCurveAccumulation = 45f;

        [Header("Steps and Vaulting")]
        [FormerlySerializedAs("StepMaxHeight")] public float StepMaxHeight = 0.5f;
        [FormerlySerializedAs("StepDetectionRayWidth")] public float StepDetectionRayWidth = 0.1f;
        [FormerlySerializedAs("VaultMinHeight")] public float VaultMinHeight = 0.2f;
        [FormerlySerializedAs("OnlyVaultWhenRunning")] public bool OnlyVaultWhenRunning = true;
        [FormerlySerializedAs("HorizontalPushDownMaximum")] public float HorizontalPushDownMaximum = 0.2f;

        [FormerlySerializedAs("TrackJumpHeight")] [Header("Trackers")] public bool TrackJumpHeight;

        [FormerlySerializedAs("TrackDashHeight")] public bool TrackDashHeight;

        [FormerlySerializedAs("DebugShowIsGrounded")] [Header("Debug Rays")] public bool DebugShowIsGrounded;

        [FormerlySerializedAs("DebugShowHeadRays")] public bool DebugShowHeadRays;
        [FormerlySerializedAs("DebugShowCornerCorrectionRays")] public bool DebugShowCornerCorrectionRays;
        [FormerlySerializedAs("DebugShowWallHit")] public bool DebugShowWallHit;
        [FormerlySerializedAs("DebugShowWallJumpBufferBox")] public bool DebugShowWallJumpBufferBox;
        [FormerlySerializedAs("DebugShowDescendSlopeRay")] public bool DebugShowDescendSlopeRay;
        [FormerlySerializedAs("DebugShowSlopeNormal")] public bool DebugShowSlopeNormal;
        [FormerlySerializedAs("DebugShowDashAngle")] public bool DebugShowDashAngle;

        [FormerlySerializedAs("ExtraRayDebugDistance")] [Range(0.0f, 1f)] public float ExtraRayDebugDistance = 0.25f;

        [FormerlySerializedAs("TimeScale")] [Range(0.0f, 1f)] public float TimeScale = 1f;

        [FormerlySerializedAs("ShowWalkJumpArc")] [Header("Jump Visualization Tool")] public bool ShowWalkJumpArc = true;

        [FormerlySerializedAs("ShowRunJumpArc")] public bool ShowRunJumpArc = true;
        [FormerlySerializedAs("StopOnCollision")] public bool StopOnCollision = true;
        [FormerlySerializedAs("DrawRight")] public bool DrawRight = true;

        [FormerlySerializedAs("ArcResolution")] [Range(5f, 100f)] public int ArcResolution = 20;

        [FormerlySerializedAs("VisualizationSteps")] [Range(0.0f, 500f)] public int VisualizationSteps = 90;

        public readonly Vector2[] DashDirections = new Vector2[9]
        {
            new(0.0f, 0.0f),
            new(1f, 0.0f),
            new Vector2(1f, 1f).normalized,
            new(0.0f, 1f),
            new Vector2(-1f, 1f).normalized,
            new(-1f, 0.0f),
            new Vector2(-1f, -1f).normalized,
            new(0.0f, -1f),
            new Vector2(1f, -1f).normalized
        };

        public float Gravity { get; private set; }

        public float InitialJumpVelocity { get; private set; }

        public float AdjustedJumpHeight { get; private set; }

        public float WallJumpGravity { get; private set; }

        public float InitialWallJumpVelocity { get; private set; }

        public float AdjustedWallJumpHeight { get; private set; }

        private void OnEnable()
        {
            CalculateJumpStats();
        }

        private void OnValidate()
        {
            CalculateJumpStats();
            Time.timeScale = TimeScale;
        }

        private void CalculateJumpStats()
        {
            AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
            Gravity = (float)-(2.0 * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
            InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
            AdjustedWallJumpHeight = WallJumpDirection.y * JumpHeightCompensationFactor;
            WallJumpGravity = (float)-(2.0 * AdjustedWallJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
            InitialWallJumpVelocity = Mathf.Abs(WallJumpGravity) * TimeTillJumpApex;
        }
    }
}