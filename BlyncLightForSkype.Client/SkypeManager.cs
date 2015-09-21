using System.Text.RegularExpressions;
using BlyncLightForSkype.Client.Extensions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Messages;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client
{
    /// <summary>
    /// Custom listening and handling of Skype events
    /// </summary>
    public class SkypeManager : IClientLifecycleCallbackHandler
    {
        #region Dependencies

        public ILogHandler Logger { get; set; }

        public IMessageRouter<ISkypeMessage> MessageRouter { get; set; }

        #endregion

        #region Props

        /// <summary>
        /// Skype4Com
        /// </summary>
        private readonly Skype Skype = new Skype();

        /// <summary>
        /// Regular expression object for checking messages and changing status to away
        /// </summary>
        private readonly Regex MsgAwayRegex = new Regex(@"^(\()+(pi|brb|coffee)+(\))$", RegexOptions.IgnoreCase);

        #endregion

        #region Ctor

        public SkypeManager(ILogHandler logger, IMessageRouter<ISkypeMessage> messageRouter)
        {
            Logger = logger;
            MessageRouter = messageRouter;
        }

        #endregion

        #region Lifecycle Callbacks

        public void OnInitialise()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype Manager initialising");
            }

            ((_ISkypeEvents_Event)Skype).AttachmentStatus += Skype_AttachmentStatus;

            //Attach async
            Skype.Attach(8, false); 
        }

        public void OnStartup()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype Manager starting");
            }

            Skype.UserStatus += Skype_UserStatus;
            Skype.CallStatus += Skype_CallStatus;
            Skype.MessageStatus += Skype_MessageStatus;
        }

        public void OnShutdown()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype Manager shutting down");
            }

            Skype.UserStatus -= Skype_UserStatus;
            Skype.CallStatus -= Skype_CallStatus;
            Skype.MessageStatus -= Skype_MessageStatus;
        }

        #endregion

        #region Skype Callbacks

        private void Skype_AttachmentStatus(TAttachmentStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_AttachmentStatus " + Status);
            }

            if (Status == TAttachmentStatus.apiAttachSuccess)
            {
                OnSkypeAttached();
            }
            else if (Status == TAttachmentStatus.apiAttachAvailable)
            {
                Skype.Attach(8, false);
            }
            else if (Status == TAttachmentStatus.apiAttachNotAvailable)
            {
                PublishUserStatus(UserStatus.None);
            }
        }

        //TODO, inject Skype Message Handlers
        private void Skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_MessageStatus " + Status);
            }

            if (Status == TChatMessageStatus.cmsSending)
            {
                if (MsgAwayRegex.IsMatch(pMessage.Body))
                {
                    Skype.ChangeUserStatus(TUserStatus.cusAway);
                }
            }
        }

        //TODO, inject Skype User Status Handlers
        private void Skype_UserStatus(TUserStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_UserStatus " + Status);
            }

            var userStatus = Status.ToUserStatus();

            PublishUserStatus(userStatus);
        }

        //TODO, inject Skype Call Status Handlers
        private void Skype_CallStatus(Call pCall, TCallStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_CallStatus " + Status);
            }

            var callStatus = Status.ToCallStatus();

            PublishCallStatus(callStatus);
        }

        #endregion

        #region Methods

        private void OnSkypeAttached()
        {
            Logger.Info("Skype Attached");

            var userStatus = Skype.CurrentUserStatus.ToUserStatus();

            PublishUserStatus(userStatus);

            var callStatus = Skype.ActiveCalls.Count > 0 ? CallStatus.InProgress : CallStatus.None;

            PublishCallStatus(callStatus);
        }

        private void PublishCallStatus(CallStatus status)
        {
            MessageRouter.Publish(new SkypeCallStatusMessage
            {
                Status = status
            });
        }

        private void PublishUserStatus(UserStatus status)
        {
            MessageRouter.Publish(new SkypeUserStatusMessage
            {
                Status = status
            });
        }

        #endregion
    }
}
