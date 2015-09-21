using System;
using System.Collections.Generic;
using Blynclight;
using BlyncLightForSkype.Client;
using BlyncLightForSkype.Client.Interfaces;
using SKYPE4COMLib;
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

            container.Register(typeof(SkypeManager),
                new SkypeManager(container.Resolve<ILogHandler>(),
                    container.Resolve<IMessageRouter<ISkypeMessage>>()));

            container.Register(typeof(BlyncLightManager),
                new BlyncLightManager(container.Resolve<ILogHandler>(),
                    container.Resolve<IMessageRouter<IBlyncLightMessage>>(),
                    container.Resolve<BlynclightController>()));

            container.RegisterMultiple<IClientLifecycleCallbackHandler>(new List<Type>()
            {
                typeof (SkypeManager),
                typeof (BlyncLightManager)

            }).AsSingleton();

            container.Register(typeof(BlyncLightForSkypeClient),
                new BlyncLightForSkypeClient());
        }
    }
}