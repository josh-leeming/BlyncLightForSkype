using BlyncLightForSkype.Client;
using BlyncLightForSkype.Client.Interfaces;
using TinyIoC;

namespace BlyncLightForSkype.Console
{
    public static class IoC
    {
        public static void ConfigureContainer(TinyIoCContainer container)
        {
            container.Register(typeof(ILogHandler), typeof(Logger.Log4NetLogger)).AsSingleton();

            container.Register(typeof(BlyncLightForSkypeClient),
                new BlyncLightForSkypeClient(container.Resolve<ILogHandler>()));
        }
    }
}