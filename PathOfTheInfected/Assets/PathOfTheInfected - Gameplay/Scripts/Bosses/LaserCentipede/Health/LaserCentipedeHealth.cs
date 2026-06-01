using PathOfTheInfected.Combat;
using PathOfTheInfected.Core.Scripts.Boss;

namespace PathOfTheInfected.Gameplay.Scripts.Bosses.LaserCentipede.Health
{
    public class LaserCentipedeHealth : BossHealth
    {
        private LaserCentipedeBrain _laserCentipedeBrain;
        protected override void Awake()
        {
            base.Awake();
            _laserCentipedeBrain = (LaserCentipedeBrain) Owner;
        }
        public override HitResponse OnHit(ref HitData damageData)
        {
            if (_laserCentipedeBrain.IsInvisible)
            {
                return new HitResponse(
                    response: Response.Invincible,
                    consumeCharges: false,
                    finalDamage: 0
                );
            }
            return base.OnHit(ref damageData);
        }
    }
}