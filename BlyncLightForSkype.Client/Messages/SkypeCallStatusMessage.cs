﻿using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Messages
{
    public class SkypeCallStatusMessage : IMessage
    {
        public CallStatus Status { get; set; }
    }
}
