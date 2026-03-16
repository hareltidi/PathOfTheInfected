using System;
using System.Collections.Generic;
using UnityEngine;

namespace TidiGameplayMessaging.Core
{
    public sealed class TidiGameplayMessagingSubsystem
    {
        private static readonly Lazy<TidiGameplayMessagingSubsystem> _instance =
            new(() => new TidiGameplayMessagingSubsystem());


        private readonly Dictionary<Type, List<Action<object>>> _listeners = new();


        private readonly Queue<QueuedMessage> _pendingMessages = new();
        private bool _isDrainingQueue;

        private TidiGameplayMessagingSubsystem()
        {
        }

        public static TidiGameplayMessagingSubsystem Instance => _instance.Value;

        public IDisposable Subscribe<TChannel, TPayload>(Action<TPayload> callback)
            where TChannel : TidiMessageChannel<TPayload>
            where TPayload : struct, ITidiGameplayPayload
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var channelType = typeof(TChannel);

            if (!_listeners.TryGetValue(channelType, out var list))
            {
                list = new List<Action<object>>();
                _listeners[channelType] = list;
            }

            Action<object> wrapper = boxedPayload => callback((TPayload)boxedPayload);
            list.Add(wrapper);

            return new Subscription(() =>
            {
                if (!_listeners.TryGetValue(channelType, out var currentList))
                    return;

                currentList.Remove(wrapper);

                if (currentList.Count == 0)
                {
                    _listeners.Remove(channelType);
                }
            });
        }

        public void Publish<TChannel, TPayload>(TPayload payload)
            where TChannel : TidiMessageChannel<TPayload>
            where TPayload : struct, ITidiGameplayPayload
        {
            var channelType = typeof(TChannel);

            _pendingMessages.Enqueue(new QueuedMessage(channelType, payload));

            if (!_isDrainingQueue)
            {
                DrainQueue();
            }
        }

        private void DrainQueue()
        {
            _isDrainingQueue = true;
            try
            {
                while (_pendingMessages.Count > 0)
                {
                    var message = _pendingMessages.Dequeue();

                    if (!_listeners.TryGetValue(message.ChannelType, out var list) || list.Count == 0)
                        continue;

                    // Snapshot prevents collection-modified exceptions if callbacks subscribe/unsubscribe.
                    var snapshot = list.ToArray();

                    foreach (var listener in snapshot)
                    {
                        try
                        {
                            listener(message.Payload);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
            finally
            {
                _isDrainingQueue = false;
            }
        }

        private readonly struct QueuedMessage
        {
            public readonly Type ChannelType;
            public readonly object Payload;

            public QueuedMessage(Type channelType, object payload)
            {
                ChannelType = channelType;
                Payload = payload;
            }
        }
    }
}