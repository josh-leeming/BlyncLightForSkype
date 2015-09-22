using System;
using System.Collections.Generic;
using System.Linq;
using Blynclight;
using BlyncLightForSkype.Client;
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
            container.Register(typeof(BlynclightController), typeof(BlynclightController)).AsSingleton();

            container.Register(typeof(IMessageRouter<ISkypeMessage>), typeof(SkypeMessageRouter)).AsSingleton();
            container.Register(typeof(IMessageRouter<IBlyncLightMessage>), typeof(BlyncLightMessageRouter)).AsSingleton();

            container.RegisterMultiple<IClientLifecycleCallbackHandler>(new List<Type>()
            {
                typeof (SkypeManager),
                typeof (BlyncLightManager)

            }).AsSingleton();

            container.RegisterMultiple<ISkypeBehaviour>(new List<Type>()
            {
                typeof (CallStatusNotifier),
                typeof (UserStatusNotifier),
                typeof (OnBreakBehaviour),
                typeof (OnLunchBehaviour),
                typeof (OnCallBehaviour)
            });

            container.Register(typeof(SkypeManager),
                new SkypeManager(container.Resolve<ILogHandler>(),
                    container.Resolve<IMessageRouter<ISkypeMessage>>(),
                    container.ResolveAll<ISkypeBehaviour>().OrderByDescending(s => s.Priority).ToList()));

            container.Register(typeof(BlyncLightManager),
                new BlyncLightManager(container.Resolve<ILogHandler>(),
                    container.Resolve<IMessageRouter<IBlyncLightMessage>>(),
                    container.Resolve<BlynclightController>()));

            container.Register(typeof(BlyncLightForSkypeClient),
                new BlyncLightForSkypeClient());
        }
    }
}