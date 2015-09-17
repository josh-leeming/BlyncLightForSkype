using System;
using Blynclight;
using BlyncLightForSkype.Client.Extensions;
using BlyncLightForSkype.Client.Interfaces;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client
{
    /// <summary>
    /// Basic client that responds to Skype events
    /// </summary>
    public class BlyncLightForSkypeClient
    {
        #region Dependencies

        public ILogHandler Logger { get; set; }

        #endregion

        #region Props

        /// <summary>
        /// Triggered by typing the message (pi) into chat
        /// </summary>
        public bool IsOnLunch { get; protected set; }

        /// <summary>
        /// Triggered by Skype Call Status change
        /// </summary>
        public CallStatus CallStatus { get; protected set; }

        /// <summary>
        /// Handler to Blynclight 
        /// </summary>
        protected BlynclightController BlynclightController { get; private set; }

        /// <summary>
        /// Skype4COM
        /// </summary>
        protected Skype Skype { get; private set; }

        /// <summary>
        /// True if the client has been initialised
        /// </summary>
        private bool isInitialised;

        #endregion

        #region Ctor

        public BlyncLightForSkypeClient(ILogHandler logger)
        {
            Logger = logger;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Start responding to Skype events
        /// </summary>
        public virtual void StartClient()
        {
            if (isInitialised == false)
            {
                InitClient();
            }

            OnStartup();

            Skype.UserStatus += Skype_UserStatus;

            Skype.CallStatus += Skype_CallStatus;

            Skype.MessageStatus += Skype_MessageStatus;

            SetLightBasedOnSkypeStatus(Skype.CurrentUserStatus);
        }

        /// <summary>
        /// Reset Blynclight
        /// </summary>
        public virtual void StopClient()
        {
            OnShutdown();

            BlynclightController.ResetAll();
        } 
        #endregion

        #region Lifecycle

        protected virtual void OnInitialise()
        {
            Logger.Info("Initialising");
        }

        protected virtual void OnStartup()
        {
            Logger.Info("Starting");
        }

        protected virtual void OnShutdown()
        {
            Logger.Info("Shutting down");
        }

        protected virtual void OnLunch()
        {
            Logger.Info("Lunch " + IsOnLunch);

            Skype.ChangeUserStatus(IsOnLunch ? TUserStatus.cusAway : TUserStatus.cusOnline);
        }

        protected virtual void OnCall()
        {
            Logger.Info("Call " + CallStatus);

            if (CallStatus == CallStatus.None)
            {
                SetLightBasedOnSkypeStatus(Skype.CurrentUserStatus);
            }

            switch (CallStatus)
            {
                case CallStatus.InProgress:
                    BlynclightController.SetStatusCallInProgress();
                    break;
                case CallStatus.Ringing:
                    BlynclightController.SetStatusRinging();
                    break;
                case CallStatus.Missed:
                    BlynclightController.SetStatusCallMissed();
                    break;
                case CallStatus.None:
                    SetLightBasedOnSkypeStatus(Skype.CurrentUserStatus);
                    break;
            }
        }

        #endregion

        #region Skype Callbacks

        private void Skype_MessageStatus(ChatMessage pMessage, TChatMessageStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_MessageStatus " + Status);
            }

            if (Status == TChatMessageStatus.cmsSending)
            {
                if (pMessage.Body.Equals("(pi)"))
                {
                    SetOnLunch(true);
                }
            }
        }

        private void Skype_UserStatus(TUserStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_UserStatus " + Status);
            }

            if (CallStatus == CallStatus.None)
            {
                SetLightBasedOnSkypeStatus(Status);
            }
        }

        private void Skype_CallStatus(Call pCall, TCallStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_CallStatus " + Status);
            }

            CallStatus status;

            switch (Status)
            {
                case TCallStatus.clsInProgress:
                    status = CallStatus.InProgress;
                    break;
                case TCallStatus.clsRinging:
                    status = CallStatus.Ringing;
                    break;
                case TCallStatus.clsMissed:
                    status = CallStatus.Missed;
                    break;
                default:
                    status = CallStatus.None;
                    break;
            }

            SetOnCall(status);
        } 

        #endregion

        #region Bootstrap
        protected void InitClient()
        {
            OnInitialise();

            Skype = new Skype();
            // Use skype protocol version 7 
            Skype.Attach(7, false);

            BlynclightController = new BlynclightController();
            var deviceCount = BlynclightController.InitBlyncDevices();
            if (deviceCount == 0)
            {
                throw new Exception("No BlyncLight Devices detected");
            }

            isInitialised = true;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Pretty much what it says on the tin
        /// </summary>
        /// <param name="Status"></param>
        private void SetLightBasedOnSkypeStatus(TUserStatus Status)
        {
            switch (Status)
            {
                case TUserStatus.cusAway:
                    BlynclightController.SetStatusAway();
                    break;
                case TUserStatus.cusOnline:
                    BlynclightController.SetStatusOnline();
                    break;
                case TUserStatus.cusDoNotDisturb:
                    BlynclightController.SetStatusDoNotDisturb();
                    break;
                case TUserStatus.cusOffline:
                    BlynclightController.SetStatusOffline();
                    break;
                default:
                    BlynclightController.ResetAll();
                    break;
            }
        }

        private void SetOnLunch(bool onLunch)
        {
            if (onLunch != IsOnLunch)
            {
                IsOnLunch = onLunch;
                OnLunch();
            }
        }

        private void SetOnCall(CallStatus status)
        {
            if (status != CallStatus)
            {
                CallStatus = status;
                OnCall();
            }
        } 
        #endregion
    }
}
