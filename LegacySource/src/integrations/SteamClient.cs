using System;
using System.Threading;
using com.codedisaster.steamworks;

namespace integrations
{
    internal sealed class SteamClient
    {
        private static readonly long interval = 5 * 1000;

        private SteamUtils clientUtils;
        private SteamUtilsCallback clUtilsCallback = new SteamUtilsCallback
        {
            OnSteamShutdown = () => { }
        };

        private readonly Thread Callbacker;
        private volatile bool die = false;

        private SteamClient()
        {
            clientUtils = new SteamUtils(clUtilsCallback);

            Callbacker = new Thread(() =>
            {
                long last = 0;
                Thread t = Thread.CurrentThread;
                while (!die && t.IsAlive && SteamAPI.IsSteamRunning())
                {
                    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    // System.out.println(INTEGRATIONS.inited() + " " + SteamAPI.isSteamRunning());
                    if (now - last > interval)
                    {
                        SteamAPI.RunCallbacks();
                        last = now;
                    }
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (ThreadInterruptedException e)
                    {
                    }
                }
            });
            Callbacker.Name = "steam callback";
            Callbacker.Start();
        }

        public void Dispose()
        {
            die = true;
            Callbacker.Interrupt();
            try
            {
                Callbacker.Join((int)interval);
            }
            catch (ThreadInterruptedException e)
            {
            }
            clientUtils.Dispose();
            SteamAPI.Shutdown();
        }

        public static SteamClient Init()
        {
            try
            {
                SteamAPI.LoadLibraries();

                if (SteamAPI.Init() && SteamAPI.IsSteamRunning())
                {
                    return new SteamClient();
                }
            }
            catch (SteamException e)
            {
                e.PrintStackTrace();
            }
            return null;
        }

        public bool Running()
        {
            return SteamAPI.IsSteamRunning();
        }
    }
}