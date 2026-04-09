using System;
using TidiMovementComponent2D.Animation.BlendSpaces;
using UnityEngine;

namespace PathOfTheInfected.Animation.BlendSpaces
{



    [Serializable]
    public struct Vector2BlendData : IBlendable
    {
        public AnimationClip clip;
        public Vector2 direction; // normalized direction vector
    }


    [CreateAssetMenu(fileName = "BlendSpace2DVector2", menuName = "Blend Space/Vector2/BlendSpace2DVector2", order = 5)]
    public class BlendSpace2DVector2 : TidiBaseBlendSpace<Vector2>
    {
        [SerializeField] protected Vector2BlendData[] blendPoints;

        public override int Resolve(Vector2 input)
        {
            if (blendPoints == null || blendPoints.Length == 0)
            {
                Debug.LogWarning("Blend space has no points!");
                return 0;
            }

            float closestAngle = float.MaxValue;
            Vector2BlendData closestPoint = blendPoints[0];
            Vector2 normalizedInput = input.normalized;

            foreach (var point in blendPoints)
            {
                float angle = Vector2.Angle(normalizedInput, point.direction.normalized);
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    closestPoint = point;
                }
            }

            return Animator.StringToHash(closestPoint.clip.name);
        }
    }
}