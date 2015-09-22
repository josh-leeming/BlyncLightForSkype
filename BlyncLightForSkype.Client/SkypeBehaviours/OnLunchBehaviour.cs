using System.Text.RegularExpressions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.SkypeBehaviours
{
    /// <summary>
    /// Simple behaviour that sets status to away when (pi) is entered into an IM
    /// </summary>
    public class OnLunchBehaviour : ISkypeBehaviour
    {
        #region Props
        /// <summary>
        /// Reference to SkypeManager object
        /// </summary>
        private SkypeManager skypeManager;
        /// <summary>
        /// Regular expression object for checking messages and changing status to away
        /// </summary>
        private readonly Regex OnLunchRegex = new Regex(@"^\(pi\)$", RegexOptions.IgnoreCase);
        /// <summary>
        /// True if behaviour has detected OnLunchRegex
        /// </summary>
        private bool onLunch;
        #endregion

        #region Ctor

        public OnLunchBehaviour()
        {
            Priority = SkypeBehaviourPriority.Meh;
        } 

        #endregion

        #region ISkypeBehaviour

        public SkypeBehaviourPriority Priority { get; private set; }

        public void InitBehaviour(SkypeManager manager)
        {
            skypeManager = manager;

            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("Initialised OnLunchBehaviour");
            }
        }

        public void Start()
        {
            skypeManager.Skype.MessageStatus += Skype_MessageStatus;
        }

        public void Stop()
        {
            skypeManager.Skype.MessageStatus -= Skype_MessageStatus;
        } 

        #endregion

        #region Skype Callbacks
        private void Skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Status == TChatMessageStatus.cmsSending)
            {
                if (OnLunchRegex.IsMatch(pMessage.Body))
                {
                    OnLunch(true);
                }
            }
        }

        private void Skype_UserStatus(TUserStatus Status)
        {
            switch (Status)
            {
                case TUserStatus.cusOnline:
                case TUserStatus.cusDoNotDisturb:
                    OnLunch(false);
                    break;
            }
        } 
        #endregion

        #region Methods
        private void OnLunch(bool lunch)
        {
            onLunch = lunch;

            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("OnLunchBehaviour " + onLunch);
            }

            if (onLunch)
            {
                skypeManager.Skype.ChangeUserStatus(TUserStatus.cusAway);
                skypeManager.Skype.UserStatus += Skype_UserStatus;
            }
            else
            {
                skypeManager.Skype.UserStatus -= Skype_UserStatus;
            }
        } 
        #endregion
    } 
}
