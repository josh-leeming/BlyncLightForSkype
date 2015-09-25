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
    /// Responsible for listening to USB connection events and initialising BlyncLight devices
    /// </summary>
    public class BlyncLightManager : IClientLifecycleCallbackHandler
    {
        #region Dependencies

        public ILogHandler Logger { get; set; }

        public IMessageRouter<IMessage> MessageRouter { get; set; }

        public List<IBlyncLightBehaviour> AttachedBehaviours { get; set; }

        #endregion

        #region Props

        /// <summary>
        /// BlyncLight SDK Controller
        /// </summary>
        public readonly BlynclightController BlynclightController = new BlynclightController();

        /// <summary>
        /// Current DeviceStatus
        /// </summary>
        public DeviceStatus DeviceStatus { get; private set; }

        /// <summary>
        /// Watches for Win32_DeviceChangeEvents
        /// </summary>
        private ManagementEventWatcher UsbDeviceChangeWatcher { get; set; }

        /// <summary>
        /// Count of attached devices
        /// </summary>
        private int deviceCount;

        #endregion

        #region Ctor

        public BlyncLightManager(ILogHandler logger, IMessageRouter<IMessage> messageRouter, List<IBlyncLightBehaviour> blyncLightBehaviours)
        {
            Logger = logger;
            MessageRouter = messageRouter;
            AttachedBehaviours = blyncLightBehaviours;
            Priority = Priority.High;
            DeviceStatus = DeviceStatus.Disconnected;
        } 

        #endregion

        #region USB Device Callbacks

        void DeviceChangeEvent(object sender, EventArrivedEventArgs e)
        {
            InitBlyncDevices();
        }

        #endregion

        #region Lifecycle Callbacks

        public Priority Priority { get; private set; }

        public void OnInitialise()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Device Manager Initialising");
            }

            UsbDeviceChangeWatcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3 GROUP WITHIN 1");
            UsbDeviceChangeWatcher.EventArrived += DeviceChangeEvent;
            UsbDeviceChangeWatcher.Query = query;

            AttachedBehaviours.ForEach(behaviour => behaviour.InitBehaviour(this));
        }

        public void OnStartup()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Device Manager Starting");
            }

            InitBlyncDevices();

            UsbDeviceChangeWatcher.Start();

            AttachedBehaviours.ForEach(behaviour => behaviour.EnableBehaviour());
        }

        public void OnShutdown()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Device Manager Stopping");
            }

            UsbDeviceChangeWatcher.Stop();

            AttachedBehaviours.ForEach(behaviour => behaviour.DisableBehaviour());
        } 

        #endregion

        #region Methods

        public void EnableBlyncDevices()
        {
            SetDeviceStatus(deviceCount > 0 ? DeviceStatus.Connected : DeviceStatus.Disconnected);
        }

        public void DisableBlyncDevices()
        {
            SetDeviceStatus(DeviceStatus.Disabled);
        }

        private void InitBlyncDevices()
        {
            deviceCount = BlynclightController.InitBlyncDevices();
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(string.Format("{0} Blynclight {1} connected", deviceCount, deviceCount == 1 ? "device" : "devices"));
            }

            if (DeviceStatus == DeviceStatus.Disabled)
            {
                Logger.Info("BlyncLight Disabled");
            }
            else 
            {
                SetDeviceStatus(deviceCount > 0 ? DeviceStatus.Connected : DeviceStatus.Disconnected);
            }
        }

        private void SetDeviceStatus(DeviceStatus status)
        {
            if (DeviceStatus != status)
            {
                DeviceStatus = status;

                if (DeviceStatus != DeviceStatus.Connected)
                {
                    BlynclightController.ResetAll();
                }

                MessageRouter.Publish(new BlyncLightDeviceStatusMessage()
                {
                    Status = status
                });
            }
        }

        #endregion
    }
}
