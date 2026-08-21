using System.Collections.Generic;
using ClothPhysicsSystem2D.Data;
using TidiMovementComponent2D.Core;
using UnityEngine;

namespace ClothPhysicsSystem2D
{
    public class ClothController : MonoBehaviour
    {
        [Header("Cloth Settings")]
        [SerializeField] private int pointCount = 8;
        [SerializeField] private float segmentLength = 0.2f;
        [SerializeField] private bool debugMode;
        public Transform segmentTransformStart;

        [Header("Physics")]
        [SerializeField] private float damping = 0.96f;
        [SerializeField] private float velocityDrag = 0.05f;
        [SerializeField] private int constraintIterations = 4;
        [SerializeField] private int solverSubsteps = 2;
        [SerializeField] private float stiffness = 0.8f;
        [SerializeField] private float simulationDt = 0.02f;
        [SerializeField] private float forceMultiplier = 1f;

        [Header("Bend Constraints")]
        [SerializeField] private bool enableBendConstraints = true;
        [SerializeField] private int bendIterations = 1;
        [SerializeField] private float bendStiffness = 0.25f;

        [Header("Shape Control")]
        [SerializeField] private float shapeStrength = 0.1f;
        [SerializeField] private bool useHierarchyFacingInShape;
        [SerializeField] private float endSegmentRotationOffset = 180f;
        [SerializeField] private float endSegmentPositionYOffset = -0.0467f;
        [SerializeField] private float endSegmentPositionXOffset = -0.134f;

        [Header("Direction")]
        [SerializeField] private Vector2 localDirection = Vector2.right;
        [SerializeField] private bool followPlayerFacing = true;
        [SerializeField] private bool invertFacingDirection = true;
        [SerializeField] private bool snapToFacingOnTurn = true;
        [SerializeField] private float turnImpulseStrength = 0.3f;
        [SerializeField] private float turnImpulseFalloff = 0.85f;
        [SerializeField] private float turnUpwardImpulseStrength = 0.22f;
        [SerializeField] private float turnUpwardImpulseFalloff = 0.9f;
        [SerializeField] private float turnLiftPosition = 0.35f;

        [Header("Turn Stabilization")]
        [SerializeField] private float turnStabilizationDuration = 0.2f;
        [SerializeField] private float maxTurnDropFromAnchor = 0.32f;
        [SerializeField] private float turnRecoveryStrength = 18f;
        [SerializeField] private float maxRiseAboveAnchor = 0.2f;
        [SerializeField] private float riseRecoveryStrength = 14f;
        [SerializeField] private float turnShapeBoost = 1.8f;
        [SerializeField] private float turnBendBoost = 1.35f;
        [SerializeField] private float turnTransitionSpeed = 10f;

        [Header("Rendering")]
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private GameObject endSegmentPrefab;
        [SerializeField] private bool quantizeSegmentAngles = true;
        [SerializeField] private float angleStepDegrees = 11.25f;
        [SerializeField] private bool steppedVisualUpdate;
        [SerializeField] private float visualFps = 15f;

        [Header("External Forces")]
        [SerializeField] private Vector2 externalGravity = new Vector2(0, -2f);
        [SerializeField] private Vector2 windDirectionWorld = Vector2.right;
        [SerializeField] private float windStrength = 1f;
        [SerializeField] private float windFrequency = 2f;
        [SerializeField] private float gustStrength = 0.35f;
        [SerializeField] private float turbulenceStrength = 0.25f;
        [SerializeField] private float globalGustFrequency = 0.5f;
        [SerializeField] private float sineWindWeight = 0.05f;
        [SerializeField] private float noiseWindWeight = 0.8f;
        [SerializeField] private float segmentPhaseJitter = 0.45f;
        [SerializeField] private float segmentAmplitudeJitter = 0.3f;
        [SerializeField] private float playerVelocitySmoothing = 10f;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float playerForceMultiplier = 0.3f;
        [SerializeField] private float maxPlayerVelocityForForce = 14f;
        [SerializeField] private float dashMovementForceMultiplier = 0.45f;
        [SerializeField] private float anchorTransportFollow = 0.82f;


        [Header("Collisions")]
        [SerializeField] private bool enableCollisions = true;
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.05f;
        [SerializeField] private float bounceFactor = 0.2f;

