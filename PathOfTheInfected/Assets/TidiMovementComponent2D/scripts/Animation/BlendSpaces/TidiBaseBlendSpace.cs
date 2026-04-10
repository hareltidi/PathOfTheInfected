using UnityEngine;

namespace TidiMovementComponent2D.Animation.BlendSpaces
{
    public abstract class TidiBaseBlendSpace<TInput> : ScriptableObject
    {
        public abstract BlendResult Evaluate(TInput input);
    }
}