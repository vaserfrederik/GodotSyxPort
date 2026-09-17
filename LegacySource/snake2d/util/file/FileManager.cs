using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Snake2D.Util.File
{
    public abstract class FileManager
    {
        public static class NAME
        {
            private NAME() { }

            public static string TimeStampString(string original)
            {
                string postfix = DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss-fff");
                return original + postfix;
            }

            public const string legalChars = "aA - zZ, 0-9, -, _, 'space'";

            private static readonly Regex okChars = new Regex("[-_ A-Za-z0-9]+");

            public static bool OkName(string filename)
            {
                return okChars.IsMatch(filename);
            }
        }

        public static class FILE
        {
            public static bool Exists(string pathname)
            {
                FileInfo f = new FileInfo(pathname);
                if (f.Exists)
                {
                    string p2 = f.FullName;
                    for (int i = 0; i < p2.Length; i++)
                    {
                        if (p2[i] != pathname[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            public static string EnsureExists(string pathname)
            {
                if (!Exists(pathname))
                    throw new Exception("File missing: " + pathname);
                return pathname;
            }

            public static string ToString(string path)
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (IOException e)
                {
                    throw new Exception(e.Message);
                }
            }

            public static string ToStringRelative(object o, string name)
            {
                string path = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name)).FullName;

                try
                {
                    return File.ReadAllText(path);
                }
                catch (IOException e)
                {
                    throw new Exception(e.Message);
                }
            }

            public static bool ReadWriteRights()
            {
                try
                {
                    return Directory.Exists(Path.GetTempPath());
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                    return false;
                }
            }
        }

        public static void OpenDesctop(string path)
        {
            Process.Start("explorer.exe", path);
        }

        public static bool SendEmail(string mail, string mess, string title)
        {
            try
            {
                string uri = "mailto:" + mail + "?subject=" + title + "&body=" + Uri.EscapeDataString(mess);

                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            }
            catch (Exception e1)
            {
                e1.printStackTrace();
                return false;
            }
            return true;
        }
    }
}