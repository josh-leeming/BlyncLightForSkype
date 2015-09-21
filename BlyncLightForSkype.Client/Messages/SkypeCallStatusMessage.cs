using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Messages
{
    public class SkypeCallStatusMessage : ISkypeMessage
    {
        public CallStatus Status { get; set; }
    }
}
