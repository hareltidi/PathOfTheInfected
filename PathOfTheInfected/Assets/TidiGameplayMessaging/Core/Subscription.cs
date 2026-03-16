using System;

namespace TidiGameplayMessaging.Core
{
    internal sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        public void Dispose()
        {
            if (_disposed) return;

            _unsubscribe?.Invoke();
            _disposed = true;
        }
    }
}