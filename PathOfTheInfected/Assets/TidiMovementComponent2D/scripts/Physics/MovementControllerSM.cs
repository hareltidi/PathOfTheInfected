using System;
using TidiMovementComponent2D.Core;
using TidiMovementComponent2D.MovingPlatforms;
using UnityEngine;

namespace TidiMovementComponent2D.Physics
{
    [RequireComponent(typeof(PlayerSm))]
    public class MovementControllerSm : MonoBehaviour, IPushable
    {
        public const float CollisionPadding = 0.015f;
        private const float AIRBORNE_ANGLE_MEMORY = -999f;

        [Range(2f, 100f)] public int NumOfHorizontalRays = 4;

        [Range(2f, 100f)] public int NumOfVerticalRays = 4;

        public int NumOfVerticalRaysForVisualNormals = 9;

        [Header("Sensors")] [SerializeField] private float _verticalProbeDistance = 0.1f;

        [SerializeField] private float _horizontalProbeDistance = 0.1f;

        [Header("Safety")] [SerializeField] private float safetyGraceDuration = 0.1f;

        private BoxCollider2D _coll;
        private bool _forceAirborneNextFrame;
        private float _horizontalRaySpace;
        private CollisionStateSm _internalState;
        private bool _isCornerCorrectingThisFrame;
        private bool _isHorizontalCornerCorrectingThisFrame;
        private float _lastSafetyGroundFixedTime = float.NegativeInfinity;
        private RaycastHit2D _lastSafetyGroundHit;
        private PlayerMovementStatsSm _moveStats;
        private readonly Collider2D[] _overlapBuffer = new Collider2D[1];
        private PlayerSm _player;
        private Vector2 _pushAmountThisFrame;
        private Rigidbody2D _rb;
        private float _rearCornerSlopeAngle;
        private float _slopeCurveAccumulator;
        private float _verticalRaySpace;
        private bool _wasPushedThisFrame;
        public IVelocityInheritable LastKnownPlatform;
        public Action <Transform> OnCrush;
        public RaycastCorners RayCastCorners;

        public bool IsSliding => _internalState.IsOnSlope &&
                                 (double)_internalState.SlopeAngle > (double)_moveStats.MaxSlopeAngle;

        public bool IsClimbingSlope { get; private set; }

        public bool WasClimbingSlopeLastFrame { get; private set; }

        public bool IsDescendingSlope { get; private set; }

        public float SlopeAngle { get; private set; }

        public Vector2 SlopeNormal { get; private set; }

        public float LastLandingTime { get; set; }

        public int FaceDirection { get; private set; }

        public CollisionStateSm State { get; private set; }

        public IVelocityInheritable PlatformFromLastFrame { get; private set; }

        public bool IsOnPlatform => LastKnownPlatform != null;

        private void Awake()
        {
            _coll = GetComponent<BoxCollider2D>();
            _rb = GetComponent<Rigidbody2D>();
            _player = GetComponent<PlayerSm>();
            _moveStats = _player.moveStats;
            FaceDirection = 1;
        }

        private void Start()
        {
            CalculateRaySpacing();
        }

