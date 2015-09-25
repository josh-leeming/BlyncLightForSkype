using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Interfaces
{
    public interface IClientLifecycleCallbackHandler
    {
        Priority Priority { get; }

        void OnInitialise();
        void OnStartup();
        void OnShutdown();
    }
}
