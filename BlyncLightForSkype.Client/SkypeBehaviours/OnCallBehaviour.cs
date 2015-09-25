using BlyncLightForSkype.Client.Interfaces;
using BlyncLightForSkype.Client.Models;
using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.SkypeBehaviours
{
    /// <summary>
    /// Behaviour that changes status to Busy/DoNotDisturb when online for the duration of a call
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
            Priority = Priority.Normal;
        } 

        #endregion

        #region ISkypeBehaviour

        public Priority Priority { get; private set; }

        public void InitBehaviour(SkypeManager manager)
        {
            skypeManager = manager;

            if (skypeManager.Logger.IsDebugEnabled)
            {
                skypeManager.Logger.Debug("Initialised OnCallBehaviour");
            }
        }

        public void EnableBehaviour()
        {
            skypeManager.Skype.CallStatus += Skype_CallStatus;
        }

        public void DisableBehaviour()
        {
            skypeManager.Skype.CallStatus -= Skype_CallStatus;
        }
        
        #endregion

        #region Skype Callbacks

        private void Skype_CallStatus(Call pCall, TCallStatus Status)
        {
            SetOnCall(Status == TCallStatus.clsInProgress);
        }

        private void Skype_UserStatus(TUserStatus Status)
        {
            priorUserStatus = Status;
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

                    //change to busy only if we're online
                    if (priorUserStatus == TUserStatus.cusOnline)
                    {
                        skypeManager.Skype.ChangeUserStatus(TUserStatus.cusDoNotDisturb);
                    }
                    
                    //monitor explicit status changes
                    skypeManager.Skype.UserStatus += Skype_UserStatus;
                }
                else
                {
                    skypeManager.Skype.UserStatus -= Skype_UserStatus;

                    //update status post call if prior status is different
                    if (priorUserStatus != skypeManager.Skype.CurrentUserStatus)
                    {
                        skypeManager.Skype.ChangeUserStatus(priorUserStatus);
                    }
                }
            }
        }  

        #endregion
    }
}
