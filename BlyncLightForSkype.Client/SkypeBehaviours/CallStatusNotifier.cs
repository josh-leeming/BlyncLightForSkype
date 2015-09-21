using BlyncLightForSkype.Client.Extensions;
using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.SkypeBehaviours
{
    public class CallStatusNotifier : ISkypeBehaviour
    {
        #region Props
        /// <summary>
        /// Reference to SkypeManager object
        /// </summary>
        private SkypeManager skypeManager; 
        #endregion

        #region Ctor
        public CallStatusNotifier()
        {
            Priority = SkypeBehaviourPriority.Normal;
        } 
        #endregion

        #region ISkypeBehaviour
        public SkypeBehaviourPriority Priority { get; private set; }

        public void InitBehaviour(SkypeManager manager)
        {
            skypeManager = manager;

            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("CallStatusNotifier initialised");
            }
        }

        public void Start()
        {
            skypeManager.Skype.CallStatus += Skype_CallStatus;
        }

        public void Stop()
        {
            skypeManager.Skype.CallStatus -= Skype_CallStatus;
        }
        
        #endregion

        #region Skype Callbacks
        private void Skype_CallStatus(Call pCall, TCallStatus Status)
        {
            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("Skype_CallStatus " + Status);
            }

            var callStatus = Status.ToCallStatus();

            skypeManager.PublishCallStatus(callStatus);
        } 
        #endregion
    }
}
