using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TidiTweening
{
    public class TidiTweenManager : MonoBehaviour
    {
        private static TidiTweenManager _instance;
        public static TidiTweenManager Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject tweenManagerObject = new GameObject("TidiTweenManager");
                    _instance = tweenManagerObject.AddComponent<TidiTweenManager>();
                }
                return _instance;
            }
            
        }

        private Dictionary<string, ITidiTween> _activeTweens = new();


        private void Update()
        {
            foreach (var pair in _activeTweens.ToList())
            {
                ITidiTween tween = pair.Value;
                tween.Update();

                if (tween.IsComplete && tween.OnComplete != null && !tween.WasKilled)
                {
                    tween.OnComplete.Invoke();
                    tween.OnComplete = null;
                    RemoveTween(tween.Identifier);
                }

                if (tween.WasKilled)
                {
                    RemoveTween(tween.Identifier);
                }
            }
        }


        #region Tween Creation and Deletion
        public void AddTween<T>(TidiTween<T> tween)
        {
            if (_activeTweens.ContainsKey(tween.Identifier))
            {
                _activeTweens[tween.Identifier].OnCompleteKill();
            }
            _activeTweens[tween.Identifier] = tween;
        }

        public void RemoveTween(string identifier)
        {
            _activeTweens.Remove(identifier);
        }


        #endregion




        #region Static Methods and helpers - Normal lerp

        public static TidiTween<float> TweenFloat(object target, float startValue, float endValue, float duration, Action<float> setter)
        {
            string identifier = $"{target}_FloatTween";
            TidiTween<float> tween = new TidiTween<float>(target, identifier, startValue, endValue, duration, (value) =>
            {
                setter(value);
            });
            return tween;
        }

        public static TidiTween<Vector2> TweenVector2(object target, Vector2 startValue, Vector2 endValue, float duration, Action<Vector2> setter)
        {
            string identifier = $"{target}_Vector2Tween";
            TidiTween<Vector2> tween = new TidiTween<Vector2>(target, identifier, startValue, endValue, duration, (value) =>
            {
                setter(value);
            });
            return tween;
        }

        public static TidiTween<Vector3> TweenVector3(object target, Vector3 startValue, Vector3 endValue, float duration, Action<Vector3> setter)
        {
            string identifier = $"{target}_Vector3Tween";
            TidiTween<Vector3> tween = new TidiTween<Vector3>(target, identifier, startValue, endValue, duration, (value) =>
            {
                setter(value);
            });
            return tween;
        }

        public static TidiTween<Color> TweenColor(object target, Color startValue, Color endValue, float duration, Action<Color> setter)
        {
            string identifier = $"{target}_ColorTween";
            TidiTween<Color> tween = new TidiTween<Color>(target, identifier, startValue, endValue, duration, (value) =>
            {
                setter(value);
            });
            return tween;
        }
        #endregion
    }

}