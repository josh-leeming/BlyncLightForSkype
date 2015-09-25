using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Messages
{
    public class SkypeUserStatusMessage : IMessage
    {
        public UserStatus Status { get; set; }
    }
}