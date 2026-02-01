using System;

namespace TidiMovementComponent2D.Core
{

    [Serializable]
    public struct NavMovementCapabilities
    {
        public bool canRun;
        public bool canWallSlide;
        public bool canWallJump;
        public bool canJump;
        public bool canDash;
        public bool canCrouch;
        public bool canVault;
    }
}
