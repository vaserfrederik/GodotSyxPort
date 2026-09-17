using System;
using System.Collections.Generic;

namespace game.battle.thread.status
{
    public sealed class DivStatus : SAVABLE
    {
        byte enemyDirMask;

        double flanks = 0;
        double encirclement = 0;
        double enemyThreats = 0;
        double friends = 0;
        short engagements = 0;

        static readonly int iSize = 8;
        private static int iFriendlyColl = 0;
        private static int iEnemyColl = iSize;
        private static int iEnemyInRange = 2 * iSize;
        private static int iEnemyDist = 3 * iSize;
        private static int iFriendlyInRange = 4 * iSize;
        private static int iEnemyCharging = 5 * iSize;

        readonly short[] lists = new short[iEnemyCharging + iSize];

        // dir faceEnemyDirection (the best way to face the enemy
        // faceEnemyWidth

        public DivStatus()
        {
        }

        public bool ThreatAt(DIR d, Div div)
        {
            return GAME.ARMIES().factors.projectiles(div) > 0 || (enemyDirMask != 0 && (Threat(d) || Threat(d.Next(-1)) || Threat(d.Next(1))));
        }

        public bool Threat(DIR d)
        {
            if (d.IsOrtho())
            {
                return (enemyDirMask & d.Mask()) != 0;
            }
            else
            {
                return ((enemyDirMask) >> 4 & d.Mask()) != 0;
            }
        }

        public override void Save(FilePutter file)
        {
            file.Ss(lists);
            file.B(enemyDirMask);
            file.D(flanks);
            file.D(enemyThreats);
            file.D(friends);
            file.S(engagements);
        }

        public override void Load(FileGetter file)
        {
            file.Ss(lists);
            enemyDirMask = file.B();
            flanks = file.D();
            enemyThreats = file.D();
            friends = file.D();
            engagements = file.S();
        }

        public override void Clear()
        {
            Array.Fill(lists, (short)-1);
            enemyDirMask = 0;
            flanks = 0;
            enemyThreats = 0;
            friends = 0;
            engagements = 0;
        }

        public LIST<Div> FriendlyCollisions(LISTE<Div> res)
        {
            return Fill(iFriendlyColl, res);
        }

        public int FriendlyCollisions()
        {
            return Count(iFriendlyColl);
        }

        void FriendlyCollisionSet(short di)
        {
            Set(iFriendlyColl, di);
        }

        public LIST<Div> EnemyCollisions(LISTE<Div> res)
        {
            return Fill(iEnemyColl, res);
        }

        public int EnemyCollisions()
        {
            return Count(iEnemyColl);
        }

        void EnemyCollisionSet(short di)
        {
            Set(iEnemyColl, di);
        }

        public LIST<Div> EnemiesClosest(LISTE<Div> res)
        {
            return Fill(iEnemyInRange, res);
        }

        public int EnemiesClosest()
        {
            return Count(iEnemyInRange);
        }

        public Div EnemyClosest()
        {
            return GetFirst(iEnemyInRange);
        }

        public int EnemyClosestDist()
        {
            return lists[iEnemyDist];
        }

        public int EnemyClosestDist(int i)
        {
            return lists[iEnemyDist + i];
        }

        void EnemiesClosestSet(short di, int tiles)
        {
            Set(iEnemyInRange, di);
            Set(iEnemyDist, (short)(tiles & 0x0FFFF));
        }

        public LIST<Div> FriendlyClosest(LISTE<Div> res)
        {
            return Fill(iFriendlyInRange, res);
        }

        public int FriendlyClosest()
        {
            return Count(iFriendlyInRange);
        }

        void FriendlyClosestSet(short di)
        {
            Set(iFriendlyInRange, di);
        }

        private void Set(int start, short di)
        {
            if (lists[start + iSize - 1] != -1)
                return;
            for (int i = 0; i < iSize; i++)
            {
                int k = i + start;
                if (lists[k] == -1)
                {
                    lists[k] = di;
                    return;
                }
            }
        }

        private int Count(int start)
        {
            for (int i = 0; i < iSize; i++)
            {
                int k = i + start;
                if (lists[k] == -1)
                    return i;
            }
            return iSize;
        }

        private LIST<Div> Fill(int start, LISTE<Div> res)
        {
            for (int i = 0; i < iSize; i++)
            {
                int k = i + start;
                if (lists[k] == -1)
                    return res;
                res.Add(GAME.ARMIES().Division(lists[k]));
                if (!res.HasRoom())
                    return res;
            }
            return res;
        }

        private Div GetFirst(int start)
        {
            if (lists[start] == -1)
                return null;
            return GAME.ARMIES().Division(lists[start]);
        }

        public bool IsFighting()
        {
            return engagements > 0;
        }

        public double AdjacentFriendsPower()
        {
            return friends;
        }

        public double AdjacentEnemiesPower()
        {
            return enemyThreats;
        }

        public double EncirclementPower()
        {
            return encirclement;
        }

        public int Engagements()
        {
            return engagements;
        }

        public double Flanks()
        {
            return flanks;
        }
    }
}