using System;
using System.Threading;
using Steamworks;

namespace Integrations
{
    internal sealed class SteamStats
    {
        private SteamUserStats stats;
        private SteamResult statResult;
        private volatile bool isInit = false;
        private int numAchievements;

        private readonly SteamUserStatsCallback userStatsCallback = new SteamUserStatsCallback();

        public SteamStats()
        {
            Init();
        }

        private void Init()
        {
            INTEGRATIONS.Log("Register Userstats ...");
            stats = new SteamUserStats(userStatsCallback);
            stats.RequestCurrentStats();
            for (int i = 0; i < 5000; i++)
            {
                if (isInit)
                    return;
                Thread.Sleep(1);
            }
        }

        private void ListAchievements()
        {
            INTEGRATIONS.Log("Songs of Syx has " + numAchievements + " Achievements:");
            for (int i = 0; i < numAchievements; i++)
                INTEGRATIONS.Log("Achievement " + i + ": " + stats.GetAchievementName(i));
        }

        public void GetStat(string name)
        {
            if (isInit)
            {
                for (int i = 0; i < 34; i++)
                    INTEGRATIONS.Log("Achievement Name: " + stats.GetAchievementName(i));
                int value = 100;
                value = stats.GetStatI(name, value);
                INTEGRATIONS.Log("Stat " + name + " has the value of " + value);
            }
            else
                INTEGRATIONS.Log("SteamStats not properly initialized");
        }

        // Incrementing a stat
        public bool IncStat(string name, int value)
        {
            if (isInit)
            {
                int oldValue = stats.GetStatI(name, value);
                int newValue = oldValue + value;
                INTEGRATIONS.Log("Increasing " + name + " by " + value + " to " + newValue);
                return stats.SetStatI(name, oldValue + value);
            }
            else
            {
                INTEGRATIONS.Log("SteamStats not properly initialized");
                return false;
            }
        }

        public bool SetStat(string name, int value)
        {
            if (isInit)
            {
                return stats.SetStatI(name, value);
            }
            else
            {
                INTEGRATIONS.Log("SteamStats not properly initialized");
                return false;
            }
        }

        // Setting a highscore for a stat
        public bool SetMaxStat(string name, int value)
        {
            if (isInit)
            {
                int oldValue = stats.GetStatI(name, value);
                if (value > oldValue)
                    return stats.SetStatI(name, value);
                else
                {
                    INTEGRATIONS.Log("Trying to set a new highscore (" + value + ") for " + name + " but the old one is higher (" + oldValue + ")");
                    return false;
                }
            }
            else
            {
                INTEGRATIONS.Log("SteamStats not properly initialized");
                return false;
            }
        }

        public bool GetAchieved(string name)
        {
            bool isAchieved = false;
            if (isInit)
            {
                isAchieved = stats.IsAchieved(name, isAchieved);
                INTEGRATIONS.Log("Achievement " + name + " achieved?  " + isAchieved);
                return isAchieved;
            }
            else
            {
                INTEGRATIONS.Log("SteamStats not properly initialized");
                return false;
            }
        }

        // Show an progress popup in Steamoverlay
        public bool IndicateProgress(string name)
        {
            int curProgress = 0;
            int maxProgress = 0;
            bool result = stats.IndicateAchievementProgress(name, curProgress, maxProgress);
            INTEGRATIONS.Log("Indicating progress for " + name + " (" + curProgress + "/" + maxProgress + ")");
            return result;
        }

        // Unlocks an achievement
        public void SetAchieved(string name)
        {
            bool isAchieved = false;
            if (isInit)
            {
                isAchieved = stats.IsAchieved(name, isAchieved);
                if (!isAchieved)
                {
                    stats.SetAchievement(name);
                    stats.StoreStats();
                    INTEGRATIONS.Log("Unlocking Achievement " + name);
                }
                else
                    INTEGRATIONS.Log("Trying to unlock Achievement " + name + " but it is already unlocked");
            }
            else
                INTEGRATIONS.Log("SteamStats not properly initialized");
        }

        // Publish and sync changes to Steam server
        public void StoreStats()
        {
            INTEGRATIONS.Log("Storing UserStats...");
            stats.StoreStats();
        }

        // Deletes ALL stats and optionally Achievements
        // ONLY for testing!!
        public void Reset(bool achievementsToo)
        {
            stats.ResetAllStats(achievementsToo);
        }

        public void Dispose()
        {
            stats.Dispose();
        }
    }
}