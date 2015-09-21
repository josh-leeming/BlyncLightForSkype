using System;
using BlyncLightForSkype.Client.Messages;

namespace BlyncLightForSkype.Client.Interfaces
{
    public interface IMessageRouter<T>
    {
        MessageSubscriptionToken Subscribe(Action<T> handler, Predicate<T> predicate, SubscriptionPriority priority = SubscriptionPriority.Normal);
        bool Publish(T message);
    }
}