using System;
using System.Collections.Generic;
using TidiMovementComponent2D.Animation.BlendSpaces;
using UnityEngine;

namespace PathOfTheInfected.Animation.BlendSpaces
{
    [System.Serializable]
    public struct Blend1DFloatData : IBlendable
    {
        public AnimationClip clip;
        public float threshold;
    }


    [CreateAssetMenu(fileName = "BlendSpace1DFloat", menuName = "Blend Space/Float/BlendSpace1DFloat", order = 5)]
    public class BlendSpace1DFloat : TidiBaseBlendSpace<float>
    {
        [SerializeField] private List<Blend1DFloatData> blendPoints = new ();
        public override int Resolve(float input)
        {
            if (blendPoints == null || blendPoints.Count == 0)
            {
                throw new InvalidOperationException("BlendPoints list is empty!");
            }


            Blend1DFloatData closestPoint = new();

           foreach(var point in blendPoints)
           {
               if (Mathf.Abs(point.threshold - input) < Mathf.Abs(closestPoint.threshold - input))
               {
                   closestPoint = point;
               }
           }

           return Animator.StringToHash(closestPoint.clip.name);
        }

        public void OnValidate()
        {
            if (blendPoints == null || blendPoints.Count == 0)
            {
                Debug.LogWarning("The array is null or has 0 elements. Please set an element!", this);
                return;
            }

            foreach (var point in blendPoints)
            {
                if (point.clip == null)
                {
                    Debug.LogWarning("One of the blend points has a null clip reference!", this);
                }
            }
        }
    }
}