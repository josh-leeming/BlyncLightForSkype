using System;
using System.Text.RegularExpressions;
using System.Management;
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
        /// True if the client is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Regular expression object for checking messages and changing status to away
        /// </summary>
        protected Regex MsgAwayRegex { get; private set; }

        /// <summary>
        /// Triggered by Skype Call Status change
        /// </summary>
        protected CallStatus CallStatus { get; private set; }

        /// <summary>
        /// Triggered by Skype User Status change
        /// </summary>
        protected UserStatus UserStatus { get; private set; }

        /// <summary>
        /// Handler to Blynclight 
        /// </summary>
        protected BlynclightController BlynclightController { get; private set; }

        /// <summary>
        /// Skype4COM
        /// </summary>
        protected Skype Skype { get; private set; }

        /// <summary>
        /// Watches for Win32_DeviceChangeEvents
        /// </summary>
        protected ManagementEventWatcher UsbDeviceChangeWatcher { get; private set; }

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
            if (IsRunning)
            {
                throw new InvalidOperationException("Client is currently running");    
            }

            IsRunning = true;

            if (isInitialised == false)
            {
                InitClient();
            }

            OnStartup();

            Skype.UserStatus += Skype_UserStatus;
            Skype.CallStatus += Skype_CallStatus;
            Skype.MessageStatus += Skype_MessageStatus;

            UsbDeviceChangeWatcher.Start();

            InitBlyncDevices();
        }

        /// <summary>
        /// Reset Blynclight
        /// </summary>
        public virtual void StopClient()
        {
            if (IsRunning == false)
            {
                throw new InvalidOperationException("Client is not running");
            }

            IsRunning = false;

            OnShutdown();

            UsbDeviceChangeWatcher.Stop();

            Skype.UserStatus -= Skype_UserStatus;
            Skype.CallStatus -= Skype_CallStatus;
            Skype.MessageStatus -= Skype_MessageStatus;

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

        protected virtual void OnCall()
        {
            Logger.Info("Call " + CallStatus);

            if (CallStatus == CallStatus.None)
            {
                SetLightBasedOnStatus();
            }
            else
            {
                SetLightBasedOnCall();
            }
        }

        protected virtual void OnStatus()
        {
            Logger.Info("Status " + UserStatus);

            if (CallStatus == CallStatus.None)
            {
                SetLightBasedOnStatus();
            }
            else
            {
                SetLightBasedOnCall();
            }
        }

        protected virtual void OnBlyncLightConnected()
        {
            Logger.Info("BlyncLight Connected");

            if (CallStatus == CallStatus.None)
            {
                SetLightBasedOnStatus();
            }
            else
            {
                SetLightBasedOnCall();
            }
        }

        protected virtual void OnSkypeAttached()
        {
            Logger.Info("Skype Attached");

            var status = Skype.CurrentUserStatus.ToUserStatus();
            SetUserStatus(status);

            if (Skype.ActiveCalls.Count > 0)
            {
                SetCallStatus(CallStatus.InProgress);
            }
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

                ((_ISkypeEvents_Event)Skype).AttachmentStatus -= Skype_AttachmentStatus;
            }
        }

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
                    SetUserStatus(UserStatus.Away);
                }
            }
        }

        private void Skype_UserStatus(TUserStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_UserStatus " + Status);
            }

            var status = Status.ToUserStatus();

            SetUserStatus(status);
        }

        private void Skype_CallStatus(Call pCall, TCallStatus Status)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Skype_CallStatus " + Status);
            }

            var status = Status.ToCallStatus();

            SetCallStatus(status);
        } 

        #endregion

        #region USB Device Callbacks
        void DeviceChangeEvent(object sender, EventArrivedEventArgs e)
        {
            InitBlyncDevices();
        } 

        #endregion

        #region Bootstrap
        protected void InitClient()
        {
            OnInitialise();

            BlynclightController = new BlynclightController();

            UserStatus = UserStatus.Connecting;

            InitBlyncDevices();

            Skype = new Skype();

            //Attach async
            ((_ISkypeEvents_Event)Skype).AttachmentStatus += Skype_AttachmentStatus;
            Skype.Attach(8, false); 

            UsbDeviceChangeWatcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3 GROUP WITHIN 1");
            UsbDeviceChangeWatcher.EventArrived += DeviceChangeEvent;
            UsbDeviceChangeWatcher.Query = query;

            MsgAwayRegex = new Regex(@"^(\(pi\)|\(brb\)|\(coffee\))$", RegexOptions.IgnoreCase);

            isInitialised = true;
        }
        #endregion

        #region Private Methods

        private void SetCallStatus(CallStatus status)
        {
            if (status != CallStatus)
            {
                CallStatus = status;
                OnCall();
            }
        }

        private void SetUserStatus(UserStatus status)
        {
            if (status != UserStatus)
            {
                UserStatus = status;
                OnStatus();
            }
        }

        private void SetLightBasedOnStatus()
        {
            switch (UserStatus)
            {
                case UserStatus.Online:
                    BlynclightController.SetStatusOnline();
                    break;
                case UserStatus.Connecting:
                case UserStatus.Offline:
                    BlynclightController.SetStatusOffline();
                    break;
                case UserStatus.Away:
                    BlynclightController.SetStatusAway();
                    break;
                case UserStatus.Busy:
                    BlynclightController.SetStatusBusy();
                    break;
                case UserStatus.None:
                    BlynclightController.ResetAll();
                    break;
            }
        }

        private void SetLightBasedOnCall()
        {
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
            }
        }

        private void InitBlyncDevices()
        {
            var deviceCount = BlynclightController.InitBlyncDevices();
            if (deviceCount == 0)
            {
                Logger.Warn("BlyncLight Disonnected");
            }
            else
            {
                OnBlyncLightConnected();
            }
        } 
        #endregion
    }
}
