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
    }
}