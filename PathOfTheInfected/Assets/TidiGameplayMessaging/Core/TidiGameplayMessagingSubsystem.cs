using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace TidiGameplayMessaging.Core
{
	/// <summary>
	/// A singleton subsystem that manages the subscription and publication of typed gameplay messages.
	/// </summary>
	public sealed class TidiGameplayMessagingSubsystem
	{
		private static readonly Lazy<TidiGameplayMessagingSubsystem> _instance =
			new(() => new TidiGameplayMessagingSubsystem());

		private readonly Dictionary<Type, IListenerList> _listeners = new();


		private readonly Queue<QueuedMessageBase> _pendingMessages = new();
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
		public IDisposable Listen<TChannel, TPayload>(Action<TPayload> callback)
			where TChannel : TidiMessageChannel<TPayload>
			where TPayload : struct, ITidiGameplayPayload
		{
			if (callback == null) throw new ArgumentNullException(nameof(callback));

			var channelType = typeof(TChannel);

			if (!_listeners.TryGetValue(channelType, out var listenerListBase))
			{
				var typedList = new ListenerList<TPayload>();
				_listeners[channelType] = typedList;
				listenerListBase = typedList;
			}

			var listenerList = (ListenerList<TPayload>)listenerListBase;
			listenerList.Add(callback);

			return new Subscription(() =>
			{
				if (!_listeners.TryGetValue(channelType, out var currentListenerListBase)) return;

				var currentList = (ListenerList<TPayload>)currentListenerListBase;
				currentList.Remove(callback);

				if (currentList.Count == 0)
				{
					_listeners.Remove(channelType);
				}
			});
		}


		public IDisposable Listen<TChannel>(Action callback)
			where TChannel : TidiMessageChannel
		{
			if (callback == null) throw new ArgumentNullException(nameof(callback));

			var channelType = typeof(TChannel);

			if (IsPayloadChannel(in channelType))
			{
				throw new InvalidOperationException(
					$"{channelType.Name} is a payload channel. Use Subscribe<TChannel, TPayload>(Action<TPayload>) instead.");
			}

			if (!_listeners.TryGetValue(channelType, out var listenerListBase))
			{
				var typedList = new ListenerListNoPayload();
				_listeners[channelType] = typedList;
				listenerListBase = typedList;
			}

			var listenerList = (ListenerListNoPayload)listenerListBase;
			listenerList.Add(callback);

			return new Subscription(() =>
			{
				if (!_listeners.TryGetValue(channelType, out var currentListenerListBase)) return;

				var currentList = (ListenerListNoPayload)currentListenerListBase;
				currentList.Remove(callback);

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
		public void Broadcast<TChannel, TPayload>(in TPayload payload)
			where TChannel : TidiMessageChannel<TPayload>
			where TPayload : struct, ITidiGameplayPayload
		{
			var channelType = typeof(TChannel);

			_pendingMessages.Enqueue(new QueuedMessage<TPayload>(channelType, payload));

			if (!_isDrainingQueue)
			{
				DrainQueue();
			}
		}

		public void Broadcast<TChannel>()
			where TChannel : TidiMessageChannel
		{
			var channelType = typeof(TChannel);

			if (IsPayloadChannel(in channelType))
			{
				throw new InvalidOperationException(
					$"{channelType.Name} is a payload channel. Use Publish<TChannel, TPayload>(payload) instead.");
			}

			_pendingMessages.Enqueue(new QueuedMessageNoPayload(channelType));

			if (!_isDrainingQueue)
			{
				DrainQueue();
			}
		}

		/// <summary>
		/// Drains the pending message queue and dispatches each message to subscribers.
		/// Uses type-preserved listener lists to avoid boxing payloads during dispatch.
		/// </summary>
		private void DrainQueue()
		{
			_isDrainingQueue = true;
			try
			{
				while (_pendingMessages.Count > 0)
				{
					var message = _pendingMessages.Dequeue();

					if (!_listeners.TryGetValue(message.ChannelType, out var listenerList) || listenerList.Count == 0)
						continue;

					message.Dispatch(listenerList);
				}
			}
			finally
			{
				_isDrainingQueue = false;
			}
		}


		/// <summary>
		/// Method to check if our channel must have a payload or not by checking if it inherits
		/// from TidiMessageChannel generic type.
		/// </summary>
		/// <param name="channelType">The channel to check on</param>
		/// <returns>If our channel must have a payload</returns>
		private static bool IsPayloadChannel(in Type channelType)
		{
			for (var current = channelType; current != null && current != typeof(object); current = current.BaseType)
			{
				if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(TidiMessageChannel<>))
                {
                    return true;
                }
			}

			return false;
		}



        #region Listener list container
        /// <summary>
        /// Interface for type-erased listener lists.
        /// Allows polymorphic storage of generic listener lists in a single dictionary.
        /// </summary>
        private interface IListenerList
        {
            int Count { get; }
        }

        /// <summary>
        /// Generic listener list that stores callbacks without boxing.
        /// </summary>
        private sealed class ListenerList<TPayload> : IListenerList
            where TPayload : struct, ITidiGameplayPayload
        {
            private readonly List<Action<TPayload>> _callbacks = new();

            public int Count => _callbacks.Count;

            public void Add(Action<TPayload> callback)
            {
                _callbacks.Add(callback);
            }

            public void Remove(Action<TPayload> callback)
            {
                _callbacks.Remove(callback);
            }

            public void InvokeAll(in TPayload payload)
            {
                // Rent pooled array to avoid allocations
                int callbackCount = _callbacks.Count;
                if (callbackCount == 0) return;
                var snapshot = ArrayPool<Action<TPayload>>.Shared.Rent(callbackCount);
                _callbacks.CopyTo(0, snapshot, 0, callbackCount);
                try
                {
                    for (int i = 0; i < callbackCount; i++)
                    {
                        try
                        {
                            snapshot[i](payload);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
                finally
                {
                    Array.Clear(snapshot, 0, callbackCount);
                    ArrayPool<Action<TPayload>>.Shared.Return(snapshot);
                }
            }
        }

        /// <summary>
        /// Zero-payload listener list for signal-only channels.
        /// </summary>
        private sealed class ListenerListNoPayload : IListenerList
        {
            private readonly List<Action> _callbacks = new();

            public int Count => _callbacks.Count;

            public void Add(Action callback)
            {
                _callbacks.Add(callback);
            }

            public void Remove(Action callback)
            {
                _callbacks.Remove(callback);
            }

            public void InvokeAll()
            {
                int callbackCount = _callbacks.Count;
                if (callbackCount == 0) return;
                // Rent pooled array to avoid allocations
                var snapshot = ArrayPool<Action>.Shared.Rent(callbackCount);
                _callbacks.CopyTo(0, snapshot, 0, callbackCount);
                try
                {
                    for (int i = 0; i < callbackCount; i++)
                    {
                        try
                        {
                            snapshot[i]();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
                finally
                {
                    Array.Clear(snapshot, 0, callbackCount);
                    ArrayPool<Action>.Shared.Return(snapshot);
                }
            }
        }
        #endregion

        #region Queued message handlers
        /// <summary>
        /// Base class for queued message entries.
        /// Enables polymorphic dispatch of heterogeneous generic messages within a single queue.
        /// </summary>
        private abstract class QueuedMessageBase
        {
            public abstract Type ChannelType { get; }

            /// <summary>
            /// Dispatches this message to all listeners in the provided listener list.
            /// No boxing occurs; dispatch is type-safe end-to-end.
            /// </summary>
            /// <param name="listenerList">The type-specific listener list to invoke.</param>
            public abstract void Dispatch(IListenerList listenerList);
        }

        /// <summary>
        /// Generic queued message that stores an unboxed payload struct.
        /// Avoids boxing overhead during enqueue and dispatch.
        /// </summary>
        private sealed class QueuedMessage<TPayload> : QueuedMessageBase
            where TPayload : struct, ITidiGameplayPayload
        {
            public override Type ChannelType { get; }
            private readonly TPayload _payload;

            public QueuedMessage(Type channelType, in TPayload payload)
            {
                ChannelType = channelType;
                _payload = payload;
            }

            public override void Dispatch(IListenerList listenerList)
            {
                var typedList = (ListenerList<TPayload>)listenerList;
                typedList.InvokeAll(in _payload);
            }
        }

        /// <summary>
        /// Queued message for zero-payload (signal-only) channels.
        /// </summary>
        private sealed class QueuedMessageNoPayload : QueuedMessageBase
        {
            public override Type ChannelType { get; }

            public QueuedMessageNoPayload(Type channelType)
            {
                ChannelType = channelType;
            }

            public override void Dispatch(IListenerList listenerList)
            {
                var typedList = (ListenerListNoPayload)listenerList;
                typedList.InvokeAll();
            }
        }
        #endregion
	}
}