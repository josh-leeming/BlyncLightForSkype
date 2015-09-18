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
        /// Regular expression object for checking messages and changing status to away
        /// </summary>
        public Regex MsgAwayRegex { get; protected set; }

        /// <summary>
        /// Triggered by Skype Call Status change
        /// </summary>
        public CallStatus CallStatus { get; protected set; }

        /// <summary>
        /// Triggered by Skype User Status change
        /// </summary>
        public UserStatus UserStatus { get; protected set; }

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
            if (isInitialised == false)
            {
                InitClient();
            }

            OnStartup();

            UsbDeviceChangeWatcher.Start();

            Skype.UserStatus += Skype_UserStatus;

            Skype.CallStatus += Skype_CallStatus;

            Skype.MessageStatus += Skype_MessageStatus;
        }

        /// <summary>
        /// Reset Blynclight
        /// </summary>
        public virtual void StopClient()
        {
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

            Skype = new Skype();
            // Use skype protocol version 7 
            Skype.Attach(7, false);

            var status = Skype.CurrentUserStatus.ToUserStatus();
            SetUserStatus(status);

            InitBlyncDevices();

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
