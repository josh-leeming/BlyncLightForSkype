using BlyncLightForSkype.Client.Interfaces;

namespace BlyncLightForSkype.Client.Messages
{
    public class BlyncLightMessage : IBlyncLightMessage
    {
        public bool DeviceConnected { get; set; }
    }
}