        public void ApplyExternalPush(Vector2 pushAmount, Transform pusher)
        {
            var normalized = pushAmount.normalized;
            var magnitude = pushAmount.magnitude;
            if (magnitude > 1.0 / 1000.0)
            {
                var raycastHit2D = Physics2D.BoxCast(_coll.bounds.center, _coll.bounds.size - Vector3.one * 0.015f * 2f,
                    0.0f, normalized, magnitude, (int)_moveStats.GroundLayer);
                if (raycastHit2D && (!(pusher != null) ? 0 : raycastHit2D.transform == pusher ? 1 : 0) == 0)
                {
                    var flag = false;
                    if (Mathf.Abs(pushAmount.x) > 1.0 / 1000.0)
                    {
                        var directionX = Mathf.Sign(pushAmount.x);
                        float stepHeight;
                        if (GetStepInfo(raycastHit2D.distance, directionX, out stepHeight))
                        {
                            _rb.position += Vector2.up * (float)(stepHeight + 0.014999999664723873 + 1.0 / 1000.0);
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        var onCrush = OnCrush;
                        if (onCrush != null)
                        {
                            onCrush(pusher);
                        }
                    }
                }
            }

            _rb.position += pushAmount;
            _wasPushedThisFrame = true;
            _pushAmountThisFrame = pushAmount;
        }

        public void PollSensors(Vector2 moveDelta)
        {
            _internalState.Reset();
            var lastKnownPlatform = LastKnownPlatform;
            PlatformFromLastFrame = LastKnownPlatform;
            LastKnownPlatform = null;
            UpdateRaycastCorners();
            if (moveDelta.x != 0.0)
                FaceDirection = (int)Mathf.Sign(moveDelta.x);
            IVelocityInheritable foundPlatform1;
            HorizontalProbes(moveDelta, lastKnownPlatform, out foundPlatform1);
            CeilingProbes(moveDelta);
            IVelocityInheritable foundPlatform2;
            if (_forceAirborneNextFrame)
            {
                _internalState.IsGrounded = false;
                _forceAirborneNextFrame = false;
                foundPlatform2 = null;
            }
            else
            {
                GroundProbes(moveDelta, lastKnownPlatform, out foundPlatform2);
            }

            LastKnownPlatform = !_internalState.IsGrounded ? !_internalState.IsAgainstWall ? null : foundPlatform1 : foundPlatform2;
            if (_internalState.IsHittingCeiling && DetectHeadCornerCorrection(moveDelta))
            {
                _internalState.IsHittingCeiling = false;
            }
            if (_moveStats.MatchVisualsToSlope)
            {
                DetectSlopeNormalsForVisuals(moveDelta);
            }
            State = _internalState;
        }

        public void Move(Vector2 velocity)
        {
            var zero = Vector2.zero;
            if (IsOnPlatform)
            {
                var velocity1 = LastKnownPlatform.GetVelocity();
                var vector2 = velocity1 * Time.fixedDeltaTime;
                if (vector2.x != 0.0 && LastKnownPlatform.NeedsFuturePositionBoxcastCheck)
                {
                    var num1 = Mathf.Sign(vector2.x);
                    var distance = Mathf.Abs(vector2.x) + 0.015f;
                    var raycastHit2D = Physics2D.BoxCast(_coll.bounds.center,
                        _coll.bounds.size - Vector3.one * 0.015f * 2f, 0.0f, Vector3.right * num1, distance,
                        (int)_moveStats.GroundLayer);
                    if (raycastHit2D)
                    {
                        var num2 = Mathf.Max(0.0f, raycastHit2D.distance - 0.015f);
                        velocity1.x = num2 * num1 / Time.fixedDeltaTime;
                    }
                }

                if (_internalState.IsAgainstWall && _internalState.IsGrounded)
                {
                    var x = (float)-_internalState.WallDirection;
                    if (Vector2.Dot(velocity1, new Vector2(x, 0.0f)) < -0.0099999997764825821)
                        velocity1.x = 0.0f;
                }

                if (_internalState.IsGrounded &&
                    Vector2.Dot(velocity1, _internalState.SlopeNormal) > 0.0099999997764825821)
                    velocity1.y = 0.0f;
                if (_wasPushedThisFrame)
                {
                    if (Mathf.Abs(_pushAmountThisFrame.x) > 0.0)
                        velocity1.x = 0.0f;
                    if (Mathf.Abs(_pushAmountThisFrame.y) > 0.0)
                        velocity1.y = 0.0f;
                }

                _rb.position += velocity1 * Time.fixedDeltaTime;
            }

            UpdateRaycastCorners();
            ResetCollisionStates();
            if (velocity.y <= 0.0 && !_player.IsDashing && !WasClimbingSlopeLastFrame)
                DescendSlope(ref velocity);
            ApplyHorizontalCornerCorrection(ref velocity);
            ApplyHeadCornerCorrection(ref velocity);
            ResolveHorizontalMovement(ref velocity);
            ResolveVerticalMovement(ref velocity);
            _rb.MovePosition(_rb.position + velocity);
            _wasPushedThisFrame = false;
            _pushAmountThisFrame = Vector2.zero;
        }

        private void GroundProbes(
            Vector2 moveDelta,
            IVelocityInheritable lastKnownPlatform,
            out IVelocityInheritable foundPlatform)
        {
            foundPlatform = null;
            var distance1 = _verticalProbeDistance + 0.015f;
            if (lastKnownPlatform != null && lastKnownPlatform.ProbesShouldLead)
            {
                var num = Mathf.Abs(lastKnownPlatform.GetVelocity().y * Time.fixedDeltaTime);
                distance1 += num;
            }

            var num1 = float.MaxValue;
            var flag1 = false;
            var flag2 = false;
            var raycastHit2D1 = new RaycastHit2D();
            var num2 = 0.0f;
            var vector2_1 = Vector2.zero;
            if (lastKnownPlatform != null && lastKnownPlatform.ProbesShouldLead)
                vector2_1 = lastKnownPlatform.GetVelocity() * Time.fixedDeltaTime;
            if (moveDelta.y <= 0.0)
            {
                num2 = moveDelta.x;
                if (_internalState.IsAgainstWall || _internalState.IsAgainstSteepSlope)
                    num2 = 0.0f;
            }

            for (var index = 0; index < NumOfVerticalRays; ++index)
            {
                var vector2_2 = RayCastCorners.BottomLeft + Vector2.right * (_verticalRaySpace * index + num2) +
                                vector2_1;
                var raycastHit2D2 = Physics2D.Raycast(vector2_2, Vector2.down, distance1, (int)_moveStats.GroundLayer);
                if (_moveStats.DebugShowIsGrounded)
                {
                    var color = raycastHit2D2 ? Color.cyan : Color.red;
                    Debug.DrawRay(vector2_2, Vector2.down * distance1, color);
                }

                if (raycastHit2D2)
                {
                    var flag3 = Mathf.Round(Vector2.Angle(raycastHit2D2.normal, Vector2.up)) <
                                (double)_moveStats.MaxSlopeAngle;
                    if (!flag1)
                    {
                        num1 = raycastHit2D2.distance;
                        raycastHit2D1 = raycastHit2D2;
                        flag1 = true;
                        flag2 = flag3;
                    }
                    else if (!flag2 & flag3)
                    {
                        num1 = raycastHit2D2.distance;
                        raycastHit2D1 = raycastHit2D2;
                        flag2 = true;
                    }
                    else if (flag2 == flag3 && raycastHit2D2.distance < (double)num1)
                    {
                        num1 = raycastHit2D2.distance;
                        raycastHit2D1 = raycastHit2D2;
                    }
                }
                else if (IsClimbingSlope || (IsDescendingSlope && !IsOnPlatform))
                {
                    _internalState.IsGrounded = true;
                    _internalState.SlopeAngle = SlopeAngle;
                    _internalState.SlopeNormal = SlopeNormal;
                    if ((double)_internalState.SlopeAngle > 0.0099999997764825821)
                        _internalState.IsOnSlope = true;
                }
            }

            if (flag1)
            {
                var component = raycastHit2D1.collider.GetComponent<IVelocityInheritable>();
                if (component != null)
                    foundPlatform = component;
                var angle = Mathf.Round(Vector2.Angle(raycastHit2D1.normal, Vector2.up));
                var flag4 = Mathf.Abs(raycastHit2D1.normal.y) < (double)Mathf.Epsilon;
                if (((!_player.IsWallSlideable(angle) ? 0 : !flag4 ? 1 : 0) &
                     (Mathf.Sign(raycastHit2D1.normal.x) != (double)FaceDirection ? true ? 1 : 0 :
                         _moveStats.CanWallSlideFacingAwayFromWall ? 1 : 0)) != 0)
                {
                    _internalState.IsAgainstWall = true;
                    _internalState.WallAngle = angle;
                    _internalState.WallDirection = (int)-(double)Mathf.Sign(raycastHit2D1.normal.x);
                }
                else
                {
                    _internalState.IsGrounded = true;
                    _internalState.SlopeAngle = angle;
                    _internalState.SlopeNormal = raycastHit2D1.normal;
                    if (angle > 0.0099999997764825821)
                        _internalState.IsOnSlope = true;
                }
            }
            else
            {
                if (WasClimbingSlopeLastFrame)
                {
                    var flag5 = false;
                    var raycastHit2D3 = new RaycastHit2D();
                    for (var index = 0; index < NumOfVerticalRays; ++index)
                    {
                        var raycastHit2D4 =
                            Physics2D.Raycast(RayCastCorners.BottomLeft + Vector2.right * (_verticalRaySpace * index),
                                Vector2.down, distance1 * 5f, (int)_moveStats.GroundLayer);
                        if (raycastHit2D4)
                        {
                            raycastHit2D3 = raycastHit2D4;
                            var distance2 = (double)raycastHit2D3.distance;
                            flag5 = true;
                            break;
                        }
                    }

                    if (flag5)
                    {
                        var component = raycastHit2D3.collider.GetComponent<IVelocityInheritable>();
                        if (component != null)
                            foundPlatform = component;
                        var angle = Mathf.Round(Vector2.Angle(raycastHit2D3.normal, Vector2.up));
                        if (!_player.IsWallSlideable(angle))
                        {
                            _lastSafetyGroundFixedTime = Time.fixedTime;
                            _lastSafetyGroundHit = raycastHit2D3;
                            _internalState.IsGrounded = true;
                            _internalState.SlopeAngle = angle;
                            _internalState.SlopeNormal = raycastHit2D3.normal;
                            if (angle > 0.0099999997764825821)
                                _internalState.IsOnSlope = true;
                        }
                    }
                }

                if (Time.fixedTime - (double)_lastSafetyGroundFixedTime <= safetyGraceDuration)
                {
                    var angle = Mathf.Round(Vector2.Angle(_lastSafetyGroundHit.normal, Vector2.up));
                    if (!_player.IsWallSlideable(angle))
                    {
                        _internalState.IsGrounded = true;
                        _internalState.SlopeAngle = angle;
                        _internalState.SlopeNormal = _lastSafetyGroundHit.normal;
                        if (angle > 0.0099999997764825821)
                            _internalState.IsOnSlope = true;
                    }
                }
            }

            if (!_moveStats.DebugShowSlopeNormal)
                return;
            Debug.DrawRay(new Vector2(_coll.bounds.center.x, _coll.bounds.min.y),
                (Vector3)(_internalState.SlopeNormal * (_moveStats.ExtraRayDebugDistance * 3f)), Color.yellow);
        }

        private void CeilingProbes(Vector2 moveDelta)
        {
            if (moveDelta.y < 0.0)
                return;
            var distance = _verticalProbeDistance + 0.015f;
            for (var index = 0; index < NumOfVerticalRays; ++index)
            {
                var vector2 = RayCastCorners.TopLeft + Vector2.right * (_verticalRaySpace * index);
                var raycastHit2D1 = Physics2D.Raycast(vector2, Vector2.up, distance, (int)_moveStats.GroundLayer);
                if (raycastHit2D1)
                {
                    var num1 = Mathf.Round(Vector2.Angle(raycastHit2D1.normal, Vector2.down));
                    if (num1 <= 85.0 && ((index == 0 ? 1 : index == NumOfVerticalRays - 1 ? 1 : 0) == 0 ||
                                         raycastHit2D1.distance != 0.0 || num1 != 0.0))
                    {
                        float num2;
                        if (raycastHit2D1.distance == 0.0)
                        {
                            var raycastHit2D2 = Physics2D.Raycast(vector2 + Vector2.down * 0.015f * 2f, Vector2.up,
                                0.0449999981f, (int)_moveStats.GroundLayer);
                            if (raycastHit2D2)
                            {
                                num2 = Mathf.Round(Vector2.Angle(raycastHit2D2.normal, Vector2.down));
                                _internalState.CeilingNormal = raycastHit2D2.normal;
                            }
                            else
                            {
                                num2 = 0.0f;
                                _internalState.CeilingNormal = Vector2.down;
                            }
                        }
                        else
                        {
                            num2 = Mathf.Round(Vector2.Angle(raycastHit2D1.normal, Vector2.down));
                            _internalState.CeilingNormal = raycastHit2D1.normal;
                        }

                        _internalState.IsHittingCeiling = true;
                        if (num2 > (double)_internalState.CeilingAngle)
                        {
                            _internalState.CeilingAngle = num2;
                            _internalState.CeilingNormal = raycastHit2D1.normal;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if (_moveStats.DebugShowHeadRays)
                {
                    var num = raycastHit2D1 ? raycastHit2D1.distance : distance + _moveStats.ExtraRayDebugDistance;
                    var color = raycastHit2D1 ? Color.cyan : Color.red;
                    if (index == 0 || index == NumOfVerticalRays - 1)
                        color = raycastHit2D1 ? Color.green : Color.magenta;
                    Debug.DrawRay(vector2, Vector2.up * num, color);
                }
            }
        }

        private void HorizontalProbes(
            Vector2 moveDelta,
            IVelocityInheritable lastKnownPlatform,
            out IVelocityInheritable foundPlatform)
        {
            foundPlatform = null;
            var distance = Mathf.Abs(moveDelta.x) + 0.015f;
            if (distance < (double)_horizontalProbeDistance)
                distance = _horizontalProbeDistance;
            if (lastKnownPlatform != null && lastKnownPlatform.ProbesShouldLead)
            {
                var num = Mathf.Abs(lastKnownPlatform.GetVelocity().x * Time.fixedDeltaTime);
                distance += num;
            }

            var lhs = Vector2.zero;
            if (lastKnownPlatform != null && lastKnownPlatform.ProbesShouldLead)
                lhs = lastKnownPlatform.GetVelocity() * Time.fixedDeltaTime;
            for (var index = 0; index < NumOfHorizontalRays; ++index)
            {
                var vector2_1 = RayCastCorners.BottomLeft + Vector2.up * (_horizontalRaySpace * index);
                if (Vector2.Dot(lhs, Vector2.left) > 0.0)
                    vector2_1 += lhs;
                var raycastHit2D1 = Physics2D.Raycast(vector2_1, Vector2.left, distance, (int)_moveStats.GroundLayer);
                if (_moveStats.DebugShowWallHit)
                    Debug.DrawRay(vector2_1, Vector2.left * distance, raycastHit2D1 ? Color.cyan : Color.red);
                var flag = false;
                if (raycastHit2D1)
                {
                    var num = Mathf.Round(Vector2.Angle(raycastHit2D1.normal, Vector2.up));
                    if (_player.IsSlideableSlope(num))
                        _internalState.IsAgainstSteepSlope = true;
                    if (_player.IsWallSlideable(num))
                    {
                        _internalState.IsAgainstWall = true;
                        _internalState.WallDirection = -1;
                        _internalState.WallAngle = num;
                        flag = true;
                    }

                    var component = raycastHit2D1.collider.GetComponent<IVelocityInheritable>();
                    if (component != null)
                        foundPlatform = component;
                }

                var vector2_2 = RayCastCorners.BottomRight + Vector2.up * (_horizontalRaySpace * index);
                if (Vector2.Dot(lhs, Vector2.right) > 0.0)
                    vector2_2 += lhs;
                var raycastHit2D2 = Physics2D.Raycast(vector2_2, Vector2.right, distance, (int)_moveStats.GroundLayer);
                if (_moveStats.DebugShowWallHit)
                    Debug.DrawRay(vector2_2, Vector2.right * distance, raycastHit2D2 ? Color.cyan : Color.red);
                if (raycastHit2D2)
                {
                    var num = Mathf.Round(Vector2.Angle(raycastHit2D2.normal, Vector2.up));
                    if (_player.IsSlideableSlope(num))
                        _internalState.IsAgainstSteepSlope = true;
                    if (_player.IsWallSlideable(num))
                    {
                        _internalState.IsAgainstWall = true;
                        _internalState.WallDirection = 1;
                        _internalState.WallAngle = num;
                        flag = true;
                    }

                    var component = raycastHit2D2.collider.GetComponent<IVelocityInheritable>();
                    if (component != null)
                        foundPlatform = component;
                }

                if (flag)
                    break;
            }
        }

        private void ResetCollisionStates()
        {
            WasClimbingSlopeLastFrame = IsClimbingSlope;
            IsClimbingSlope = false;
            SlopeAngle = 0.0f;
            SlopeNormal = Vector2.zero;
            IsDescendingSlope = false;
            _isCornerCorrectingThisFrame = false;
            _isHorizontalCornerCorrectingThisFrame = false;
            if (_internalState.IsGrounded)
                return;
            _slopeCurveAccumulator = 0.0f;
        }

        private void ResolveVerticalMovement(ref Vector2 velocity)
        {
            if (velocity.y >= 0.0)
            {
                if (_isCornerCorrectingThisFrame)
                    return;
                var distance = Mathf.Abs(velocity.y) + 0.015f;
                for (var index = 0; index < NumOfVerticalRays; ++index)
                {
                    var origin = RayCastCorners.TopLeft + Vector2.right * (_verticalRaySpace * index);
                    var raycastHit2D1 = Physics2D.Raycast(origin, Vector2.up, distance, (int)_moveStats.GroundLayer);
                    if (raycastHit2D1)
                    {
                        var num = Mathf.Round(Vector2.Angle(raycastHit2D1.normal, Vector2.down));
                        if (raycastHit2D1.distance == 0.0)
                        {
                            var vector2 = Vector2.zero;
                            if (index == 0)
                                vector2 = Vector2.right * 0.015f * 2f;
                            else if (index == NumOfVerticalRays - 1)
                                vector2 = Vector2.left * 0.015f * 2f;
                            if (vector2 != Vector2.zero)
                            {
                                var raycastHit2D2 = Physics2D.Raycast(origin + vector2, Vector2.up, distance,
                                    (int)_moveStats.GroundLayer);
                                num = !raycastHit2D2
                                    ? 90f
                                    : Mathf.Round(Vector2.Angle(raycastHit2D2.normal, Vector2.down));
                            }
                        }

                        if (num <= 85.0)
                        {
                            if (raycastHit2D1.distance == 0.0)
                            {
                                velocity.y = 0.0f;
                            }
                            else
                            {
                                velocity.y = raycastHit2D1.distance - 0.015f;
                                distance = raycastHit2D1.distance;
                            }
                        }
                    }
                }
            }

            var distance1 = Mathf.Abs(velocity.y) + 0.015f;
            if (LastKnownPlatform != null && LastKnownPlatform.ProbesShouldLead)
                distance1 += Mathf.Abs(LastKnownPlatform.GetVelocity().y * Time.fixedDeltaTime);
            var num1 = float.MaxValue;
            var raycastHit2D3 = new RaycastHit2D();
            var flag1 = false;
            var flag2 = false;
            for (var index = 0; index < NumOfVerticalRays; ++index)
            {
                var raycastHit2D4 =
                    Physics2D.Raycast(
                        RayCastCorners.BottomLeft + Vector2.right * (_verticalRaySpace * index + velocity.x),
                        Vector2.down, distance1, (int)_moveStats.GroundLayer);
                if (raycastHit2D4)
                {
                    var flag3 = Mathf.Round(Vector2.Angle(raycastHit2D4.normal, Vector2.up)) <
                                (double)_moveStats.MaxSlopeAngle;
                    if (raycastHit2D4.distance < 0.014999999664723873 || flag3 || (!IsSliding &&
                            (!_internalState.IsGrounded ||
                             (double)_internalState.SlopeAngle >= (double)_moveStats.MaxSlopeAngle)))
                    {
                        if (!flag1)
                        {
                            num1 = raycastHit2D4.distance;
                            raycastHit2D3 = raycastHit2D4;
                            flag1 = true;
                            flag2 = flag3;
                        }
                        else if (!flag2 & flag3)
                        {
                            num1 = raycastHit2D4.distance;
                            raycastHit2D3 = raycastHit2D4;
                            flag2 = true;
                        }
                        else if (flag2 == flag3 && raycastHit2D4.distance < (double)num1)
                        {
                            num1 = raycastHit2D4.distance;
                            raycastHit2D3 = raycastHit2D4;
                        }
                    }
                }
            }

            if (flag1)
            {
                var f1 = (float)((raycastHit2D3.distance - 0.014999999664723873) * -1.0);
                if ((f1 <= 0.0 || f1 > 0.015999998897314072) && Mathf.Abs(f1) <= 1.0 / 1000.0)
                    f1 = 0.0f;
                if (velocity.y <= 0.0)
                {
                    var num2 = raycastHit2D3.distance - 0.015f;
                    if (IsSliding && num2 > 0.0 && (double)_internalState.SlopeAngle < 89.9000015258789)
                    {
                        var f2 = _internalState.SlopeAngle * ((float)Math.PI / 180f);
                        var num3 = num2 / Mathf.Tan(f2);
                        velocity.x += num3 * Mathf.Sign(_internalState.SlopeNormal.x);
                    }

                    velocity.y = f1;
                }
                else if (IsClimbingSlope)
                {
                    velocity.y += f1;
                }
            }

            if (!IsClimbingSlope)
                return;
            var num4 = Mathf.Sign(velocity.x);
            var distance2 = Mathf.Abs(velocity.x) + 0.015f;
            var raycastHit2D5 =
                Physics2D.Raycast(
                    (num4 == -1.0 ? RayCastCorners.BottomLeft : RayCastCorners.BottomRight) + Vector2.up * velocity.y,
                    Vector2.right * num4, distance2, (int)_moveStats.GroundLayer);
            if (!raycastHit2D5)
                return;
            var num5 = Mathf.Round(Vector2.Angle(raycastHit2D5.normal, Vector2.up));
            if (num5 == (double)SlopeAngle)
                return;
            velocity.x = (raycastHit2D5.distance - 0.015f) * num4;
            SlopeAngle = num5;
            SlopeNormal = raycastHit2D5.normal;
        }

        private void ResolveHorizontalMovement(ref Vector2 velocity)
        {
            if (_isHorizontalCornerCorrectingThisFrame)
                return;
            var x = velocity.x;
            var directionX = Mathf.Sign(velocity.x);
            if (velocity.x == 0.0)
                directionX = FaceDirection;
            var distance = Mathf.Abs(velocity.x) + 0.015f;
            if (Mathf.Abs(velocity.x) < 0.014999999664723873)
                distance = 0.03f;
            if (LastKnownPlatform != null && LastKnownPlatform.ProbesShouldLead)
                distance += Mathf.Abs(LastKnownPlatform.GetVelocity().x * Time.fixedDeltaTime);
            for (var index = 0; index < NumOfHorizontalRays; ++index)
            {
                var origin = (directionX == -1.0 ? RayCastCorners.BottomLeft : RayCastCorners.BottomRight) +
                             Vector2.up * (_horizontalRaySpace * index);
                var hit = Physics2D.Raycast(origin, Vector2.right * directionX, distance, (int)_moveStats.GroundLayer);
                if (hit)
                {
                    var num = Mathf.Round(Vector2.Angle(hit.normal, Vector2.up));
                    if (index <= NumOfHorizontalRays / 2 && _player.IsWalkableSlope(num) && num > 0.0)
                    {
                        ClimbSlope(ref velocity, num, hit.normal, x);
                    }
                    else if (!IsClimbingSlope || !_player.IsWalkableSlope(num))
                    {
                        if (index == 0 && hit.distance > 1.0 / 1000.0 && _internalState.IsGrounded &&
                            AttemptStepUp(hit, ref velocity, directionX, x))
                            break;
                        if (index == 0)
                        {
                            var raycastHit2D = Physics2D.Raycast(origin + Vector2.up * _horizontalRaySpace,
                                Vector2.right * directionX, distance, (int)_moveStats.GroundLayer);
                            if (raycastHit2D)
                            {
                                var slopeAngle = Mathf.Round(Vector2.Angle(raycastHit2D.normal, Vector2.up));
                                if (slopeAngle <= (double)_moveStats.MaxSlopeAngle)
                                {
                                    ClimbSlope(ref velocity, slopeAngle, raycastHit2D.normal, x);
                                    continue;
                                }
                            }
                        }

                        if ((num <= (double)_moveStats.MaxSlopeAngle ? 0 :
                                num < (double)_moveStats.MinAngleForWallSlide ? 1 : 0) != 0)
                        {
                            velocity.x = (hit.distance - 0.015f) * directionX;
                            if (IsClimbingSlope)
                                velocity.y = Mathf.Tan(SlopeAngle * ((float)Math.PI / 180f)) * Mathf.Abs(velocity.x);
                        }
                        else if (!IsSliding || _player.IsWallSlideable(num))
                        {
                            velocity.x = (hit.distance - 0.015f) * directionX;
                            distance = hit.distance;
                            if (IsClimbingSlope)
                                velocity.y = Mathf.Tan(SlopeAngle * ((float)Math.PI / 180f)) * Mathf.Abs(velocity.x);
                        }
                    }
                }
            }
        }

        private void UpdateRaycastCorners()
        {
            var bounds = _coll.bounds;
            bounds.Expand(-0.03f);
            RayCastCorners.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            RayCastCorners.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            RayCastCorners.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            RayCastCorners.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void CalculateRaySpacing()
        {
            var bounds = _coll.bounds;
            bounds.Expand(-0.03f);
            _horizontalRaySpace = bounds.size.y / (NumOfHorizontalRays - 1);
            _verticalRaySpace = bounds.size.x / (NumOfVerticalRays - 1);
        }

        private void ClimbSlope(
            ref Vector2 velocity,
            float slopeAngle,
            Vector2 slopeNormal,
            float originalInputX)
        {
            var num1 = Mathf.Abs(originalInputX);
            var num2 = Mathf.Sin(slopeAngle * ((float)Math.PI / 180f)) * num1;
            if (velocity.y <= (double)num2)
            {
                velocity.y = num2;
                velocity.x = Mathf.Cos(slopeAngle * ((float)Math.PI / 180f)) * num1 * Mathf.Sign(velocity.x);
                IsClimbingSlope = true;
                SlopeAngle = slopeAngle;
                SlopeNormal = slopeNormal;
                _rearCornerSlopeAngle = slopeAngle;
                _slopeCurveAccumulator = 0.0f;
            }
            else
            {
                if (Mathf.Abs(Vector2.Dot(velocity.normalized, slopeNormal)) >= 0.05000000074505806 ||
                    !_player.IsDashing)
                    return;
                IsClimbingSlope = true;
                SlopeAngle = slopeAngle;
                SlopeNormal = slopeNormal;
                _rearCornerSlopeAngle = slopeAngle;
                _slopeCurveAccumulator = 0.0f;
            }
        }

        private void DescendSlope(ref Vector2 velocity)
        {
            Vector2 vector2 = FaceDirection == -1.0 ? RayCastCorners.BottomRight : RayCastCorners.BottomLeft;
            var num1 = Mathf.Tan(_moveStats.MinAngleForWallSlide * ((float)Math.PI / 180f)) * Mathf.Abs(velocity.x);
            var distance1 = Mathf.Max(Mathf.Abs(velocity.y) + 0.015f + num1, Mathf.Abs(velocity.y), 0.03f);
            var hit1 = Physics2D.Raycast(vector2, Vector2.down, distance1, (int)_moveStats.GroundLayer);
            if (_moveStats.DebugShowDescendSlopeRay)
            {
                var isDescendingSlope = IsDescendingSlope;
                var color = Color.blue;
                if (hit1 & isDescendingSlope)
                    color = Color.green;
                else if (hit1 && IsSliding)
                    color = Color.yellow;
                else if (hit1 && !isDescendingSlope)
                    color = Color.red;
                var num2 = hit1
                    ? hit1.distance + _moveStats.ExtraRayDebugDistance
                    : _moveStats.ExtraRayDebugDistance * 5f;
                Debug.DrawRay(vector2, -Vector2.up * num2, color);
            }

            if (hit1)
            {
                var slopeAngle = Mathf.Round(Vector2.Angle(hit1.normal, Vector2.up));
                var num3 = Mathf.Abs(velocity.x) / Time.fixedDeltaTime;
                var flag1 = Time.time - (double)LastLandingTime < (double)_moveStats.LandingGraceTime;
                var flag2 = _rearCornerSlopeAngle == -999.0;
                var num4 = flag2 ? 0.0f : slopeAngle - _rearCornerSlopeAngle;
                var num5 = (int)Mathf.Sign(hit1.normal.x);
                var flag3 = (num5 == -1 && _player.IsFacingRight) || (num5 == 1 && !_player.IsFacingRight);
                var num6 = _player.IsSlideableSlope(slopeAngle) ? 1 : 0;
                var flag4 = slopeAngle >= (double)_moveStats.MinAngleForWallSlide;
                var flag5 = num6 != 0 || (flag4 && !flag3);
                if (!flag2 && num4 > 0.10000000149011612)
                    _slopeCurveAccumulator += num4;
                else
                    _slopeCurveAccumulator -= _moveStats.SlopeCurveDecayRate * Time.fixedDeltaTime;
                _slopeCurveAccumulator = Mathf.Clamp(_slopeCurveAccumulator, 0.0f, 180f);
                if (flag1 & flag2)
                {
                    if (flag5)
                    {
                        SlideDownMaxSlope(hit1, ref velocity);
                        _rearCornerSlopeAngle = slopeAngle;
                        return;
                    }

                    if (slopeAngle <= (double)_moveStats.MaxSlopeAngle)
                    {
                        ApplySlopeStick(ref velocity, slopeAngle, hit1);
                        _rearCornerSlopeAngle = slopeAngle;
                    }
                    else
                    {
                        _rearCornerSlopeAngle = slopeAngle;
                    }
                }
                else if (!flag2 && num4 >= (double)_moveStats.MaxAngleDeltaForRunOff)
                {
                    _rearCornerSlopeAngle = -999f;
                    _forceAirborneNextFrame = true;
                }
                else if (!flag2 && (num4 >= (double)_moveStats.MinAngleDeltaForRunOff ||
                                    _slopeCurveAccumulator > (double)_moveStats.MaxSlopeCurveAccumulation))
                {
                    if (num3 >= (double)_moveStats.SpeedForRunOff)
                    {
                        _rearCornerSlopeAngle = -999f;
                        _forceAirborneNextFrame = true;
                        _slopeCurveAccumulator = 0.0f;
                    }
                    else
                    {
                        if (flag5)
                        {
                            SlideDownMaxSlope(hit1, ref velocity);
                            _rearCornerSlopeAngle = slopeAngle;
                            return;
                        }

                        if (slopeAngle <= (double)_moveStats.MaxSlopeAngle)
                        {
                            ApplySlopeStick(ref velocity, slopeAngle, hit1);
                            _rearCornerSlopeAngle = slopeAngle;
                        }
                        else
                        {
                            _rearCornerSlopeAngle = slopeAngle;
                        }
                    }
                }
                else if (flag2 && !flag1)
                {
                    var num7 = Mathf.Abs(velocity.x) < 0.0099999997764825821 ? FaceDirection : Mathf.Sign(velocity.x);
                    var flag6 = Mathf.Sign(hit1.normal.x) == (double)num7;
                    if (!flag6)
                        _slopeCurveAccumulator = 0.0f;
                    if (flag5 & flag6)
                    {
                        SlideDownMaxSlope(hit1, ref velocity);
                        _rearCornerSlopeAngle = slopeAngle;
                        return;
                    }

                    _rearCornerSlopeAngle = slopeAngle > (double)_moveStats.MaxSlopeAngle ? -999f : slopeAngle;
                }
                else if (slopeAngle > (double)_moveStats.MaxSlopeAngle)
                {
                    var num8 = Mathf.Abs(velocity.x) < 0.0099999997764825821 ? FaceDirection : Mathf.Sign(velocity.x);
                    if (Mathf.Sign(hit1.normal.x) != (double)num8)
                    {
                        _slopeCurveAccumulator = 0.0f;
                        _rearCornerSlopeAngle = slopeAngle;
                    }
                    else
                    {
                        if (slopeAngle < (double)_moveStats.MinAngleForWallSlide)
                        {
                            SlideDownMaxSlope(hit1, ref velocity);
                            _rearCornerSlopeAngle = slopeAngle;
                            return;
                        }

                        _rearCornerSlopeAngle = slopeAngle;
                    }
                }
                else
                {
                    ApplySlopeStick(ref velocity, slopeAngle, hit1);
                    _rearCornerSlopeAngle = slopeAngle;
                }
            }
            else
            {
                _slopeCurveAccumulator = 0.0f;
                _rearCornerSlopeAngle = -999f;
            }

            var flag = false;
            var distance2 = Mathf.Max(Mathf.Abs(velocity.y) + 0.015f, _verticalProbeDistance);
            for (var index = 0; index < NumOfVerticalRays; ++index)
            {
                var raycastHit2D =
                    Physics2D.Raycast(RayCastCorners.BottomLeft + Vector2.right * (_verticalRaySpace * index),
                        Vector2.down, distance2, (int)_moveStats.GroundLayer);
                if (raycastHit2D && Mathf.Round(Vector2.Angle(raycastHit2D.normal, Vector2.up)) <=
                    (double)_moveStats.MaxSlopeAngle)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
                return;
            var hit2 = Physics2D.Raycast(RayCastCorners.BottomLeft, Vector2.down, Mathf.Abs(velocity.y) + 0.015f,
                (int)_moveStats.GroundLayer);
            var hit3 = Physics2D.Raycast(RayCastCorners.BottomRight, Vector2.down, Mathf.Abs(velocity.y) + 0.015f,
                (int)_moveStats.GroundLayer);
            if (!(hit2 ^ hit3))
                return;
            SlideDownMaxSlope(hit2, ref velocity);
            SlideDownMaxSlope(hit3, ref velocity);
        }

        public void UpdateSlopeMemory()
        {
            _slopeCurveAccumulator = 0.0f;
            _rearCornerSlopeAngle = -999f;
            LastLandingTime = Time.time;
        }

        private void ApplySlopeStick(ref Vector2 moveAmount, float slopeAngle, RaycastHit2D hit)
        {
            if (_player.IsWallSlideable(slopeAngle))
            {
                if ((Mathf.Sign(hit.normal.x) != (double)FaceDirection ? 1 :
                        _moveStats.CanWallSlideFacingAwayFromWall ? 1 : 0) != 0)
                    return;
                SlideDownMaxSlope(hit, ref moveAmount);
            }

            var num1 = Mathf.Tan(slopeAngle * ((float)Math.PI / 180f)) * Mathf.Abs(moveAmount.x);
            if (hit.distance - 0.014999999664723873 > num1 || slopeAngle <= 1.0 / 1000.0)
                return;
            var num2 = Mathf.Abs(moveAmount.x);
            var num3 = Mathf.Sin(slopeAngle * ((float)Math.PI / 180f)) * num2;
            moveAmount.x = Mathf.Cos(slopeAngle * ((float)Math.PI / 180f)) * num2 * Mathf.Sign(moveAmount.x);
            moveAmount.y -= num3;
            SlopeAngle = slopeAngle;
            IsDescendingSlope = true;
            SlopeNormal = hit.normal;
        }

        private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 velocity)
        {
            if (!hit)
                return;
            var num1 = Mathf.Round(Vector2.Angle(hit.normal, Vector2.up));
            var num2 = (int)Mathf.Sign(hit.normal.x);
            var flag1 = (num2 == -1 && _player.IsFacingRight) || (num2 == 1 && !_player.IsFacingRight);
            var num3 = num1 <= (double)_moveStats.MaxSlopeAngle ? 0 :
                num1 < (double)_moveStats.MinAngleForWallSlide ? 1 : 0;
            var flag2 = num1 >= (double)_moveStats.MinAngleForWallSlide;
            if (num3 == 0 && (!flag2 || flag1))
                return;
            var vector2_1 = new Vector2(hit.normal.y, -hit.normal.x);
            if (vector2_1.y > 0.0)
                vector2_1 = -vector2_1;
            vector2_1.Normalize();
            var magnitude = velocity.magnitude;
            var vector2_2 = vector2_1 * magnitude;
            vector2_2.y -= hit.distance - 0.015f;
            velocity.x = vector2_2.x;
            velocity.y = vector2_2.y;
            SlopeAngle = num1;
            SlopeNormal = hit.normal;
            IsDescendingSlope = true;
        }

        private bool GetStepInfo(float hitDistance, float directionX, out float stepHeight)
        {

            stepHeight = 0.0f;

            if (!_moveStats.movementCapabilities.canVault) return false;
            Vector2 origin1 = directionX == -1.0 ? RayCastCorners.BottomLeft : RayCastCorners.BottomRight;
            origin1.y += _moveStats.StepMaxHeight;
            var distance = hitDistance + _moveStats.StepDetectionRayWidth;
            int groundLayer = (int)_moveStats.GroundLayer;

            if (Physics2D.Raycast(origin1, Vector2.right * directionX, distance, groundLayer)) return false;


            var origin2 = origin1;
            origin2.x += directionX * distance;

            var raycastHit2D = Physics2D.Raycast(origin2, Vector2.down, _moveStats.StepMaxHeight + CollisionPadding, groundLayer);
            if (!raycastHit2D || !_player.IsWalkableSlope(Mathf.Round(Vector2.Angle(raycastHit2D.normal, Vector2.up))))
            {
                return false;
            }

            var y = _coll.bounds.size.y;
            var origin3 = raycastHit2D.point + Vector2.up * CollisionPadding;
            const float smallOffset = 0.01f;
            float upCastDistance = Mathf.Max(0f, y - CollisionPadding - smallOffset);
            if (upCastDistance > Mathf.Epsilon)
            {
                if (Physics2D.Raycast(origin3 + Vector2.up * smallOffset, Vector2.up, upCastDistance, groundLayer)) return false;

            }

            Vector2 origin4 = directionX == -1.0 ? RayCastCorners.TopLeft : RayCastCorners.TopRight;
            var vector2 = origin3 + Vector2.up * (y - CollisionPadding) - origin4;
            var magnitude = vector2.magnitude;
            if (magnitude > Mathf.Epsilon)
            {
                var dir = vector2.normalized;
                if (Physics2D.Raycast(origin4 + dir * smallOffset, dir, Mathf.Max(0f, magnitude - smallOffset), groundLayer)) return false;

            }

            float num = directionX == -1.0 ? RayCastCorners.BottomLeft.y : RayCastCorners.BottomRight.y;
            stepHeight = raycastHit2D.point.y - num;
            return true;
        }

        private bool AttemptStepUp(
            RaycastHit2D hit,
            ref Vector2 velocity,
            float directionX,
            float originalVelocityX)
        {
            float stepHeight;
            if (GetStepInfo(hit.distance, directionX, out stepHeight))
            {
                var f = (hit.distance + _moveStats.StepDetectionRayWidth) * directionX;
                var num = f;
                if (Mathf.Abs(originalVelocityX) > Mathf.Abs(f))
                {
                    num = originalVelocityX;
                }
                if ((stepHeight <= (double)_moveStats.VaultMinHeight) | !_moveStats.OnlyVaultWhenRunning | (_player.IsRunning || _player.IsDashing))
                {
                    velocity.y = stepHeight + 1f / 1000f;
                    velocity.x = num;
                    return true;
                }
            }
            return false;
        }

        public bool IsStep(Vector2 currentVelocity)
        {
            var directionX = Mathf.Sign(currentVelocity.x);
            if (Mathf.Abs(currentVelocity.x) < 1.0 / 1000.0)
                directionX = FaceDirection;
            var distance = (float)(Mathf.Abs(currentVelocity.x) * (double)Time.fixedDeltaTime + 0.014999999664723873);
            var raycastHit2D =
                Physics2D.Raycast(directionX == -1.0 ? RayCastCorners.BottomLeft : RayCastCorners.BottomRight,
                    Vector2.right * directionX, distance, (int)_moveStats.GroundLayer);
            return raycastHit2D && GetStepInfo(raycastHit2D.distance, directionX, out var _);
        }

        private bool CalculateHeadCornerCorrection(Vector3 velocity, out float correctionAmount)
        {
            correctionAmount = 0.0f;
            if (velocity.y <= 0.0 || !_moveStats.EnableCornerCorrection)
                return false;
            var distance1 = Mathf.Max(Mathf.Abs(velocity.y) + 0.015f, _verticalProbeDistance);
            Vector2 topLeft = RayCastCorners.TopLeft;
            Vector2 topRight = RayCastCorners.TopRight;
            var up = Vector2.up;
            var distance2 = (double)distance1;
            var groundLayer = (int)_moveStats.GroundLayer;
            var raycastHit2D1 = Physics2D.Raycast(topLeft, up, (float)distance2, groundLayer);
            var raycastHit2D2 = Physics2D.Raycast(topRight, Vector2.up, distance1, (int)_moveStats.GroundLayer);
            var flag1 = (bool)raycastHit2D1;
            var flag2 = (bool)raycastHit2D2;
            if (flag1 & flag2 || (!flag1 && !flag2))
                return false;
            var num1 = flag1 ? 1f : -1f;
            var num2 = _moveStats.CornerCorrectionWidth + 0.015f;
            var raycastHit2D3 = flag1 ? raycastHit2D1 : raycastHit2D2;
            var distance3 = (double)raycastHit2D3.distance;
            var normal = raycastHit2D3.normal;
            var num3 = num2 * num1;
            var num4 = 0.0f;
            if (Mathf.Abs(normal.y) > 1.0 / 1000.0)
                num4 = (float)(-(double)num3 * (normal.x / (double)normal.y));
            var num5 = 0.3f;
            var num6 = (double)num4;
            var num7 = (float)(distance3 + num6) + num5;
            var vector2 = _rb.position + Vector2.right * num3 + Vector2.up * num7;
            if (IsSpaceClear(vector2))
            {
                correctionAmount = num2 * num1;
                if (_moveStats.DebugShowCornerCorrectionRays)
                    DrawDebugBox(vector2, _coll.bounds.size, Color.green, 0.05f);
                return true;
            }

            if (_moveStats.DebugShowCornerCorrectionRays)
                DrawDebugBox(vector2, _coll.bounds.size, Color.red, 0.05f);
            return false;
        }

        private void DrawDebugBox(Vector2 center, Vector2 size, Color color, float duration)
        {
            var vector2_1 = size / 2f;
            var vector2_2 = center + new Vector2(-vector2_1.x, vector2_1.y);
            var vector2_3 = center + new Vector2(vector2_1.x, vector2_1.y);
            var vector2_4 = center + new Vector2(-vector2_1.x, -vector2_1.y);
            var vector2_5 = center + new Vector2(vector2_1.x, -vector2_1.y);
            Debug.DrawLine(vector2_2, vector2_3, color, duration);
            Debug.DrawLine(vector2_3, vector2_5, color, duration);
            Debug.DrawLine(vector2_5, vector2_4, color, duration);
            Debug.DrawLine(vector2_4, vector2_2, color, duration);
        }

        private bool IsSpaceClear(Vector2 targetPosition)
        {
            var vector2 = (Vector2)(_coll.bounds.size - Vector3.one * 0.03f);
            var point = targetPosition + _coll.offset;
            var contactFilter2D = new ContactFilter2D();
            contactFilter2D.SetLayerMask(_moveStats.GroundLayer);
            contactFilter2D.useTriggers = false;
            var size = vector2;
            var contactFilter = contactFilter2D;
            var overlapBuffer = _overlapBuffer;
            return Physics2D.OverlapBox(point, size, 0.0f, contactFilter, overlapBuffer) == 0;
        }

        private bool DetectHeadCornerCorrection(Vector2 velocity)
        {
            return CalculateHeadCornerCorrection(velocity, out var _);
        }

        private void ApplyHeadCornerCorrection(ref Vector2 velocity)
        {
            float correctionAmount;
            if (!CalculateHeadCornerCorrection(velocity, out correctionAmount))
                return;
            velocity.x += correctionAmount;
            _isCornerCorrectingThisFrame = true;
        }


        private bool CalculateHorizontalCornerCorrection(Vector2 velocity, out float correctionAmount)
        {
            correctionAmount = 0.0f;
            if (!_player.IsDashing || IsClimbingSlope)
                return false;
            var num1 = Mathf.Sign(velocity.x);
            var distance1 = Mathf.Abs(velocity.x) + 0.015f;
            if (Mathf.Abs(velocity.x) < 0.014999999664723873)
                distance1 = 0.03f;
            Vector2 vector2_1 = num1 == -1.0 ? RayCastCorners.BottomLeft : RayCastCorners.BottomRight;
            Vector2 vector2_2 = num1 == -1.0 ? RayCastCorners.TopLeft : RayCastCorners.TopRight;
            var flag1 = false;
            var num2 = 0.0f;
            var raycastHit2D1 =
                Physics2D.Raycast(vector2_1, Vector2.right * num1, distance1, (int)_moveStats.GroundLayer);
            if (_moveStats.DebugShowCornerCorrectionRays)
                Debug.DrawRay(vector2_1, Vector2.right * num1 * distance1, raycastHit2D1 ? Color.red : Color.green);
            if (raycastHit2D1)
            {
                if (Mathf.Round(Vector2.Angle(raycastHit2D1.normal, Vector2.up)) <= (double)_moveStats.MaxSlopeAngle)
                    return false;
                float correctionHeight = _moveStats.HorizontalCornerCorrectionHeight;
                var vector2_3 = vector2_1 + Vector2.up * correctionHeight;
                var distance2 = raycastHit2D1.distance + 0.015f;
                var flag2 = (bool)Physics2D.Raycast(vector2_3, Vector2.right * num1, distance2,
                    (int)_moveStats.GroundLayer);
                if (_moveStats.DebugShowCornerCorrectionRays)
                    Debug.DrawRay(vector2_3, Vector2.right * num1 * distance2, flag2 ? Color.red : Color.green);
                if (!flag2)
                {
                    var vector2_4 = raycastHit2D1.point + Vector2.right * num1 * 0.015f + Vector2.up * correctionHeight;
                    var raycastHit2D2 = Physics2D.Raycast(vector2_4, Vector2.down, correctionHeight + 0.015f,
                        (int)_moveStats.GroundLayer);
                    if (_moveStats.DebugShowCornerCorrectionRays)
                        Debug.DrawRay(vector2_4, Vector2.down * (correctionHeight + 0.015f), Color.cyan);
                    if (raycastHit2D2)
                    {
                        var num3 = raycastHit2D2.point.y + 0.015f - vector2_1.y;
                        if (num3 > 0.0 && num3 <= correctionHeight + 0.014999999664723873 &&
                            !Physics2D.Raycast(vector2_2 + Vector2.right * num1 * 0.015f + Vector2.up * num3,
                                Vector2.up, 0.015f, (int)_moveStats.GroundLayer))
                        {
                            num2 = num3;
                            flag1 = true;
                        }
                    }
                }
            }

            var flag3 = false;
            var num4 = 0.0f;
            var raycastHit2D3 =
                Physics2D.Raycast(vector2_2, Vector2.right * num1, distance1, (int)_moveStats.GroundLayer);
            if (_moveStats.DebugShowCornerCorrectionRays)
                Debug.DrawRay(vector2_2, Vector2.right * num1 * distance1, raycastHit2D3 ? Color.red : Color.green);
            if (raycastHit2D3)
            {
                float correctionHeight = _moveStats.HorizontalCornerCorrectionHeight;
                var vector2_5 = vector2_2 - Vector2.up * correctionHeight;
                var distance3 = raycastHit2D3.distance + 0.015f;
                var flag4 = (bool)Physics2D.Raycast(vector2_5, Vector2.right * num1, distance3,
                    (int)_moveStats.GroundLayer);
                if (_moveStats.DebugShowCornerCorrectionRays)
                    Debug.DrawRay(vector2_5, Vector2.right * num1 * distance3, flag4 ? Color.red : Color.green);
                if (!flag4)
                {
                    var vector2_6 = raycastHit2D3.point + Vector2.right * num1 * 0.015f - Vector2.up * correctionHeight;
                    var raycastHit2D4 = Physics2D.Raycast(vector2_6, Vector2.up, correctionHeight + 0.015f,
                        (int)_moveStats.GroundLayer);
                    if (_moveStats.DebugShowCornerCorrectionRays)
                        Debug.DrawRay(vector2_6, Vector2.up * (correctionHeight + 0.015f), Color.cyan);
                    if (raycastHit2D4)
                    {
                        var f = raycastHit2D4.point.y - 0.015f - vector2_2.y;
                        if (f < 0.0 && Mathf.Abs(f) <= correctionHeight + 0.014999999664723873)
                        {
                            if (Mathf.Abs(f) > (double)_moveStats.HorizontalPushDownMaximum)
                                flag3 = false;
                            if (!Physics2D.Raycast(vector2_1 + Vector2.right * num1 * 0.015f + Vector2.up * f,
                                    Vector2.down, 0.015f, (int)_moveStats.GroundLayer))
                            {
                                num4 = f;
                                flag3 = true;
                            }
                        }
                    }
                }
            }

            if (flag1 & flag3)
                return false;
            if (flag1)
            {
                correctionAmount = num2;
                return true;
            }

            if (!flag3)
                return false;
            correctionAmount = num4;
            return true;
        }

        private void ApplyHorizontalCornerCorrection(ref Vector2 velocity)
        {
            float correctionAmount;
            if (!CalculateHorizontalCornerCorrection(velocity, out correctionAmount))
                return;
            velocity.y = correctionAmount;
            _isHorizontalCornerCorrectingThisFrame = true;
        }

        private void DetectSlopeNormalsForVisuals(Vector2 velocity)
        {
            if (_internalState.IsGrounded)
            {
                float normalsRayLength = _moveStats.SlopeAveragedNormalsRayLength;
                var zero = Vector2.zero;
                var num1 = 0;
                float visualRaycastWidth = _moveStats.VisualRaycastWidth;
                var num2 = _coll.bounds.center.x - visualRaycastWidth / 2f;
                var y = _coll.bounds.min.y;
                var num3 = 0.0f;
                if (NumOfVerticalRaysForVisualNormals > 1)
                    num3 = visualRaycastWidth / (NumOfVerticalRaysForVisualNormals - 1);
                for (var index = 0; index < NumOfVerticalRaysForVisualNormals; ++index)
                {
                    var raycastHit2D =
                        Physics2D.Raycast(new Vector2(num2 + num3 * index, y) + Vector2.right * velocity.x,
                            Vector2.down, normalsRayLength, (int)_moveStats.GroundLayer);
                    if (raycastHit2D)
                    {
                        zero += raycastHit2D.normal;
                        ++num1;
                    }
                }

                if (num1 > 0)
                {
                    var vector2 = zero / num1;
                    vector2.Normalize();
                    _internalState.AveragedVisualNormal = vector2;
                }
                else
                {
                    _internalState.AveragedVisualNormal = Vector2.up;
                }
            }
            else
            {
                _internalState.AveragedVisualNormal = Vector2.Lerp(_internalState.AveragedVisualNormal, Vector2.up,
                    _moveStats.SlopeRotationSpeed * Time.fixedDeltaTime);
            }
        }

        public bool IsGrounded()
        {
            return _internalState.IsGrounded;
        }

        public bool BumpedHead()
        {
            return _internalState.IsHittingCeiling;
        }

        public bool IsTouchingWall()
        {
            return _internalState.IsAgainstWall;
        }

        public int GetWallDirection()
        {
            return _internalState.WallDirection;
        }

        public struct RaycastCorners
        {
            public Vector2 TopLeft;
            public Vector2 TopRight;
            public Vector2 BottomLeft;
            public Vector2 BottomRight;
        }
    }
}