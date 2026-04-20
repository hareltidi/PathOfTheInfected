using TidiGameplayMessaging.Core;

namespace PathOfTheInfected.Damagable.Messages
{

    public enum HealthChangeType
    {
        Damage,
        Heal,
        Init
    }

    public struct PlayerHealthChangedPayload : ITidiGameplayPayload
    {
        public float NewHealth;
        public HealthChangeType Type;
    }


    public class PlayerHitChannel : TidiMessageChannel<PlayerHealthChangedPayload>
    {

    }
}