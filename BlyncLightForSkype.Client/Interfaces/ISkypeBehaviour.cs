﻿using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Interfaces
{
    public interface ISkypeBehaviour
    {
        Priority Priority { get; }

        void InitBehaviour(SkypeManager manager);
        void EnableBehaviour();
        void DisableBehaviour();
    }
}
