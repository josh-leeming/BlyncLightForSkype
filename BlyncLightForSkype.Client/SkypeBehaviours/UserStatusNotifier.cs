using BlyncLightForSkype.Client.Extensions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.SkypeBehaviours
{
    public class UserStatusNotifier : ISkypeBehaviour
    {
        #region Props
        /// <summary>
        /// Reference to SkypeManager object
        /// </summary>
        private SkypeManager skypeManager; 
        #endregion

        #region Ctor
        public UserStatusNotifier()
        {
            Priority = SkypeBehaviourPriority.Normal;
        } 
        #endregion

        #region ISkypeBehaviour
        public SkypeBehaviourPriority Priority { get; private set; }

        public void InitBehaviour(SkypeManager manager)
        {
            skypeManager = manager;

            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("UserStatusNotifier initialised");
            }
        }

        public void Start()
        {
            skypeManager.Skype.UserStatus += Skype_UserStatus;
        }

        public void Stop()
        {
            skypeManager.Skype.UserStatus -= Skype_UserStatus;
        } 
        #endregion

        #region Skype Callbacks
        private void Skype_UserStatus(TUserStatus Status)
        {
            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("Skype_UserStatus " + Status);
            }

            var userStatus = Status.ToUserStatus();

            skypeManager.PublishUserStatus(userStatus);
        } 
        #endregion
    }
}
