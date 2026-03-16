
namespace TidiGameplayMessaging.Core
{
    public abstract class TidiMessageChannel
    {
    }

    public abstract class TidiMessageChannel<TPayload>  : TidiMessageChannel where TPayload : struct, ITidiGameplayPayload
    {

    }
}