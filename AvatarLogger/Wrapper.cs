using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace AvatarLogger
{
    internal static class Wrapper
    {
        public static VRCPlayerApi GetVrcPlayerApi(this Player instance)
        {
            return instance?.prop_VRCPlayerApi_0;
        }

        public static ApiAvatar GetAvatarInfo(this Player instance)
        {
            return instance?.prop_ApiAvatar_0;
        }

        public static string GetAvatarStatus(this Player player)
        {
            var status = player.GetAvatarInfo().releaseStatus.ToLower();
            if (status == "public")
                return "<color=green>" + status + "</color>";
            return "<color=red>" + status + "</color>";
        }

        public static string GetPlatform(this Player player)
        {
            if (player != null)
            {
                if (player.field_Private_APIUser_0.IsOnMobile)
                    return "<color=green>Q</color>";
                if (player.GetVrcPlayerApi().IsUserInVR())
                    return "<color=#CE00D5>V</color>";
                return "<color=grey>PC</color>";
            } return "";
        }
    }
}