        public List<ClothPoint> points = new();
        private List<Transform> _segmentTransforms = new();

        private Vector2 _previousPlayerPosition;
        private Vector2 _smoothedPlayerVelocity;
        private bool _hasPreviousPlayerPosition;
        private float _currentFacingSign = 1f;
        private float _smoothedFacingSign = 1f;
        private float _simulationAccumulator;
        private float _visualAccumulator;
        private bool _isInitialized;
        private Vector2 _lastAnchorPosition;
        private bool _hasLastAnchorPosition;
        private bool _forceVisualRefreshThisFrame;
        private float _turnStabilizeTimer;

        // This array caches our collision results
        private Collider2D[] _collisionResults = new Collider2D[4];
        // Modern Unity uses Contact Filters instead of NonAlloc
        private ContactFilter2D _contactFilter;

        private PlayerSm Player => PlayerSm.Instance;

        private void Start()
        {
            if (!ValidateSetup())
            {
                enabled = false;
                return;
            }

            pointCount = Mathf.Max(2, pointCount);
            segmentLength = Mathf.Max(0.001f, segmentLength);
            solverSubsteps = Mathf.Max(1, solverSubsteps);
            constraintIterations = Mathf.Max(1, constraintIterations);
            bendIterations = Mathf.Max(1, bendIterations);
            simulationDt = Mathf.Max(0.001f, simulationDt);

            ResolvePlayerTransform();
            SetupContactFilter();

            _currentFacingSign = GetFacingSign();
            _smoothedFacingSign = _currentFacingSign;
            localDirection = new Vector2(_currentFacingSign, localDirection.y).normalized;

            InitializePoints();
            InitializeSegments();

            for (int i = 0; i < 10; i++)
            {
                ApplyConstraints();
                if (enableBendConstraints)
                {
                    ApplyBendConstraints();
                }
            }

            if (playerTransform)
            {
                _previousPlayerPosition = playerTransform.position;
                _hasPreviousPlayerPosition = true;
            }

            _isInitialized = true;

            if (segmentTransformStart)
            {
                _lastAnchorPosition = segmentTransformStart.position;
                _hasLastAnchorPosition = true;
            }
        }

        private void LateUpdate()
        {
            if (!_isInitialized) return;

            ResolvePlayerTransform();
            SamplePlayerMotion(Time.deltaTime);
            _forceVisualRefreshThisFrame = false;
            DecideLocalDirection();
            _smoothedFacingSign = Mathf.MoveTowards(_smoothedFacingSign, _currentFacingSign, turnTransitionSpeed * Time.deltaTime);
            TransportWithAnchor();
            _turnStabilizeTimer = Mathf.Max(0f, _turnStabilizeTimer - Time.deltaTime);

            _simulationAccumulator += Time.deltaTime;
            int maxSimSteps = 8;
            int simSteps = 0;

            while (_simulationAccumulator >= simulationDt && simSteps < maxSimSteps)
            {
                SimulateCloth(simulationDt);
                _simulationAccumulator -= simulationDt;
                simSteps++;
            }

            PinAnchorToTransform();

            if (steppedVisualUpdate)
            {
                float frameDt = 1f / Mathf.Max(1f, visualFps);
                _visualAccumulator += Time.deltaTime;
                if (!_forceVisualRefreshThisFrame)
                {
                    if (_visualAccumulator < frameDt) return;
                    _visualAccumulator %= frameDt;
                }
                else
                {
                    _visualAccumulator = 0f;
                }
            }

            UpdateSegments();
        }

        #region Initialization

        private void InitializePoints()
        {
            points.Clear();

            Vector2 startPos = segmentTransformStart.position;
            Vector2 direction = segmentTransformStart.TransformDirection(localDirection).normalized;

            for (int i = 0; i < pointCount; i++)
            {
                Vector2 pos = startPos + direction * segmentLength * i;

                points.Add(new ClothPoint
                {
                    position = pos,
                    previousPosition = pos
                });
            }
        }

        private void SetupContactFilter()
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.useLayerMask = true;
            _contactFilter.layerMask = collisionMask;
            _contactFilter.useTriggers = false;
        }

