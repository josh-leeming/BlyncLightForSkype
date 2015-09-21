using System;
using System.Collections.Generic;
using System.Management;
using Blynclight;
using BlyncLightForSkype.Client.Extensions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Messages;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client
{
    /// <summary>
    /// Basic client that responds to Skype events and updates a BlyncLight device
    /// </summary>
    public class BlyncLightForSkypeClient
    {
        #region Dependencies

        public IMessageRouter<ISkypeMessage> SkypeMessageRouter { get; set; }

        public IMessageRouter<IBlyncLightMessage> BlyncLightMessageRouter { get; set; }

        public ILogHandler Logger { get; set; }
        
        public List<IClientLifecycleCallbackHandler> ApplicationLifecycleCallbacks { get; set; }

        public BlynclightController BlynclightController { get; set; }

        #endregion

        #region Props

        /// <summary>
        /// True if the client is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Skype Call Status
        /// </summary>
        private CallStatus CallStatus = CallStatus.None;

        /// <summary>
        /// Skype User Status
        /// </summary>
        private UserStatus UserStatus = UserStatus.Connecting;

        /// <summary>
        /// True if the client has been initialised
        /// </summary>
        private bool isInitialised;

        #endregion

        #region Public Methods

        /// <summary>
        /// Start responding to Skype events
        /// </summary>
        public void StartClient()
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

            UpdateBlyncLight();

            Logger.Info("Client started");
        }

        /// <summary>
        /// Reset Blynclight
        /// </summary>
        public void StopClient()
        {
            if (IsRunning == false)
            {
                throw new InvalidOperationException("Client is not running");
            }

            IsRunning = false;

            OnShutdown();

            BlynclightController.ResetAll();

            Logger.Info("Client stopped");
        } 

        #endregion

        #region Lifecycle

        protected void OnInitialise()
        {
            ApplicationLifecycleCallbacks.ForEach(handler => handler.OnInitialise());
        }

        protected void OnStartup()
        {
            ApplicationLifecycleCallbacks.ForEach(handler => handler.OnStartup());
        }

        protected void OnShutdown()
        {
            ApplicationLifecycleCallbacks.ForEach(handler => handler.OnShutdown());
        }

        #endregion

        #region Message Callbacks

        protected virtual void OnSkypeCallStatusMessage(ISkypeMessage message)
        {
            var callStatusMessage = message as SkypeCallStatusMessage;
            if (callStatusMessage != null)
            {
                Logger.Info("Call Status " + callStatusMessage.Status);

                CallStatus = callStatusMessage.Status;

                UpdateBlyncLight();
            }
        }

        protected virtual void OnSkypeUserStatusMessage(ISkypeMessage message)
        {
            var userStatusMessage = message as SkypeUserStatusMessage;
            if (userStatusMessage != null)
            {
                Logger.Info("User Status " + userStatusMessage.Status);

                UserStatus = userStatusMessage.Status;

                UpdateBlyncLight();
            }
        }

        protected virtual void OnBlyncLightMessage(IBlyncLightMessage message)
        {
            var blyncLightMessage = message as BlyncLightMessage;
            if (blyncLightMessage != null)
            {
                if (blyncLightMessage.DeviceConnected)
                {
                    Logger.Info("BlyncLight Connected");

                    UpdateBlyncLight();
                }
                else
                {
                    Logger.Warn("BlyncLight Disonnected");
                }
            }
        } 

        #endregion

        #region Bootstrap

        protected void InitClient()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Client initialising");
            }

            SkypeMessageRouter.Subscribe(OnSkypeCallStatusMessage, x => x is SkypeCallStatusMessage);
            SkypeMessageRouter.Subscribe(OnSkypeUserStatusMessage, x => x is SkypeUserStatusMessage);

            BlyncLightMessageRouter.Subscribe(OnBlyncLightMessage, x => true);

            OnInitialise();

            isInitialised = true;

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Client initialised");
            }
        }

        #endregion

        #region Methods

        private void UpdateBlyncLight()
        {
            if (CallStatus == CallStatus.None)
            {
                SetLightBasedOnUserStatus();
            }
            else
            {
                SetLightBasedOnCallStatus();
            }
        }

        private void SetLightBasedOnUserStatus()
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

        private void SetLightBasedOnCallStatus()
        {
            switch (CallStatus)
            {
                case CallStatus.InProgress:
                    BlynclightController.SetStatusCallInProgress();
                    break;
                case CallStatus.Ringing:
                    BlynclightController.SetStatusRinging();
                    break;
            }
        }

        #endregion
    }
}
