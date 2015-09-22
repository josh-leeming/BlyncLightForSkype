using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.SkypeBehaviours
{
    /// <summary>
    /// Behaviour that changes status to Busy/DoNotDisturb while on a call
    /// </summary>
    public class OnCallBehaviour : ISkypeBehaviour
    {
        #region Props
        /// <summary>
        /// Reference to SkypeManager object
        /// </summary>
        private SkypeManager skypeManager;
        /// <summary>
        /// True if behaviour has detected OnLunchRegex
        /// </summary>
        private bool onCall;
        /// <summary>
        /// UserStatus prior to going on call
        /// </summary>
        private TUserStatus priorUserStatus;
        #endregion

        #region Ctor
        public OnCallBehaviour()
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
                skypeManager.Logger.Debug("Initialised OnCallBehaviour");
            }

            if (skypeManager.Skype.ActiveCalls.Count > 0)
            {
                SetOnCall(true);
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
            SetOnCall(Status == TCallStatus.clsInProgress);
        } 
        #endregion

        #region Methods
        private void SetOnCall(bool call)
        {
            if (onCall != call)
            {
                onCall = call;

                if (skypeManager.Logger.IsDebugEnabled)
                {
                    skypeManager.Logger.Debug("OnCallBehaviour " + onCall);
                }

                if (onCall)
                {
                    priorUserStatus = skypeManager.Skype.CurrentUserStatus;
                    skypeManager.Skype.ChangeUserStatus(TUserStatus.cusDoNotDisturb);
                }
                else
                {
                    skypeManager.Skype.ChangeUserStatus(priorUserStatus);
                }
            }
        }  
        #endregion
    }
}
