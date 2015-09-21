using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Messages
{
    public class SkypeUserStatusMessage : ISkypeMessage
    {
        public UserStatus Status { get; set; }
    }
}