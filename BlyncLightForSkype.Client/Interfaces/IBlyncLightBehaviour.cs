using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Interfaces
{
    public interface IBlyncLightBehaviour
    {
        Priority Priority { get; }

        void InitBehaviour(BlyncLightManager manager);
        void EnableBehaviour();
        void DisableBehaviour();
    }
}
