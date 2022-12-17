using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLogger
{
    public class Config
    {
        public bool CustomNameplates { get; set; } = true;
        public bool Stealth { get; set; } = false;
        public bool LogAvatars { get; set; } = true;
        public bool LogWorlds { get; set; } = true;
        public bool LogFriendsAvatars { get; set; } = false;
        public bool LogOwnAvatars { get; set; } = false;
        public bool LogPublicAvatars { get; set; } = true;
        public bool LogPrivateAvatars { get; set; } = true;
        public bool LogToConsole { get; set; } = true;
        public bool ConsoleError { get; set; } = true;
        public bool HWIDSpoof { get; set; } = false;
        public bool AutoUpdate { get; set; } = true;
    }
}
