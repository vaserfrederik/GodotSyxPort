using System;

namespace Integrations
{
    class INTEGRATIONS
    {
        private static volatile INTEGRATIONS I;

        public static void Init(bool log, bool achieve)
        {
            if (I != null)
            {
                throw new Exception("Already initialized");
            }
            try
            {
                new INTEGRATIONS(log, achieve);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                I = null;
            }
        }

        private readonly RPCHandler handler;
        private readonly SteamClient steam;
        private readonly SteamStats stats;
        private static bool logging = false;

        private INTEGRATIONS(bool logging, bool achieve) 
        {
            INTEGRATIONS.logging = logging;
            I = this;
            steam = SteamClient.Init();

            handler = new RPCHandler(steam);
            if (steam != null)
            {
                if (achieve)
                    stats = new SteamStats();
                else
                    stats = null;
            }
            else
            {
                stats = null;
            }

            Log("INTEGRATION INITED");
            Log("STEAM: " + SteamRunning());
            Log("DISCORD: " + true);
        }

        public static void Dispose()
        {
            if (I != null)
            {
                I.handler.Dispose();

                if (I.steam != null)
                {
                    if (I.stats != null)
                        I.stats.Dispose();
                    I.steam.Dispose();
                }
                I = null;
            }
        }

        public static void UpdateRPC(INTER_RPC rpc)
        {
            if (I != null)
            {
                I.handler.Update(rpc);
            }
        }

        public static bool SteamRunning()
        {
            return I != null && I.steam != null && I.steam.Running();
        }

        public static bool Inited()
        {
            return I != null;
        }

        static void Log(object obj)
        {
            if (logging)
            {
                Console.WriteLine("[INTEGRATIONS] " + obj);
            }
        }

        public static void Achieve(string key, int value)
        {
            if (I != null && I.stats != null)
            {
                I.stats.SetStat(key, value);
            }
        }

        public static void Achieve(string key)
        {
            if (I != null && I.stats != null)
            {
                I.stats.SetAchieved(key);
            }
        }

        public static void Reset()
        {
            if (I != null && I.stats != null)
            {
                I.stats.Reset(true);
            }
        }

        public static void AchieveInc(string key, int value)
        {
            if (I != null && I.stats != null)
            {
                I.stats.IncStat(key, value);
            }
        }

        public static void AchievementsFlush()
        {
            if (I != null && I.stats != null)
            {
                I.stats.StoreStats();
            }
        }
    }
}