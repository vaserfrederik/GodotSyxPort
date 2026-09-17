using System;

namespace Integrations
{
    public class SteamAchieve
    {
        public const string RPC = "RPC";
        public const string ACHIEVE = "ACHIEVE";
        public const string ACHIEVE_NOT = "NOT_ACHIEVE";
        public const string EXIT = "EXIT";
        public const string SEP = "%%";

        public static void Main(string[] achievements)
        {
            SteamClient steam = SteamClient.Init();
            if (steam == null)
            {
                Console.WriteLine("STEAM ACHIEVEMENTS COULD NOT BE INITED");
                return;
            }

            SteamStats stats = new SteamStats();

            if (achievements.Length == 0)
            {
                stats.Reset(true);
            }
            else
            {
                for (int i = 0; i < achievements.Length; i++)
                {
                    stats.SetAchieved(achievements[i]);
                }
                stats.StoreStats();
            }

            stats.Dispose();
            steam.Dispose();
        }
    }
}