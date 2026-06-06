using PathOfTheInfected.Core.Scripts.Boss;
using PathOfTheInfected.Gameplay.Scripts.Bosses.LaserCentipede.Animation;
using TidiTweening;
using UnityEngine;

namespace PathOfTheInfected.Gameplay.Scripts.Bosses.LaserCentipede
{
    public class LaserCentipedeBrain : BossBrain
    {
        public bool IsInvincible => IsGrounded;
        public LaserCentipedeAnimInstance AnimInstance { get; private set; }

        [ColorUsage(true, true)]
        [SerializeField] private Color vulnerableColor;

        protected override void BossAwake()
        {
            base.BossAwake();
            AnimInstance = GetComponentInChildren<LaserCentipedeAnimInstance>();
        }

        protected override void BossUpdate()
        {
            base.BossUpdate();
            if (!IsInvincible)
            {
                // make the boss flash a different color when vulnerable
               
            }
        }
    }
}