using UnityEngine;

namespace TidiMovementComponent2D.Animation.BlendSpaces
{
    public abstract class TidiBaseBlendSpace<TInput> : ScriptableObject
    {
        public abstract int Resolve(TInput input);
    }
}