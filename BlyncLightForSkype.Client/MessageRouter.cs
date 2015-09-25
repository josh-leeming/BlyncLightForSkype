using System;
using System.Collections.Generic;
using System.Linq;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Messages;

namespace BlyncLightForSkype.Client
{
    public class MessageRouter : IMessageRouter<IMessage>
    {
        private static readonly object _lock = new object();
        private static readonly List<EventSubscription<IMessage>> _subscriptions = new List<EventSubscription<IMessage>>();

        #region IMessageRouter

        /// <summary>
        /// Publish a message
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <returns>Flag indicating if the messaged was handled by at least 1 subscriber</returns>
        public bool Publish(IMessage message)
        {
            var handled = false;

            lock (_lock)
            {
                _subscriptions.OrderByDescending(s => s.Priority).ToList().ForEach(es =>
                {
                    if (es != null && es.Handler != null && es.Predicate != null)
                    {
                        if (es.Predicate.Invoke(message))
                        {
                            if (InvokeActionWrapper != null)
                            {
                                InvokeActionWrapper.Invoke(() => es.Handler.Invoke(message));
                            }
                            else
                            {
                                es.Handler.Invoke(message);
                            }
                            handled = true;
                        }
                    }
                });
            }
            return handled;
        }

        /// <summary>
        /// Subscribe to a message
        /// </summary>
        /// <param name="handler">Action to call when a message is present</param>
        /// <returns>A subscription token that can be used to modify this subscription</returns>
        public MessageSubscriptionToken Subscribe(Action<IMessage> handler)
        {
            return Subscribe(handler, T => true);
        }

        /// <summary>
        /// Subscribe to a message
        /// </summary>
        /// <param name="handler">Action to call when a message is present</param>
        /// <param name="predicate">Predicate to determine whether the message is appropriate for this subscriber</param>
        /// <param name="priority"></param>
        /// <returns>A subscription token that can be used to modify this subscription</returns>
        public MessageSubscriptionToken Subscribe(Action<IMessage> handler, Predicate<IMessage> predicate, SubscriptionPriority priority = SubscriptionPriority.Normal)
        {
            EventSubscription<IMessage> es;

            lock (_lock)
            {
                es = new EventSubscription<IMessage> { Handler = handler, Predicate = predicate };
                _subscriptions.Add(es);
            }
            return es.Token;
        }

        /// <summary>
        /// Action wrapper used to contain the invoked action. 
        /// </summary>
        public Action<Action> InvokeActionWrapper { get; set; }

        /// <summary>
        /// Unsubscribe from a message
        /// </summary>
        /// <param name="token">Subscription to unsubscribe</param>
        public void Unsubscribe(MessageSubscriptionToken token)
        {
            lock (_lock)
            {
                var sub = _subscriptions.SingleOrDefault(s => s.Token == token);
                if (sub != null)
                {
                    _subscriptions.Remove(sub);
                    sub.Dispose();
                }
            }
        }

        #endregion
    }
}
