using System;
using System.Collections.Generic;
using System.Linq;
using Blynclight;
using BlyncLightForSkype.Client.Interfaces;

namespace BlyncLightForSkype.Client
{
    /// <summary>
    /// Basic client that responds to Skype events and updates a BlyncLight device
    /// </summary>
    public class BlyncLightForSkypeClient
    {
        #region Dependencies

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

            Logger.Info("Client stopped");
        } 

        #endregion

        #region Lifecycle

        protected void OnInitialise()
        {
            ApplicationLifecycleCallbacks.OrderByDescending(s => s.Priority).ToList().ForEach(handler => handler.OnInitialise());
        }

        protected void OnStartup()
        {
            ApplicationLifecycleCallbacks.OrderByDescending(s => s.Priority).ToList().ForEach(handler => handler.OnStartup());
        }

        protected void OnShutdown()
        {
            ApplicationLifecycleCallbacks.OrderByDescending(s => s.Priority).ToList().ForEach(handler => handler.OnShutdown());
        }

        #endregion

        #region Init

        protected void InitClient()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Client initialising");
            }

            OnInitialise();

            isInitialised = true;

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Client initialised");
            }
        }

        #endregion
    }
}
