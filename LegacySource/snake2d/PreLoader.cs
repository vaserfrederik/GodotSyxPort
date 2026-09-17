using System;
using System.Diagnostics;
using System.IO;

namespace snake2d
{
    public static class PreLoader
    {
        private static Process preloader;

        public static void Load(string version, string pathToImage, string pathToIcon)
        {
            Exit();

            try
            {
                if (OS.Get() != OS.MAC)
                {
                    preloader = Proccesser.Exec(typeof(PreLoaderSwing), new string[] { }, new string[] { version, pathToImage, pathToIcon }, new string[] { });
                }
            }
            catch (Exception e)
            {
                PreLoader.Exit();
                e.printStackTrace();
                return;
            }
        }

        public static void Exit()
        {
            Process p = preloader;
            preloader = null;
            if (p == null)
                return;

            if (!p.HasExited)
            {
                try
                {
                    p.StandardInput.Write('s');
                    p.StandardInput.Write('s');
                    p.StandardInput.Write('s');
                    p.StandardInput.Write('s');
                    p.StandardInput.Flush();
                    // p.Kill();
                    // p.WaitForExit();
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
            preloader = null;
        }
    }
}