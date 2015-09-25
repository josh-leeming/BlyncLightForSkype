using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Messages
{
    public class BlyncLightDeviceStatusMessage : IMessage
    {
        public DeviceStatus Status { get; set; }
    }
}
