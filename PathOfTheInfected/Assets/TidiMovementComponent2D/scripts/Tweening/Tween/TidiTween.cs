using System;
using UnityEngine;

namespace TidiTweening
{
    public class TidiTween<T> : ITidiTween
    {
        #region ITween Members
        public object Target { get; set; }
        public bool IsComplete { get; set; }
        public bool WasKilled { get; set; }
        public bool IsPaused { get; set; }
        public bool IgnoreTimeScale { get; set; }
        public string Identifier { get; set; }
        public float DelayTime { get; set; }
        public Action OnComplete { get; set; }
        #endregion

        #region Member Variables
        private T _startValue;
        private T _endValue;
        private float _duration;
        private Action<T> _onTweenUpdate;
        private float _elapsedTime = 0f;
        private float _delayElapsed = 0f;
        private int _loopsCompleted = 0;
        private bool _reverse = false;
        private bool _pingPong = false;
        private bool _constantSpeed = false;
        private int _loopCount = 1;
        private float _percentThreshold = -1f;
        private Action _onUpdate;
        private Action _onPercentCompleted;
        private EaseType _easeType = EaseType.Linear;
        #endregion

        public TidiTween(object target, string identifier, T startValue, T endValue,float duration, Action<T> onTweenUpdate)
        {
            Target = target;
            Identifier = identifier;
            _startValue = startValue;
            _endValue = endValue;
            _duration = duration;
            _onTweenUpdate = onTweenUpdate;
            TidiTweenManager.Instance.AddTween<T>(this);
        }


        public void Update()
        {
            if (IsPaused) return;
            if (IgnoreTimeScale)
            {
                _delayElapsed += Time.unscaledDeltaTime;
            }
            else
            {
                _delayElapsed += Time.deltaTime;
            }

            if (_delayElapsed < DelayTime) return;
            if (IsComplete) return;

            if (IsTargetDestroyed())
            {
                FullKill();
                return;
            }


            if (IgnoreTimeScale)
            {
                _elapsedTime += Time.unscaledDeltaTime;
            }
            else
            {
                _elapsedTime += Time.deltaTime;
            }

            float t;
            float easedT;
            if (_duration <= 0f)
            {
                t = 1f;
                easedT = 1f;
            }
            else
            {
                t = Mathf.Clamp01(_elapsedTime / _duration);
                easedT = Ease(_easeType, t);
            }

            T currentValue;
            if (_reverse)
            {
                currentValue = Interpolate(_endValue, _startValue, easedT, _constantSpeed);
            }
            else
            {
                currentValue = Interpolate(_startValue, _endValue, easedT, _constantSpeed);
            }
            _onUpdate?.Invoke();
            _onTweenUpdate?.Invoke(currentValue);

            if (_percentThreshold > 0f && t >= _percentThreshold)
            {
                _onPercentCompleted?.Invoke();
                _percentThreshold = -1f;

            }

            if (_elapsedTime >= _duration)
            {
                _loopsCompleted++;
                _elapsedTime = 0f;
                if (_pingPong)
                {
                    _reverse = !_reverse;
                }

                if (_loopCount > 0 && _loopsCompleted >= _loopCount)
                {
                    OnCompleteKill();
                    IsComplete = true;
                }

            }
        }

        public T Interpolate(T start, T end, float t, bool constantSpeed)
        {
            if (start is float startFloat && end is float endFloat)
            {
                if (!constantSpeed)
                {
                    return (T)(object)Mathf.LerpUnclamped(startFloat, endFloat, t);
                }
                return (T)(object)Mathf.MoveTowards(startFloat, endFloat, t);
            }

            if (start is Vector2 startVector2 && end is Vector2 endVector2)
            {
                if (!constantSpeed)
                {
                    return (T)(object)Vector2.LerpUnclamped(startVector2, endVector2, t);
                }

                return (T)(object)Vector2.MoveTowards(startVector2, endVector2, t);
            }

            if (start is Vector3 startVector3 && end is Vector3 endVector3)
            {
                if (!constantSpeed)
                {
                    return (T)(object)Vector3.LerpUnclamped(startVector3, endVector3, t);
                }

                return (T)(object)Vector3.MoveTowards(startVector3, endVector3, t);
            }

            if (start is Color startColor && end is Color endColor)
            {
                return (T)(object) Color.Lerp(startColor, endColor, t);
            }
            throw new NotImplementedException($"Tween interpolation of type {typeof(T)} is not implemented!");
        }

