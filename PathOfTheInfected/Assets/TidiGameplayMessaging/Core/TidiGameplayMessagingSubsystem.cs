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

        /// <summary>
        /// Gets the singleton instance of the gameplay messaging subsystem.
        /// The instance is created lazily on first access.
        /// </summary>
        public static TidiGameplayMessagingSubsystem Instance => _instance.Value;


        /// <summary>
        /// Subscribes a callback to a typed message channel.
        /// </summary>
        /// <typeparam name="TChannel">
        /// The channel type used to route this message.
        /// Must inherit from <see cref="TidiMessageChannel{TPayload}"/>.
        /// </typeparam>
        /// <typeparam name="TPayload">
        /// The payload value type delivered to subscribers.
        /// Must be a <c>struct</c> implementing <see cref="ITidiGameplayPayload"/>.
        /// </typeparam>
        /// <param name="callback">
        /// The callback invoked when a message is published on <typeparamref name="TChannel"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IDisposable"/> token. Dispose it to unsubscribe the callback.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="callback"/> is <see langword="null"/>.
        /// </exception>
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

        /// <summary>
        /// Publishes a payload to the specified channel.
        /// Messages are enqueued and drained in order to support safe re-entrant publishing.
        /// </summary>
        /// <typeparam name="TChannel">
        /// The channel type that identifies the message route.
        /// </typeparam>
        /// <typeparam name="TPayload">
        /// The payload value type carried with the message.
        /// </typeparam>
        /// <param name="payload">
        /// The payload data sent to all current subscribers of the channel.
        /// </param>
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

        /// <summary>
        /// Drains the pending message queue and dispatches each message to subscribers.
        /// Uses a listener snapshot to avoid collection-modified exceptions during callbacks.
        /// </summary>
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

        /// <summary>
        /// Represents a queued message entry containing the destination channel and payload.
        /// </summary>
        private readonly struct QueuedMessage
        {
            public readonly Type ChannelType;
            public readonly object Payload;

            /// <summary>
            /// Creates a queued message entry.
            /// <param name="channelType">The message channel type to dispatch to.</param>
            /// <param name="payload">The boxed payload instance.</param>
            ///</summary>
            public QueuedMessage(Type channelType, object payload)
            {
                ChannelType = channelType;
                Payload = payload;
            }
        }
    }
}