using System;
using System.Globalization;

namespace snake2d.util.misc
{
    public enum OS
    {
        MAC, WINDOWS, LINUX, UNSUPPORTED
    }

    public static class OSUtils
    {
        public static OS Get()
        {
            string OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription.ToLower(CultureInfo.InvariantCulture);

            if (OS.Contains("mac") || OS.Contains("darwin"))
            {
                return OS.MAC;
            }
            if (OS.Contains("nix") || OS.Contains("nux") || OS.Contains("aix"))
            {
                return OS.LINUX;
            }
            if (OS.Contains("win"))
            {
                return OS.WINDOWS;
            }
            return OS.UNSUPPORTED;
        }
    }
}