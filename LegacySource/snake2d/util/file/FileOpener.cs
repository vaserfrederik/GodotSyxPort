using System;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace Snake2d.Util.File
{
    /**
     * 
     * @author jjYBdx4IL
     */
    internal static class FileOpener
    {

        public static void Open(FileInfo file)
        {

            if (!OpenSystem(file.FullName) && !OpenDesktop(file))
                Console.Error.WriteLine("unable to open file " + Environment.OSVersion.Platform.ToString());
        }

        private static bool OpenSystem(string what)
        {

            string os = Environment.OSVersion.Platform.ToString().ToLowerInvariant();

            if (os.Contains("win"))
            {
                return Run("explorer", "%s", what);
            }

            if (os.Contains("mac"))
            {
                return Run("open", "%s", what);
            }

            return Run("kde-open", "%s", what) || Run("gnome-open", "%s", what) || Run("xdg-open", "%s", what);

        }

        private static bool OpenDesktop(FileInfo file)
        {

            if (System.Windows.Forms.SystemInformation.PowerStatus.BatteryChargeStatus == System.Windows.Forms.PowerStatus.BatteryChargeStatus.NoSystemBattery)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.FullName) { UseShellExecute = true });
                    return true;
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
            return false;

        }

        private static bool Run(string command, string arg, string file)
        {

            string[] args = arg.Split(' ');
            string[] parts = new string[args.Length + 1];
            parts[0] = command;
            for (int i = 0; i < args.Length; i++)
            {
                parts[i + 1] = string.Format(args[0], file).Trim();
            }

            try
            {
                Process p = Process.Start(new ProcessStartInfo(command, parts[1]));
                if (p == null)
                    return false;

                try
                {
                    if (p.ExitCode == 0)
                        return true;
                    return false;
                }
                catch (InvalidOperationException itse)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return false;
            }
        }

    }

}