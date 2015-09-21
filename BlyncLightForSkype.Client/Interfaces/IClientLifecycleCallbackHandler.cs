namespace BlyncLightForSkype.Client.Interfaces
{
    public interface IClientLifecycleCallbackHandler
    {
        void OnInitialise();
        void OnStartup();
        void OnShutdown();
    }
}