        public void OnCompleteKill()
        {
            IsComplete = true;
            _onTweenUpdate = null;
            _onUpdate = null;
            _onPercentCompleted = null;
        }

        public void FullKill()
        {
            OnCompleteKill();
            WasKilled = true;

        }

        public bool IsTargetDestroyed()
        {
            if (Target is MonoBehaviour mono && !mono)
            {
                return true;
            }

            if (Target is GameObject go && !go)
            {
                return true;
            }

            if (Target is Delegate del && del.Target == null)
            {
                return true;
            }

            return false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }




        #region Easing Calculations
        public static float Linear(float t)
        {
            return t;
        }

        // --------------------
        // Quadratic
        // --------------------

        public static float EaseInQuad(float t)
        {
            return t * t;
        }

        public static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f
                ? 2f * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        // --------------------
        // Cubic
        // --------------------

        public static float EaseInCubic(float t)
        {
            return t * t * t;
        }

        public static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        // --------------------
        // Quartic
        // --------------------

        public static float EaseInQuart(float t)
        {
            return t * t * t * t;
        }

        public static float EaseOutQuart(float t)
        {
            return 1f - Mathf.Pow(1f - t, 4f);
        }

        public static float EaseInOutQuart(float t)
        {
            return t < 0.5f
                ? 8f * Mathf.Pow(t, 4f)
                : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;
        }

        // --------------------
        // Quintic
        // --------------------

        public static float EaseInQuint(float t)
        {
            return t * t * t * t * t;
        }

        public static float EaseOutQuint(float t)
        {
            return 1f - Mathf.Pow(1f - t, 5f);
        }

        public static float EaseInOutQuint(float t)
        {
            return t < 0.5f
                ? 16f * Mathf.Pow(t, 5f)
                : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;
        }

        // --------------------
        // Sine
        // --------------------

        public static float EaseInSine(float t)
        {
            return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
        }

        public static float EaseOutSine(float t)
        {
            return Mathf.Sin((t * Mathf.PI) / 2f);
        }

        public static float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        }

        // --------------------
        // Exponential
        // --------------------

        public static float EaseInExpo(float t)
        {
            return t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
        }

        public static float EaseOutExpo(float t)
        {
            return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
        }

        public static float EaseInOutExpo(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;

            return t < 0.5f
                ? Mathf.Pow(2f, 20f * t - 10f) / 2f
                : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
        }

        // --------------------
        // Circular
        // --------------------

        public static float EaseInCirc(float t)
        {
            return 1f - Mathf.Sqrt(1f - t * t);
        }

