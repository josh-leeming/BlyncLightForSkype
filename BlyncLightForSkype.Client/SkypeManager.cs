using System.Collections.Generic;
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

        public List<ISkypeBehaviour> AttachedBehaviours { get; set; }

        #endregion

        #region Props

        /// <summary>
        /// Skype4Com
        /// </summary>
        public readonly Skype Skype = new Skype();

        #endregion

        #region Ctor

        public SkypeManager(ILogHandler logger, IMessageRouter<ISkypeMessage> messageRouter, List<ISkypeBehaviour> skypeBehaviours)
        {
            Logger = logger;
            MessageRouter = messageRouter;
            AttachedBehaviours = skypeBehaviours;
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

            AttachedBehaviours.ForEach(behaviour => behaviour.InitBehaviour(this));
        }

        public void OnStartup()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype Manager starting");
            }

            AttachedBehaviours.ForEach(behaviour => behaviour.Start());
        }

        public void OnShutdown()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype Manager shutting down");
            }

            AttachedBehaviours.ForEach(behaviour => behaviour.Stop());
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
                Logger.Info("Skype Attached");

                var userStatus = Skype.CurrentUserStatus.ToUserStatus();

                PublishUserStatus(userStatus);

                var callStatus = Skype.ActiveCalls.Count > 0 ? CallStatus.InProgress : CallStatus.None;

                PublishCallStatus(callStatus);
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

        #endregion

        #region Methods

        public void PublishCallStatus(CallStatus status)
        {
            MessageRouter.Publish(new SkypeCallStatusMessage
            {
                Status = status
            });
        }

        public void PublishUserStatus(UserStatus status)
        {
            MessageRouter.Publish(new SkypeUserStatusMessage
            {
                Status = status
            });
        }

        #endregion
    }
}
