using SKYPE4COMLib;

namespace BlyncLightForSkype.Client.Extensions
{
    public static class SkypeUserStatusExtensions
    {
        public static UserStatus ToUserStatus(this TUserStatus status)
        {
            UserStatus userStatus;
            switch (status)
            {
                case TUserStatus.cusOnline:
                    userStatus = UserStatus.Online;
                    break;
                case TUserStatus.cusOffline:
                case TUserStatus.cusLoggedOut:
                    userStatus = UserStatus.Offline;
                    break;
                case TUserStatus.cusDoNotDisturb:
                    userStatus = UserStatus.Busy;
                    break;
                case TUserStatus.cusAway:
                    userStatus = UserStatus.Away;
                    break;
                default:
                    userStatus = UserStatus.None;
                    break;
            }
            return userStatus;
        }
    }
}