        public static float EaseOutCirc(float t)
        {
            return Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f));
        }

        public static float EaseInOutCirc(float t)
        {
            return t < 0.5f
                ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f
                : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
        }

        // --------------------
        // Back (Overshoot)
        // --------------------

        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return c3 * t * t * t - c1 * t * t;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (2f * t - 2f) + c2) + 2f) / 2f;
        }

        // --------------------
        // Elastic
        // --------------------

        public static float ElasticEaseIn(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;

            const float c4 = (2f * Mathf.PI) / 3f;
            return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
        }


        public static float ElasticEaseOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;

            const float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }


        public static float ElasticEaseInOut(float t)
        {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;

            const float c5 = (2f * Mathf.PI) / 4.5f;

            return t < 0.5f
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f
                : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
        }


        // --------------------
        // Bounce
        // --------------------

        public static float BounceEaseOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
                return n1 * t * t;
            else if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return n1 * t * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return n1 * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / d1;
                return n1 * t * t + 0.984375f;
            }
        }

        public static float BounceEaseIn(float t)
        {
            return 1f - BounceEaseOut(1f - t);
        }


        public static float BounceEaseInOut(float t)
        {
            return t < 0.5f
                ? (1f - BounceEaseOut(1f - 2f * t)) / 2f
                : (1f + BounceEaseOut(2f * t - 1f)) / 2f;
        }






        #endregion

        #region Easing Function
        public static float Ease(EaseType equation, float t)
        {
            t = Mathf.Clamp01(t);

            switch (equation)
            {
                case EaseType.Linear: return Linear(t);

                case EaseType.EaseInQuad: return EaseInQuad(t);
                case EaseType.EaseOutQuad: return EaseOutQuad(t);
                case EaseType.EaseInOutQuad: return EaseInOutQuad(t);

                case EaseType.EaseInCubic: return EaseInCubic(t);
                case EaseType.EaseOutCubic: return EaseOutCubic(t);
                case EaseType.EaseInOutCubic: return EaseInOutCubic(t);

                case EaseType.EaseInQuart: return EaseInQuart(t);
                case EaseType.EaseOutQuart: return EaseOutQuart(t);
                case EaseType.EaseInOutQuart: return EaseInOutQuart(t);

                case EaseType.EaseInQuint: return EaseInQuint(t);
                case EaseType.EaseOutQuint: return EaseOutQuint(t);
                case EaseType.EaseInOutQuint: return EaseInOutQuint(t);

                case EaseType.EaseInSine: return EaseInSine(t);
                case EaseType.EaseOutSine: return EaseOutSine(t);
                case EaseType.EaseInOutSine: return EaseInOutSine(t);

                case EaseType.EaseInExpo: return EaseInExpo(t);
                case EaseType.EaseOutExpo: return EaseOutExpo(t);
                case EaseType.EaseInOutExpo: return EaseInOutExpo(t);

                case EaseType.EaseInCirc: return EaseInCirc(t);
                case EaseType.EaseOutCirc: return EaseOutCirc(t);
                case EaseType.EaseInOutCirc: return EaseInOutCirc(t);

                case EaseType.EaseInBack: return EaseInBack(t);
                case EaseType.EaseOutBack: return EaseOutBack(t);
                case EaseType.EaseInOutBack: return EaseInOutBack(t);

                
                case EaseType.ElasticEaseIn: return ElasticEaseIn(t);
                case EaseType.ElasticEaseOut: return ElasticEaseOut(t);
                case EaseType.ElasticEaseInOut: return ElasticEaseInOut(t);

     
                case EaseType.BounceEaseIn: return BounceEaseIn(t);
                case EaseType.BounceEaseOut: return BounceEaseOut(t);
                case EaseType.BounceEaseInOut: return BounceEaseInOut(t);

                default:
                    return t;
            }
        }

        #endregion

        #region Method Chaining
        public TidiTween<T> SetEase(EaseType easeType)
        {
            _easeType = easeType;
            return this;
        }

        public TidiTween<T> SetPingPong(int loopCount)
        {
            _loopCount = loopCount;
            _pingPong = true;
            return this;
        }

        public TidiTween<T> SetOnComplete(Action onComplete)
        {
            OnComplete = onComplete;
            return this;
        }

        public TidiTween<T> SetIgnoreTimeScale()
        {
            IgnoreTimeScale = true;
            return this;
        }

        public TidiTween<T> SetOnUpdate(Action onUpdate)
        {
            _onUpdate = onUpdate;
            return this;
        }

        public TidiTween<T> SetOnPercentCompleted(float percentCompleted, Action onPercentCompleted)
        {
            _percentThreshold = Mathf.Clamp01(percentCompleted);
            _onPercentCompleted = onPercentCompleted;
            return this;
        }

        public TidiTween<T> SetStartDelay(float delay)
        {
            DelayTime = delay;
            return this;
        }

        public TidiTween<T> SetConstantSpeed()
        {
            _constantSpeed = true;
            return this;
        }

        #endregion




    }
}