        private void InitializeSegments()
        {
            _segmentTransforms.Clear();

            for (int i = 0; i < points.Count - 1; i++)
            {
                GameObject prefabToUse = (i == points.Count - 2)
                    ? endSegmentPrefab
                    : segmentPrefab;

                if (!prefabToUse)
                {
                    prefabToUse = segmentPrefab ? segmentPrefab : endSegmentPrefab;
                }

                if (!prefabToUse) continue;

                GameObject seg = Instantiate(prefabToUse, transform);
                _segmentTransforms.Add(seg.transform);
            }
        }

        #endregion

        #region Simulation

        private void SimulateCloth(float dt)
        {
            if (points == null || points.Count < 2) return;

            int substeps = Mathf.Max(1, solverSubsteps);
            float substepDt = dt / substeps;

            for (int step = 0; step < substeps; step++)
            {
                ApplyExternalForces(substepDt);
                HandleCollisions();

                for (int i = 0; i < constraintIterations; i++)
                {
                    ApplyConstraints();

                    if (!enableBendConstraints) continue;

                    for (int bendStep = 0; bendStep < bendIterations; bendStep++)
                    {
                        ApplyBendConstraints();
                    }
                }
            }

            ApplyShapeConstraints();
        }

        private void ApplyExternalForces(float dt)
        {
            if (points == null || points.Count < 2) return;

            Vector2 windDir = windDirectionWorld.sqrMagnitude > 0.0001f ? windDirectionWorld.normalized : Vector2.right;
            float globalGust = (Mathf.PerlinNoise(Time.time * Mathf.Max(0.01f, globalGustFrequency), 0.173f) - 0.5f) * 2f;
            float stabilization01 = turnStabilizationDuration > 0.0001f
                ? Mathf.Clamp01(_turnStabilizeTimer / turnStabilizationDuration)
                : 0f;

            for (int i = 1; i < points.Count; i++)
            {
                var p = points[i];

                float t = (float)i / (points.Count - 1);
                float tipInfluence = Mathf.Lerp(0.25f, 1f, t);
                float stable01 = GetStableNoise01(i, 0.913f);
                float phaseJitter = (stable01 - 0.5f) * 2f * segmentPhaseJitter;
                float amplitudeJitter = 1f + ((GetStableNoise01(i, 1.771f) - 0.5f) * 2f * segmentAmplitudeJitter);

                float wave = Mathf.Sin((Time.time * windFrequency) + (i * 0.37f) + phaseJitter);
                float lowBandNoise = (Mathf.PerlinNoise((i * 0.29f) + phaseJitter, Time.time * windFrequency * 0.35f) - 0.5f) * 2f;
                float highBandNoise = (Mathf.PerlinNoise((i * 0.57f) + phaseJitter, Time.time * windFrequency * 1.1f) - 0.5f) * 2f;
                float noiseBlend = (lowBandNoise * 0.7f) + (highBandNoise * 0.3f);
                
                float ambientGust = globalGust * gustStrength;
                float flutter = (wave * sineWindWeight) + (noiseBlend * noiseWindWeight * turbulenceStrength);

                Vector2 tangent = i < points.Count - 1
                    ? (points[i + 1].position - p.position)
                    : (p.position - points[i - 1].position);
                
                if (tangent.sqrMagnitude < 0.000001f)
                {
                    tangent = (i > 0) ? (p.position - points[0].position) : Vector2.right;
                    if (tangent.sqrMagnitude < 0.000001f)
                    {
                        tangent = Vector2.right;
                    }
                }
                tangent.Normalize();
                Vector2 normal = new Vector2(-tangent.y, tangent.x);

                float windExposure = Mathf.Lerp(0.35f, 1f, Mathf.Abs(Vector2.Dot(windDir, normal)));

                float baseWindMag = Mathf.Max(0f, windStrength * (1f + ambientGust) * amplitudeJitter);
                Vector2 baseWindForce = windDir * (baseWindMag * windExposure * tipInfluence);

                float flutterMag = flutter * windStrength * amplitudeJitter * 2f;
                Vector2 flutterForce = normal * (flutterMag * tipInfluence);

                Vector2 playerVelocityForces = Vector2.ClampMagnitude(_smoothedPlayerVelocity, Mathf.Max(0.01f, maxPlayerVelocityForForce));
                float dashForceScale = (Player && Player.IsDashing) ? Mathf.Clamp01(dashMovementForceMultiplier) : 1f;
                Vector2 movementForce = -playerVelocityForces * (playerForceMultiplier * tipInfluence * dashForceScale);

                Vector2 turnRecoveryForce = Vector2.zero;
                if (stabilization01 > 0f)
                {
                    float maxDrop = Mathf.Max(0.01f, maxTurnDropFromAnchor) * tipInfluence;
                    float minY = points[0].position.y - maxDrop;
                    if (p.position.y < minY)
                    {
                        float penetration = minY - p.position.y;
                        turnRecoveryForce = Vector2.up * (penetration * Mathf.Max(0f, turnRecoveryStrength) * stabilization01);
                    }
                }

                // Counter upward drift from repeated turn lift / partial-keyframe anchor motion.
                Vector2 riseRecoveryForce = Vector2.zero;
                float maxRise = Mathf.Max(0.01f, maxRiseAboveAnchor) * tipInfluence;
                float maxY = points[0].position.y + maxRise;
                if (p.position.y > maxY)
                {
                    float excess = p.position.y - maxY;
                    riseRecoveryForce = Vector2.down * (excess * Mathf.Max(0f, riseRecoveryStrength));
                }

                // Standard Verlet Integration
                Vector2 velocity = p.position - p.previousPosition;
                Vector2 dragForce = -velocity * velocityDrag;
                Vector2 totalForce = (externalGravity + baseWindForce + flutterForce + movementForce + turnRecoveryForce + riseRecoveryForce + dragForce) * forceMultiplier;
                Vector2 nextPosition = p.position + velocity + totalForce * (dt * dt);

                nextPosition = p.position + (nextPosition - p.position) * damping;

                p.previousPosition = p.position;
                p.position = nextPosition;
            }
        }

