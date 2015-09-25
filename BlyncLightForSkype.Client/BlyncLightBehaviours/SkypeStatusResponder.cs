using BlyncLightForSkype.Client.Extensions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Messages;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.BlyncLightBehaviours
{
    public class SkypeStatusResponder : IBlyncLightBehaviour
    {
        #region Dependencies

        public IMessageRouter<IMessage> MessageRouter { get; set; } 

        #endregion

        #region Props

        /// <summary>
        /// Reference to BlyncLightManager object
        /// </summary>
        private BlyncLightManager blyncLighteManager; 

        /// <summary>
        /// Skype Call Status
        /// </summary>
        private CallStatus CallStatus = CallStatus.None;

        /// <summary>
        /// Skype User Status
        /// </summary>
        private UserStatus UserStatus = UserStatus.Connecting;

        #endregion

        #region Ctor
        public SkypeStatusResponder(IMessageRouter<IMessage> messageRouter)
        {
            MessageRouter = messageRouter;
            Priority = Priority.Normal;
        } 
        #endregion

        #region IBlyncLightBehaviour

        public Priority Priority { get; private set; }

        public void InitBehaviour(BlyncLightManager manager)
        {
            blyncLighteManager = manager;

            if (blyncLighteManager.Logger.IsDebugEnabled)
            {
                blyncLighteManager.Logger.Debug("Initialised SkypeStatusResponder");
            }

            MessageRouter.Subscribe(OnSkypeCallStatusMessage, x => x is SkypeCallStatusMessage);
            MessageRouter.Subscribe(OnSkypeUserStatusMessage, x => x is SkypeUserStatusMessage);

            MessageRouter.Subscribe(OnDeviceStatusChange, x => x is BlyncLightDeviceStatusMessage);
        }

        public void EnableBehaviour()
        {
            UpdateBlyncLight();
        }

        public void DisableBehaviour()
        {
            blyncLighteManager.BlynclightController.ResetAll();
        }
       
        #endregion

        #region Message Callbacks

        protected virtual void OnSkypeCallStatusMessage(IMessage message)
        {
            var callStatusMessage = message as SkypeCallStatusMessage;
            if (callStatusMessage != null)
            {
                blyncLighteManager.Logger.Info("Call Status " + callStatusMessage.Status);

                if (callStatusMessage.Status == CallStatus.Missed)
                {
                    //TODO handle missed calls
                }
                else
                {
                    CallStatus = callStatusMessage.Status;
                    UpdateBlyncLight();
                }
            }
        }

        protected virtual void OnSkypeUserStatusMessage(IMessage message)
        {
            var userStatusMessage = message as SkypeUserStatusMessage;
            if (userStatusMessage != null)
            {
                blyncLighteManager.Logger.Info("User Status " + userStatusMessage.Status);

                UserStatus = userStatusMessage.Status;

                UpdateBlyncLight();
            }
        }

        public void OnDeviceStatusChange(IMessage message)
        {
            var deviceStatusMessage = message as BlyncLightDeviceStatusMessage;
            if (deviceStatusMessage != null)
            {
                blyncLighteManager.Logger.Info("BlyncLight " + deviceStatusMessage.Status);

                UpdateBlyncLight();
            }
        }

        #endregion

        #region Methods

        private void UpdateBlyncLight()
        {
            if (blyncLighteManager.DeviceStatus != DeviceStatus.Connected)
            {
                blyncLighteManager.BlynclightController.ResetAll();

                return;
            }

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
                    blyncLighteManager.BlynclightController.SetStatusOnline();
                    break;
                case UserStatus.Connecting:
                case UserStatus.Offline:
                    blyncLighteManager.BlynclightController.SetStatusOffline();
                    break;
                case UserStatus.Away:
                    blyncLighteManager.BlynclightController.SetStatusAway();
                    break;
                case UserStatus.Busy:
                    blyncLighteManager.BlynclightController.SetStatusBusy();
                    break;
                case UserStatus.None:
                    blyncLighteManager.BlynclightController.ResetAll();
                    break;
            }
        }

        private void SetLightBasedOnCallStatus()
        {
            switch (CallStatus)
            {
                case CallStatus.InProgress:
                    blyncLighteManager.BlynclightController.SetStatusCallInProgress();
                    break;
                case CallStatus.Ringing:
                    blyncLighteManager.BlynclightController.SetStatusRinging();
                    break;
            }
        }

        #endregion
    }
}
