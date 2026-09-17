using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace game.faction.diplomacy
{
    public class DIP : FactionResource
    {
        static DIP s;

        private readonly ArrayListGrower<DipStance> all = new ArrayListGrower<DipStance>();

        public readonly DipStance neutral;
        public readonly DWar enemies;
        public readonly DipStance traders;
        public readonly DipStance pact;
        public readonly DipStance allied;
        public readonly DipStance vassal;
        public readonly DipStance overlord;
        public readonly DipWarPlayer warPlayer = new DipWarPlayer();

        private readonly UpVassal uv = new UpVassal();
        private readonly Deal dealTmp;
        private readonly Deal dealTmp2;

        private readonly Bitsmap2D data = new Bitsmap2D(0, 4, FACTIONS.MAX(), FACTIONS.MAX());
        private readonly int[] overlords = new int[FACTIONS.MAX()];
        private readonly int[] secondOfStance = new int[FACTIONS.MAX() * FACTIONS.MAX()];
        public double playerWarSecond = -1;

        int stateI = -1;

        protected override void save(FilePutter file)
        {
            data.save(file);
            uv.save(file);
            file.isE(overlords);
            file.is(secondOfStance);
            file.d(playerWarSecond);
            warPlayer.teamName.save(file);
            warPlayer.warName.save(file);
        }

        protected override void load(FileGetter file) throws IOException
        {
            data.load(file);
            uv.load(file);
            file.isE(overlords);
            file.is(secondOfStance);
            playerWarSecond = file.d();
            warPlayer.teamName.load(file);
            warPlayer.warName.load(file);
        }

        protected override void update(double ds, Faction f)
        {
            uv.update();
        }

        public static DipStance NEUTRAL()
        {
            return s.neutral;
        }

        public static DWar WAR()
        {
            return s.enemies;
        }

        public static DipStance TRADE()
        {
            return s.traders;
        }

        public static Deal TMP()
        {
            return s.dealTmp;
        }

        public static Deal TMP2()
        {
            return s.dealTmp2;
        }

        public static DipStance ALLY()
        {
            return s.allied;
        }

        public static DipStance PACT()
        {
            return s.pact;
        }

        public static DipStance VASSAL()
        {
            return s.vassal;
        }

        public static DipStance OVERLORD()
        {
            return s.overlord;
        }

        public static DipWarPlayer WAR_PLAYER()
        {
            return s.warPlayer;
        }

        public static abstract class DipActivityListener
        {
            static readonly LinkedList<DipActivityListener> all = new LinkedList<DipActivityListener>();
            static DipActivityListener()
            {
                new GameDisposable
                {
                    protected override void dispose()
                    {
                        all.Clear();
                    }
                };
            }

            public DipActivityListener()
            {
                all.Add(this);
            }

            public abstract void change(Faction faction, Faction other, DipStance old, DipStance nn);
        }
    }
}