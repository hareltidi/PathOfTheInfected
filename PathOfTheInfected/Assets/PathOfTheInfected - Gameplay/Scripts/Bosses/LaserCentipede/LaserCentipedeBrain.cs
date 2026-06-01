using PathOfTheInfected.Core.Scripts.Boss;

namespace PathOfTheInfected.Gameplay.Scripts.Bosses.LaserCentipede
{
    public class LaserCentipedeBrain : BossBrain
    {
        public bool IsInvisible => IsGrounded;
    }
}