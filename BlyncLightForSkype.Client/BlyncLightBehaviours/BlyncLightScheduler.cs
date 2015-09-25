using System;
using System.Threading;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.BlyncLightBehaviours
{
    /// <summary>
    /// Behaviour that only activates blynclight during office hours
    /// </summary>
    public class BlyncLightScheduler : IBlyncLightBehaviour
    {
        #region Props

        /// <summary>
        /// Reference to BlyncLightManager object
        /// </summary>
        private BlyncLightManager blyncLighteManager; 

        /// <summary>
        /// True if the light is active
        /// </summary>
        private bool blyncLightActive = false;

        private Timer startTimer;

        private Timer endTimer;

        #endregion

        #region Ctor

        public BlyncLightScheduler()
        {
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
                blyncLighteManager.Logger.Debug("Initialised BlyncLightScheduler");
            }

            DateTime startTime = DateTime.Today.AddHours(7).AddMinutes(00);
            if (DateTime.Now > startTime)
            {
                if (startTime.DayOfWeek != DayOfWeek.Saturday &&
                    startTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    blyncLightActive = true;
                }
            }

            DateTime endTime = DateTime.Today.AddHours(18).AddMinutes(00);
            if (DateTime.Now > endTime)
            {
                blyncLightActive = false;
            }

            UpdateBlyncLightState();
        }

        public void EnableBehaviour()
        {
            SetStartTimer();
            SetEndTimer();

            UpdateBlyncLightState();
        }

        public void DisableBehaviour()
        {
            startTimer.Change(Timeout.Infinite, Timeout.Infinite);
            endTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        
        #endregion

        private void SetStartTimer()
        {
            // trigger the event at 7 AM
            DateTime startTime = DateTime.Today.AddHours(7).AddMinutes(00);
            if (DateTime.Now > startTime)
            {
                if (startTime.DayOfWeek != DayOfWeek.Saturday &&
                    startTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    blyncLightActive = true;
                }

                startTime = startTime.AddDays(1);
            }

            startTimer = new Timer(StartTimerAction);
            startTimer.Change((int)(startTime - DateTime.Now).TotalMilliseconds, Timeout.Infinite);
        }

        private void SetEndTimer()
        {
            // trigger the event at 6 PM.
            DateTime endTime = DateTime.Today.AddHours(18).AddMinutes(00);
            if (DateTime.Now > endTime)
            {
                blyncLightActive = false;

                endTime = endTime.AddDays(1);
            }

            endTimer = new Timer(EndTimerAction);
            endTimer.Change((int)(endTime - DateTime.Now).TotalMilliseconds, Timeout.Infinite);
        }

        private void StartTimerAction(object e)
        {
            blyncLighteManager.Logger.Info("Turning on BlyncLights");

            blyncLightActive = true;

            UpdateBlyncLightState();

            SetStartTimer();
        }

        private void EndTimerAction(object e)
        {
            blyncLighteManager.Logger.Info("Turning off BlyncLights");

            blyncLightActive = false;

            UpdateBlyncLightState();

            SetEndTimer();
        }

        private void UpdateBlyncLightState()
        {
            if (blyncLightActive)
            {
                blyncLighteManager.EnableBlyncDevices();
            }
            else
            {
                blyncLighteManager.DisableBlyncDevices();
            }
        }
    }
}
