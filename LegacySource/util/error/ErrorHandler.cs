using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Util.Error
{
    public class ErrorHandler : ERROR_HANDLER
    {
        private const string bugMail = "info@songsofsyx.com";
        private const string pgmname = "Songs of syx";

        public ErrorHandler()
        {
        }

        public override void Handle(string output, string dump)
        {
            string path = null;

            try
            {
                if (PATHS.Local().Logs.Exists("UnhandledDump"))
                    PATHS.Local().Logs.Delete("UnhandledDump");
                string p = PATHS.Local().Logs.Get() + Path.DirectorySeparatorChar + "UnhandledDump.txt";
                if (File.Create(p).Close())
                {
                    File.WriteAllText(p, dump);
                    Console.WriteLine("saved " + p);
                    path = p;
                }
            }
            catch (IOException e1)
            {
                e1.printStackTrace();
            }

            Error(null, 2, "Unhandled error output: " + Environment.NewLine + output, dump, path);
        }

        public override void Handle(DataError e, string dump)
        {
            Error(e, 0, e.Error, dump, e.Path);
        }

        public override void Handle(GameError e, string dump)
        {
            Error(e, 1, e.Error, dump, null);
        }

        public override void Handle(Exception e, string dump)
        {
            if (IsModError(e))
            {
                e.printStackTrace();
                Handle(new DataError("An error has occured caused by a code mod. Please inform the modders of this error."), dump);
            }
            else
                Error(e, 3, e.GetType().Name + ": " + e.Message, dump, null);
        }

        private bool IsModError(Exception e)
        {
            if (e is MissingFieldException)
            {
                return true;
            }

            if (e is MissingMethodException)
            {
                return true;
            }

            return false;
        }

        private void Error(Exception ee, int type, string message, string dump, string dataPath)
        {
            // save data;
            string p = new FileInfo("error.txt").FullName;
            try
            {
                if (PATHS.Local().Logs.GetFiles().Length > 50)
                {
                    int am = PATHS.Local().Logs.GetFiles().Length - 25;
                    foreach (string f in PATHS.Local().Logs.GetFiles())
                    {
                        new FileInfo(f).Delete();
                        if (am-- < 0)
                            break;
                    }
                }
                else
                {
                    while (PATHS.Local().Logs.GetFiles().Length > 25)
                    {
                        string l = null;
                        long ff = long.MaxValue;
                        foreach (string f in PATHS.Local().Logs.GetFiles())
                        {
                            long lm = new FileInfo(f).LastWriteTime.Ticks;
                            if (lm < ff)
                            {
                                l = f;
                                ff = lm;
                            }
                        }
                        if (l != null)
                        {
                            new FileInfo(l).Delete();
                        }
                    }
                }

                p = FileManager.NAME.TimeStampString(PATHS.Local().Logs.Get() + Path.DirectorySeparatorChar + "error") + ".txt";
            }
            catch (Exception e)
            {
            }

            if (type == 3)
            {
                try
                {
                    if (File.Create(p).Close())
                    {
                        File.WriteAllText(p, dump);
                        Console.WriteLine("saved " + p);
                    }
                }
                catch (IOException e1)
                {
                    e1.printStackTrace();
                }
            }

            string dumpFile = p;

            if (dataPath == null)
                dataPath = "none";
            if (message == null)
                message = "no message";
            if (dumpFile == null)
                dumpFile = "none";

            message = message.Replace("\"", "Quote");
            if (message.Length > 8000)
                message = message.Substring(0, 8000);

            string eee = "unhandled " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (ee != null)
                eee = VERSION.VERSION_STRING + " " + ee.ToString() + " ";
            if (ee != null && ee.StackTrace != null && ee.StackTrace.Length > 0)
            {
                eee += ee.StackTrace.Split('\n')[0].Replace("   at ", "");
            }

            string[] args = new string[]
            {
                pgmname,
                bugMail,
                "" + type,
                message,
                dumpFile,
                dataPath,
                eee
            };

            Proccesser.Exec(ErrorMessage.class, new string[] { }, args, new string[] { });
        }
    }
}