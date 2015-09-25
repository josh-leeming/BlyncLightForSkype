using System;
using System.Collections.Generic;
using System.Linq;
using Blynclight;
using BlyncLightForSkype.Client;
using BlyncLightForSkype.Client.BlyncLightBehaviours;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.SkypeBehaviours;
using TinyIoC;

namespace BlyncLightForSkype.App
{
    public static class IoC
    {
        public static void ConfigureContainer(TinyIoCContainer container)
        {
            container.Register(typeof(ILogHandler), typeof(Logger.Log4NetLogger)).AsSingleton();
            container.Register(typeof(IMessageRouter<IMessage>), typeof(MessageRouter)).AsSingleton();

            container.RegisterMultiple<IClientLifecycleCallbackHandler>(new List<Type>()
            {
                typeof (BlyncLightManager),
                typeof (SkypeManager)

            }).AsSingleton();

            container.RegisterMultiple<IBlyncLightBehaviour>(new List<Type>()
            {
                typeof (SkypeStatusResponder),
                typeof (BlyncLightScheduler)
            });

            container.RegisterMultiple<ISkypeBehaviour>(new List<Type>()
            {
                typeof (CallStatusNotifier),
                typeof (UserStatusNotifier),
                typeof (OnBreakBehaviour),
                typeof (OnLunchBehaviour),
                typeof (OnCallBehaviour)
            });

            //IBlyncLightBehaviour
            container.Register(typeof(SkypeStatusResponder),
                new SkypeStatusResponder(container.Resolve<IMessageRouter<IMessage>>()));

            //IClientLifecycleCallbackHandler
            container.Register(typeof(SkypeManager),
                new SkypeManager(container.Resolve<ILogHandler>(),
                    container.Resolve<IMessageRouter<IMessage>>(),
                    container.ResolveAll<ISkypeBehaviour>().OrderByDescending(s => s.Priority).ToList()));

            container.Register(typeof(BlyncLightManager),
                new BlyncLightManager(container.Resolve<ILogHandler>(),
                    container.Resolve<IMessageRouter<IMessage>>(),
                    container.ResolveAll<IBlyncLightBehaviour>().OrderByDescending(s => s.Priority).ToList()));

            //Client
            container.Register(typeof(BlyncLightForSkypeClient),
                new BlyncLightForSkypeClient());
        }
    }
}