using System; 
using UnityEngine;
using TidiTweening;

namespace TidiMovementComponent2D.MovingPlatforms
{
    public class VisualInterpolator : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Vector3 _prevPos;
        private Vector3 _currPos;
        public EaseType EaseType = EaseType.Linear;
        private TidiTween<Vector3> _posTween;

        public Vector3 PivotOffset { get; set; }

        private void Start()
        {
            _rb = GetComponentInParent<Rigidbody2D>();
            transform.parent = null;
            _prevPos = _currPos = _rb.position;
        }

        public void UpdatePhysicsState()
        {
            _prevPos = _currPos;
            _currPos = (Vector3)_rb.position;
            _posTween?.FullKill();
            
            _posTween = TidiTweenManager.TweenVector3(this, _prevPos, _currPos, Time.fixedDeltaTime, (value) => 
            {
                transform.position = value + PivotOffset;
            }).SetEase(EaseType);   
        }

        public void ForceTeleport(Vector3 pos)
        {
            _prevPos = _currPos = pos;
            transform.position = pos;
            _rb.position = (Vector2)pos;
        }
    }
}
