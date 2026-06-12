using TidiGameplayMessaging.Core;

namespace GlobalMessages
{

    public struct PlayerHealthChangedPayloadUI : ITidiGameplayPayload
    {
        public float NewHealth;
        public float MaxHealth;
    }


    public class OnPlayerHealthChangedUI : TidiMessageChannel<PlayerHealthChangedPayloadUI>
    {
    }
}