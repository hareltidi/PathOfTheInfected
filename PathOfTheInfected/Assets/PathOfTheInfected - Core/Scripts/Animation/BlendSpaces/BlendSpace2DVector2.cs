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


    [CreateAssetMenu(fileName = "BlendSpace2DVector2", menuName = "Blend Space/Vector2/BlendSpace2DVector2", order = 10)]
    public class BlendSpace2DVector2 : TidiBaseBlendSpace<Vector2>
    {
        [SerializeField] protected Vector2BlendData[] blendPoints;
        [SerializeField] [Min(1)] private int maxActiveSamples = 3;
        [SerializeField] private BlendApplicationMode applicationMode = BlendApplicationMode.Override;
        [SerializeField] [Range(0f, 1f)] private float blendWeight = 1f;

        public override BlendResult Evaluate(Vector2 input)
        {
            if (blendPoints == null || blendPoints.Length == 0)
                return default;

            float epsilon = 0.0001f;

            var activeCount = Mathf.Min(Mathf.Max(1, maxActiveSamples), blendPoints.Length);
            var bestIndices = new int[activeCount];
            var bestDistances = new float[activeCount];
            for (int i = 0; i < activeCount; i++)
            {
                bestIndices[i] = -1;
                bestDistances[i] = float.PositiveInfinity;
            }

            for (var i = 0; i < blendPoints.Length; i++)
            {
                if (blendPoints[i].clip == null)
                    continue;

                var dist = Vector2.Distance(input, blendPoints[i].direction);
                for (int slot = 0; slot < activeCount; slot++)
                {
                    if (dist >= bestDistances[slot])
                        continue;

                    for (int shift = activeCount - 1; shift > slot; shift--)
                    {
                        bestDistances[shift] = bestDistances[shift - 1];
                        bestIndices[shift] = bestIndices[shift - 1];
                    }

                    bestDistances[slot] = dist;
                    bestIndices[slot] = i;
                    break;
                }
            }

            int validCount = 0;
            for (int i = 0; i < activeCount; i++)
            {
                if (bestIndices[i] >= 0)
                    validCount++;
            }

            if (validCount == 0)
                return default;

            var samples = new BlendSample[validCount];
            var totalWeight = 0f;
            for (int i = 0; i < validCount; i++)
            {
                float weight = 1f / (bestDistances[i] + epsilon);
                samples[i] = new BlendSample
                {
                    Clip = blendPoints[bestIndices[i]].clip,
                    Weight = weight
                };
                totalWeight += weight;
            }

            if (totalWeight > 0f)
            {
                for (int i = 0; i < validCount; i++)
                {
                    samples[i].Weight /= totalWeight;
                }
            }

            return new BlendResult
            {
                Samples = samples,
                Mode = applicationMode,
                BlendWeight = blendWeight
            };
        }
    }
}