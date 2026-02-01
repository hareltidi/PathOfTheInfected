using UnityEngine;

namespace TidiMovementComponent2D.Physics
{
    public struct CollisionStateSm
    {
        public bool IsGrounded;
        public bool WasGroundedLastFrame;
        public bool IsAgainstWall;
        public bool WasAgainstWallLastFrame;
        public int WallDirection;
        public float WallAngle;
        public bool IsOnSlope;
        public float SlopeAngle;
        public Vector2 SlopeNormal;
        public bool IsAgainstSteepSlope;
        public bool IsHittingCeiling;
        public float CeilingAngle;
        public Vector2 CeilingNormal;
        public Vector2 AveragedVisualNormal;

        public void Reset()
        {
            WasGroundedLastFrame = IsGrounded;
            WasAgainstWallLastFrame = IsAgainstWall;
            IsGrounded = false;
            IsAgainstWall = false;
            WallDirection = 0;
            WallAngle = 0.0f;
            IsOnSlope = false;
            SlopeAngle = 0.0f;
            SlopeNormal = Vector2.zero;
            IsAgainstSteepSlope = false;
            IsHittingCeiling = false;
            CeilingAngle = 0.0f;
            CeilingNormal = Vector2.zero;
            AveragedVisualNormal = Vector2.up;
        }
    }
}