        private void ApplyConstraints()
        {
            if (points == null || points.Count < 2)
                return;

            // Anchor
            points[0].position = segmentTransformStart.position;
            points[0].previousPosition = segmentTransformStart.position;

            // Distance constraints
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                Vector2 dir = p2.position - p1.position;
                float dist = dir.magnitude;

                if (dist == 0) continue;

                float error = dist - segmentLength;

                if (i == 0)
                {
                    // Node 0 is pinned, so apply 100% of the correction to Node 1 to prevent stretching
                    Vector2 correction = dir.normalized * (error * stiffness);
                    p2.position -= correction;
                }
                else
                {
                    // Split the correction between the two free nodes
                    Vector2 correction = dir.normalized * (error * 0.5f * stiffness);
                    p1.position += correction;
                    p2.position -= correction;
                }
            }
        }

        private void ApplyBendConstraints()
        {
            if (points == null || points.Count < 3)
                return;

            float targetLength = segmentLength * 2f;
            float stabilization01 = turnStabilizationDuration > 0.0001f
                ? Mathf.Clamp01(_turnStabilizeTimer / turnStabilizationDuration)
                : 0f;
            float bendStrength = bendStiffness * Mathf.Lerp(1f, Mathf.Max(1f, turnBendBoost), stabilization01);

            for (int i = 0; i < points.Count - 2; i++)
            {
                ClothPoint p0 = points[i];
                ClothPoint p2 = points[i + 2];

                Vector2 delta = p2.position - p0.position;
                float dist = delta.magnitude;
                if (dist < 0.00001f) continue;

                float error = dist - targetLength;
                Vector2 correction = delta.normalized * (error * 0.5f * bendStrength);

                if (i == 0)
                {
                    p2.position -= correction * 2f;
                }
                else
                {
                    p0.position += correction;
                    p2.position -= correction;
                }
            }
        }

        private void ApplyShapeConstraints()
        {
            if (points == null || points.Count < 2)
                return;

            float stabilization01 = turnStabilizationDuration > 0.0001f
                ? Mathf.Clamp01(_turnStabilizeTimer / turnStabilizationDuration)
                : 0f;
            float shapeStrengthNow = shapeStrength * Mathf.Lerp(1f, Mathf.Max(1f, turnShapeBoost), stabilization01);

            Vector2 shapeDirection = followPlayerFacing
                ? new Vector2(_smoothedFacingSign, localDirection.y).normalized
                : (localDirection.sqrMagnitude > 0.0001f ? localDirection.normalized : Vector2.right);

            if (useHierarchyFacingInShape && !followPlayerFacing)
            {
                float hierarchyFacing = Mathf.Sign(segmentTransformStart.lossyScale.x);
                if (Mathf.Abs(hierarchyFacing) < 0.0001f)
                {
                    hierarchyFacing = 1f;
                }

                if (invertFacingDirection)
                {
                    hierarchyFacing *= -1f;
                }

                shapeDirection = new Vector2(shapeDirection.x * hierarchyFacing, shapeDirection.y);
            }

            Vector2 baseDir = (segmentTransformStart.rotation * shapeDirection).normalized;

            for (int i = 1; i < points.Count; i++)
            {
                Vector2 targetPos = points[0].position + baseDir * (segmentLength * i);

                float t = (float)i / (points.Count - 1);
                float falloff = Mathf.Lerp(1f, 0.2f, t);

                points[i].position = Vector2.Lerp(
                    points[i].position,
                    targetPos,
                    shapeStrengthNow * falloff
                );
            }
        }

        private void DecideLocalDirection()
        {
            if (!followPlayerFacing)
            {
                localDirection = localDirection.sqrMagnitude > 0.0001f ? localDirection.normalized : Vector2.right;
                return;
            }

            float targetX = GetFacingSign();

            if (Mathf.Abs(targetX - _currentFacingSign) > 0.001f)
            {
                _currentFacingSign = targetX;
                _smoothedFacingSign = _currentFacingSign;
                localDirection = new Vector2(_currentFacingSign, localDirection.y).normalized;
                _forceVisualRefreshThisFrame = true;
                _turnStabilizeTimer = Mathf.Max(_turnStabilizeTimer, turnStabilizationDuration);

                if (snapToFacingOnTurn)
                {
                    SnapClothToFacing(_currentFacingSign);
                }
                else
                {
                    MirrorClothAcrossAnchor(_currentFacingSign);
                    ApplyTurnImpulse(_currentFacingSign);
                }

                ApplyTurnUpwardImpulse();
            }
            else
            {
                localDirection = new Vector2(_currentFacingSign, localDirection.y).normalized;
            }
        }

        private float GetFacingSign()
        {
            float targetX = 1f;

            if (Player)
            {
                targetX = Player.IsFacingRight ? 1f : -1f;
            }
            else if (segmentTransformStart)
            {
                targetX = segmentTransformStart.lossyScale.x >= 0f ? 1f : -1f;
            }

            if (invertFacingDirection)
            {
                targetX *= -1f;
            }

            return NormalizeSign(targetX, _currentFacingSign);
        }

        private void ResolvePlayerTransform()
        {
            if (playerTransform)
            {
                return;
            }

            if (Player)
            {
                playerTransform = Player.Rb ? Player.Rb.transform : Player.transform;
                if (playerTransform && !_hasPreviousPlayerPosition)
                {
                    _previousPlayerPosition = playerTransform.position;
                    _hasPreviousPlayerPosition = true;
                }
            }
        }

        private void SamplePlayerMotion(float dt)
        {
            ResolvePlayerTransform();

            if (!playerTransform)
            {
                _smoothedPlayerVelocity = Vector2.Lerp(_smoothedPlayerVelocity, Vector2.zero,
                    1f - Mathf.Exp(-Mathf.Max(0.1f, playerVelocitySmoothing) * dt));
                return;
            }

            Vector2 currentPlayerPosition = playerTransform.position;

            if (!_hasPreviousPlayerPosition)
            {
                _previousPlayerPosition = currentPlayerPosition;
                _hasPreviousPlayerPosition = true;
                _smoothedPlayerVelocity = Vector2.zero;
                return;
            }

            Vector2 playerVelocity = (currentPlayerPosition - _previousPlayerPosition) / Mathf.Max(dt, 0.0001f);
            _previousPlayerPosition = currentPlayerPosition;

            float velocityLerp = 1f - Mathf.Exp(-Mathf.Max(0.1f, playerVelocitySmoothing) * dt);
            _smoothedPlayerVelocity = Vector2.Lerp(_smoothedPlayerVelocity, playerVelocity, velocityLerp);
        }

        private void ApplyTurnImpulse(float directionSign)
        {
            if (points == null || points.Count < 2 || turnImpulseStrength <= 0f)
                return;

            for (int i = 1; i < points.Count; i++)
            {
                float t = (float)i / (points.Count - 1);
                float tipInfluence = Mathf.Lerp(0.25f, 1f, t);
                float falloff = Mathf.Lerp(1f, turnImpulseFalloff, t);
                float impulse = directionSign * turnImpulseStrength * tipInfluence * falloff;

                // Adjusting previousPosition adds instantaneous directional velocity without teleporting points.
                points[i].previousPosition -= new Vector2(impulse, 0f);
            }
        }

        private void ApplyTurnUpwardImpulse()
        {
            if (points == null || points.Count < 2 || turnUpwardImpulseStrength <= 0f)
                return;

            for (int i = 1; i < points.Count; i++)
            {
                float t = (float)i / (points.Count - 1);
                float tipInfluence = Mathf.Lerp(0.3f, 1f, t);
                float falloff = Mathf.Lerp(1f, turnUpwardImpulseFalloff, t);

                // 1. Calculate the Velocity Impulse
                float upwardImpulse = turnUpwardImpulseStrength * tipInfluence * falloff;

                // 2. Calculate the Physical Bump
                float positionLift = turnLiftPosition * tipInfluence * falloff;

                // Instantly teleport the cloth upwards in world space safely
                points[i].position += new Vector2(0f, positionLift);
                points[i].previousPosition += new Vector2(0f, positionLift);

                // Apply the Verlet velocity impulse by pushing the previous position down
                points[i].previousPosition -= new Vector2(0f, upwardImpulse);
            }
        }

        private void MirrorClothAcrossAnchor(float directionSign)
        {
            if (points == null || points.Count < 2)
                return;

            for (int i = 0; i < points.Count; i++)
            {
                float offsetX = Mathf.Abs(points[i].position.x - segmentTransformStart.position.x);
                float facingSign = NormalizeSign(directionSign, _currentFacingSign);
                Vector2 mirroredPos = new Vector2(segmentTransformStart.position.x + (offsetX * facingSign), points[i].position.y);
                points[i].position = mirroredPos;
                points[i].previousPosition = mirroredPos;
            }
        }

        private void SnapClothToFacing(float directionSign)
        {
            if (!segmentTransformStart || points == null || points.Count < 2)
                return;

            float facingSign = NormalizeSign(directionSign, _currentFacingSign);
            Vector2 anchor = segmentTransformStart.position;
            Vector2 direction = segmentTransformStart.rotation * new Vector2(facingSign, localDirection.y).normalized;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 snappedPos = anchor + direction * (segmentLength * i);
                points[i].position = snappedPos;
                points[i].previousPosition = snappedPos;
            }

            _lastAnchorPosition = anchor;
            _hasLastAnchorPosition = true;
        }

        private static float NormalizeSign(float value, float fallback)
        {
            if (Mathf.Abs(value) >= 0.0001f)
            {
                return Mathf.Sign(value);
            }

            if (Mathf.Abs(fallback) >= 0.0001f)
            {
                return Mathf.Sign(fallback);
            }

            return 1f;
        }

        private static float GetStableNoise01(int index, float salt)
        {
            float value = Mathf.Sin((index * 12.9898f) + (salt * 78.233f)) * 43758.5453f;
            return value - Mathf.Floor(value);
        }

        private void TransportWithAnchor()
        {
            if (!segmentTransformStart || points == null || points.Count == 0)
                return;

            Vector2 currentAnchor = segmentTransformStart.position;

            if (!_hasLastAnchorPosition)
            {
                _lastAnchorPosition = currentAnchor;
                _hasLastAnchorPosition = true;
            }

            Vector2 anchorDelta = currentAnchor - _lastAnchorPosition;
            if (anchorDelta.sqrMagnitude > 0f)
            {
                // Move free points with the anchor so high-speed character motion does not inject artificial stretch jitter.
                for (int i = 1; i < points.Count; i++)
                {
                    float t = (float)i / (points.Count - 1);
                    float transportFollow = Mathf.Lerp(1f, Mathf.Clamp01(anchorTransportFollow), t * t);
                    Vector2 transport = anchorDelta * transportFollow;

                    points[i].position += transport;
                    points[i].previousPosition += transport;
                }
            }

            _lastAnchorPosition = currentAnchor;
            PinAnchorToTransform();
        }

        private void PinAnchorToTransform()
        {
            if (!segmentTransformStart || points == null || points.Count == 0)
                return;

            Vector2 anchor = segmentTransformStart.position;
            points[0].position = anchor;
            points[0].previousPosition = anchor;
        }

        #endregion

        #region Collisions
        private void HandleCollisions()
        {
            if (!enableCollisions || points == null || points.Count < 2) return;

            // Start at 1 so we don't apply collisions to the pinned anchor point
            for (int i = 1; i < points.Count; i++)
            {
                var segment = points[i];

                // 1. Calculate current velocity
                Vector2 velocity = segment.position - segment.previousPosition;
                bool hitSomething = false;

                // 2. Find colliders (using NonAlloc to save performance!)
                int hitCount = Physics2D.OverlapCircle(segment.position, collisionRadius, _contactFilter, _collisionResults);

                for (int j = 0; j < hitCount; j++)
                {
                    Collider2D col = _collisionResults[j];

                    Vector2 closestPoint = col.ClosestPoint(segment.position);
                    float distance = Vector2.Distance(segment.position, closestPoint);
                    Vector2 normal = (segment.position - closestPoint).normalized;

                    // If within the collision radius, resolve
                    if (distance < collisionRadius)
                    {
                        // Push the point out of the collider
                        float depth = collisionRadius - distance;
                        if (normal != Vector2.zero)
                        {
                            segment.position += normal * depth;
                            hitSomething = true;
                        }
                    }
                }

                if (hitSomething)
                {
                    segment.previousPosition = segment.position - (velocity * bounceFactor);
                }
            }
        }
        #endregion

        #region Rendering

        private void UpdateSegments()
        {
            if (points == null || points.Count < 2 || _segmentTransforms == null || _segmentTransforms.Count == 0)
                return;

            int segmentCount = Mathf.Min(_segmentTransforms.Count, points.Count - 1);

            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 p1 = points[i].position;
                Vector2 p2 = points[i + 1].position;

                Vector2 dir = p2 - p1;
                float dirSqr = dir.sqrMagnitude;

                if (dirSqr < 1e-8f)
                    continue;

                // Snap directly to the simulated physical midpoint.
                // The physics points themselves are already smooth, so we don't need to lerp visuals.
                Vector2 pos = (p1 + p2) * 0.5f;

                float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float finalAngle = quantizeSegmentAngles ? QuantizeAngle(rawAngle) : rawAngle;

                if (i == segmentCount - 1)
                {
                    // Rotate the custom local offsets by the cloth's raw physical angle so position stays smooth.
                    Vector3 localOffset = new Vector3(endSegmentPositionXOffset, endSegmentPositionYOffset, 0);
                    Vector3 rotatedOffset = Quaternion.Euler(0, 0, rawAngle) * localOffset;

                    _segmentTransforms[i].position = (Vector3)pos + rotatedOffset;

                    // Apply visual sprite flip only to rendered rotation.
                    finalAngle += endSegmentRotationOffset;

                    // Do not stretch the custom end-piece sprite. Stretching causes pixel distortion that looks like a kink.
                    _segmentTransforms[i].localScale = new Vector3(1f, _currentFacingSign, 1f);
                }
                else
                {
                    _segmentTransforms[i].position = pos;

                    // Only stretch the middle generic repeating bits
                    float length = Mathf.Sqrt(dirSqr);
                    _segmentTransforms[i].localScale = new Vector3(length / segmentLength, _currentFacingSign, 1f);
                }

                _segmentTransforms[i].rotation = Quaternion.Euler(0, 0, finalAngle);
            }
        }

        private float QuantizeAngle(float angle)
        {
            float step = Mathf.Max(0.001f, angleStepDegrees);
            return Mathf.Round(angle / step) * step;
        }

        private bool ValidateSetup()
        {
            if (!segmentTransformStart)
            {
                Debug.LogWarning("ClothController is missing segmentTransformStart.", this);
                return false;
            }

            if (!segmentPrefab)
            {
                Debug.LogWarning("ClothController is missing segmentPrefab", this);
                return false;
            }
            else if (!endSegmentPrefab)
            {
                Debug.LogWarning("ClothController is missing segmentPrefab and endSegmentPrefab.", this);
                return false;
            }
            else if (!segmentPrefab && !endSegmentPrefab)
            {
                Debug.LogWarning("ClothController is missing segmentPrefab and endSegmentPrefab.", this);
                return false;
            }


            return true;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (points == null || !debugMode) return;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }

        #endregion
    }
}


