using System;
using System.Collections.Generic;
using TidiMovementComponent2D.Animation.BlendSpaces;
using UnityEngine;

namespace PathOfTheInfected.Animation.BlendSpaces
{
    [Serializable]
    public struct Blend1DFloatData : IBlendable
    {
        public AnimationClip clip;
        public float threshold;
    }


    [CreateAssetMenu(fileName = "BlendSpace1DFloat", menuName = "Blend Space/Float/BlendSpace1DFloat", order = 10)]
    public class BlendSpace1DFloat : TidiBaseBlendSpace<float>
    {
        [SerializeField] private List<Blend1DFloatData> blendPoints = new();
        [SerializeField] private BlendApplicationMode applicationMode = BlendApplicationMode.Override;
        [SerializeField] [Range(0f, 1f)] private float blendWeight = 1f;


        public void OnValidate()
        {
            if (blendPoints == null || blendPoints.Count == 0)
            {
                Debug.LogWarning("The array is null or has 0 elements. Please set an element!", this);
                return;
            }

            blendPoints.Sort((a, b) => a.threshold.CompareTo(b.threshold));

            foreach (var point in blendPoints)
                if (point.clip == null)
                    Debug.LogWarning("One of the blend points has a null clip reference!", this);
        }

        public override BlendResult Evaluate(float input)
        {
            if (blendPoints == null || blendPoints.Count == 0)
                return default;

            // Optional safety: if only one point, return it directly
            if (blendPoints.Count == 1)
                return new BlendResult
                {
                    Mode = applicationMode,
                    BlendWeight = blendWeight,
                    Samples = new[]
                    {
                        new BlendSample
                        {
                            Clip = blendPoints[0].clip,
                            Weight = 1f
                        }
                    }
                };

            if (input <= blendPoints[0].threshold)
                return new BlendResult
                {
                    Mode = applicationMode,
                    BlendWeight = blendWeight,
                    Samples = new[]
                    {
                        new BlendSample
                        {
                            Clip = blendPoints[0].clip,
                            Weight = 1f
                        }
                    }
                };

            var last = blendPoints[blendPoints.Count - 1];
            if (input >= last.threshold)
                return new BlendResult
                {
                    Mode = applicationMode,
                    BlendWeight = blendWeight,
                    Samples = new[]
                    {
                        new BlendSample
                        {
                            Clip = last.clip,
                            Weight = 1f
                        }
                    }
                };

            // Find closest left and right points
            var left = blendPoints[0];
            var right = blendPoints[0];

            var leftThreshold = float.NegativeInfinity;
            var rightThreshold = float.PositiveInfinity;

            for (var i = 0; i < blendPoints.Count; i++)
            {
                var t = blendPoints[i].threshold;

                // best left candidate (<= input)
                if (t <= input && t > leftThreshold)
                {
                    left = blendPoints[i];
                    leftThreshold = t;
                }

                // best right candidate (>= input)
                if (t >= input && t < rightThreshold)
                {
                    right = blendPoints[i];
                    rightThreshold = t;
                }
            }

            // Edge cases: clamp if outside range
            if (left.clip == null) left = right;
            if (right.clip == null) right = left;

            // If both points are identical → no blend needed
            if (Mathf.Approximately(left.threshold, right.threshold))
                return new BlendResult
                {
                    Mode = applicationMode,
                    BlendWeight = blendWeight,
                    Samples = new[]
                    {
                        new BlendSample
                        {
                            Clip = left.clip,
                            Weight = 1f
                        }
                    }
                };

            // Compute interpolation factor (0–1)
            var range = right.threshold - left.threshold;

            var t01 = range <= 0f
                ? 0f
                : (input - left.threshold) / range;
            t01 = Mathf.Clamp01(t01);

            // Build result (2-way blend)
            return new BlendResult
            {
                Mode = applicationMode,
                BlendWeight = blendWeight,
                Samples = new[]
                {
                    new BlendSample
                    {
                        Clip = left.clip,
                        Weight = 1f - t01
                    },
                    new BlendSample
                    {
                        Clip = right.clip,
                        Weight = t01
                    }
                }
            };
        }
    }
}