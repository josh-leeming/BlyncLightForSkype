using System.Management;
using Blynclight;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Messages;

namespace BlyncLightForSkype.Client
{
    /// <summary>
    /// Responsible for listening to USB connection events and initialising BlyncLight devices
    /// </summary>
    public class BlyncLightManager : IClientLifecycleCallbackHandler
    {
        #region Dependencies

        public ILogHandler Logger { get; set; }

        public IMessageRouter<IBlyncLightMessage> MessageRouter { get; set; }

        public BlynclightController BlynclightController { get; set; }

        #endregion

        #region Props

        /// <summary>
        /// Watches for Win32_DeviceChangeEvents
        /// </summary>
        private ManagementEventWatcher UsbDeviceChangeWatcher { get; set; } 

        #endregion

        #region Ctor

        public BlyncLightManager(ILogHandler logger, IMessageRouter<IBlyncLightMessage> messageRouter, BlynclightController controller)
        {
            Logger = logger;
            MessageRouter = messageRouter;
            BlynclightController = controller;
        } 

        #endregion

        #region USB Device Callbacks

        void DeviceChangeEvent(object sender, EventArrivedEventArgs e)
        {
            InitBlyncDevices();
        }

        #endregion

        #region Lifecycle Callbacks

        public void OnInitialise()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Device Manager initialising");
            }

            UsbDeviceChangeWatcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3 GROUP WITHIN 1");
            UsbDeviceChangeWatcher.EventArrived += DeviceChangeEvent;
            UsbDeviceChangeWatcher.Query = query;
        }

        public void OnStartup()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Device Manager starting");
            }

            InitBlyncDevices();

            UsbDeviceChangeWatcher.Start();
        }

        public void OnShutdown()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Device Manager shutting down");
            }

            UsbDeviceChangeWatcher.Stop();
        } 

        #endregion

        #region Methods

        private void InitBlyncDevices()
        {
            var deviceCount = BlynclightController.InitBlyncDevices();
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(string.Format("{0} Blynclight {1} connected", deviceCount, deviceCount == 1 ? "device" : "devices"));
            }

            MessageRouter.Publish(new BlyncLightMessage()
            {
                DeviceConnected = deviceCount > 0
            });
        } 

        #endregion
    }
}
