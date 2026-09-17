using System;

namespace Game
{
    public static class VERSION
    {
        public const int VERSION_MAJOR = 71;
        public const int VERSION_MINOR = 44;
        public const int VERSION = Version(VERSION_MAJOR, VERSION_MINOR);
        public const string VERSION_STRING = VersionString(VERSION);

        private VERSION()
        {
            if (false)
            {
                //have the ant script build with a certain java
                //warning for cerfew
                //warning for captives incoming, but no stockade
            }
        }

        public static bool VersionIsBefore(int major, int minor)
        {
            return GAME.Version() < Version(major, minor);
        }

        public static bool VersionIs(int major, int minor)
        {
            return GAME.Version() == Version(major, minor);
        }

        public static int Version(int major, int minor)
        {
            return (major << 16) | minor;
        }

        public static string VersionString(int version)
        {
            int m = VersionMajor(version);
            int n = VersionMinor(version);

            return "0." + m + "." + n;
        }

        public static int VersionMajor(int version)
        {
            return (version >> 16) & 0x0FFFF;
        }

        public static int VersionMinor(int version)
        {
            return version & 0x0FFFF;
        }
    }
}