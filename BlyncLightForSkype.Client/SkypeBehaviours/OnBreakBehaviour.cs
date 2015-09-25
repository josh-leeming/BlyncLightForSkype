using System.Text.RegularExpressions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.SkypeBehaviours
{
    /// <summary>
    /// Simple behaviour that sets status to away when (coffee) or (brb) is entered into an IM
    /// </summary>
    public class OnBreakBehaviour : ISkypeBehaviour
    {
        #region Props

        /// <summary>
        /// Reference to SkypeManager object
        /// </summary>
        private SkypeManager skypeManager;
        /// <summary>
        /// Regular expression object for checking messages and changing status to away
        /// </summary>
        private readonly Regex OnBreakRegex = new Regex(@"^\(?(brb|coffee)\)?$", RegexOptions.IgnoreCase);

        #endregion

        #region Ctor

        public OnBreakBehaviour()
        {
            Priority = Priority.Meh;
        } 

        #endregion

        #region ISkypeBehaviour

        public Priority Priority { get; private set; }

        public void InitBehaviour(SkypeManager manager)
        {
            skypeManager = manager;

            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("Initialised OnBreakBehaviour");
            }
        }

        public void EnableBehaviour()
        {
            skypeManager.Skype.MessageStatus += Skype_MessageStatus;
        }

        public void DisableBehaviour()
        {
            skypeManager.Skype.MessageStatus -= Skype_MessageStatus;
        } 

        #endregion

        #region Skype Callbacks

        private void Skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Status == TChatMessageStatus.cmsSending)
            {
                if (OnBreakRegex.IsMatch(pMessage.Body))
                {
                    if (skypeManager.Logger.IsDebugEnabled)
                    {
                        skypeManager.Logger.Debug("OnBreakBehaviour");
                    }

                    skypeManager.Skype.ChangeUserStatus(TUserStatus.cusAway);
                }
            }
        }

        #endregion
    } 
}
