using System;

namespace BlyncLightForSkype.Client.Messages
{
    //public class MessageRouter
    //{
    //    #region Dependencies

    //    public ILogHandler Logger { get; set; }

    //    #endregion

    //    #region Props

    //    private static readonly object _lock = new object();
    //    private static readonly List<EventSubscription<UdpMessage>> _subscriptions = new List<EventSubscription<UdpMessage>>(); 

    //    #endregion

    //    #region Ctor

    //    public MessageRouter(ILogHandler logger)
    //    {
    //        Logger = logger;
    //    } 

    //    #endregion

    //    /// <summary>
    //    /// Publish a message
    //    /// </summary>
    //    /// <param name="message">Message to publish</param>
    //    /// <returns>Flag indicating if the messaged was handled by at least 1 subscriber</returns>
    //    public bool Publish(UdpMessage message)
    //    {
    //        var handled = false;

    //        lock (_lock)
    //        {
    //            _subscriptions.OrderByDescending(s => s.Priority).ToList().ForEach(es =>
    //            {
    //                if (es != null && es.Handler != null && es.Predicate != null)
    //                {
    //                    if (es.Predicate.Invoke(message))
    //                    {
    //                        if (InvokeActionWrapper != null)
    //                        {
    //                            InvokeActionWrapper.Invoke(() => es.Handler.Invoke(message));
    //                        }
    //                        else
    //                        {
    //                            es.Handler.Invoke(message);
    //                        }
    //                        handled = true;
    //                    }
    //                }
    //            });
    //        }
    //        return handled;
    //    }

    //    /// <summary>
    //    /// Subscribe to a message
    //    /// </summary>
    //    /// <param name="handler">Action to call when a message is present</param>
    //    /// <returns>A subscription token that can be used to modify this subscription</returns>
    //    public MessageSubscriptionToken Subscribe(Action<UdpMessage> handler)
    //    {
    //        return Subscribe(handler, T => true);
    //    }

    //    /// <summary>
    //    /// Subscribe to a message
    //    /// </summary>
    //    /// <param name="handler">Action to call when a message is present</param>
    //    /// <param name="predicate">Predicate to determine whether the message is appropriate for this subscriber</param>
    //    /// <param name="priority"></param>
    //    /// <returns>A subscription token that can be used to modify this subscription</returns>
    //    public MessageSubscriptionToken Subscribe(Action<UdpMessage> handler, Predicate<UdpMessage> predicate, SubscriptionPriority priority = SubscriptionPriority.Normal)
    //    {
    //        EventSubscription<UdpMessage> es;

    //        lock (_lock)
    //        {
    //            es = new EventSubscription<UdpMessage> { Handler = handler, Predicate = predicate };
    //            _subscriptions.Add(es);
    //        }
    //        return es.Token;
    //    }

    //    /// <summary>
    //    /// Action wrapper used to contain the invoked action. 
    //    /// </summary>
    //    public Action<Action> InvokeActionWrapper { get; set; }

    //    /// <summary>
    //    /// Unsubscribe from a message
    //    /// </summary>
    //    /// <param name="token">Subscription to unsubscribe</param>
    //    public void Unsubscribe(MessageSubscriptionToken token)
    //    {
    //        lock (_lock)
    //        {
    //            var sub = _subscriptions.SingleOrDefault(s => s.Token == token);
    //            if (sub != null)
    //            {
    //                _subscriptions.Remove(sub);
    //                sub.Dispose();
    //            }
    //        }
    //    }
    //}

    internal class EventSubscription<T>
    {
        public EventSubscription()
        {
            Token = new MessageSubscriptionToken() { Token = Guid.NewGuid() };
        }
        public MessageSubscriptionToken Token { get; set; }
        public Action<T> Handler { get; set; }
        public Predicate<T> Predicate { get; set; }
        public SubscriptionPriority Priority { get; set; }

        public void Dispose()
        {
            Handler = null;
            Predicate = null;
        }
    }
}
