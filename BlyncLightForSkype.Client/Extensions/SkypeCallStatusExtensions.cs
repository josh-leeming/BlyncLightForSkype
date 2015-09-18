using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.Extensions
{
    public static class SkypeCallStatusExtensions
    {
        public static CallStatus ToCallStatus(this TCallStatus status)
        {
            CallStatus callStatus;
            switch (status)
            {
                case TCallStatus.clsInProgress:
                    callStatus = CallStatus.InProgress;
                    break;
                case TCallStatus.clsRinging:
                    callStatus = CallStatus.Ringing;
                    break;
                case TCallStatus.clsMissed:
                    callStatus = CallStatus.Missed;
                    break;
                default:
                    callStatus = CallStatus.None;
                    break;
            }
            return callStatus;
        }
    }
}
