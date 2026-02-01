using UnityEngine;

namespace TidiMovementComponent2D.MovingPlatforms
{
    public interface IPushable
    {
        void ApplyExternalPush(Vector2 pushAmount, Transform pusher);
    }
}
