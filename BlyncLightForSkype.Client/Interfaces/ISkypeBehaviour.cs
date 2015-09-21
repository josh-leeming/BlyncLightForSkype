using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.Interfaces
{
    public interface ISkypeBehaviour
    {
        SkypeBehaviourPriority Priority { get; }

        void InitBehaviour(SkypeManager manager);

        void Start();

        void Stop();
    }
}
