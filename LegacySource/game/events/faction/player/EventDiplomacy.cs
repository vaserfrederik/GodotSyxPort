using System;
using System.IO;
using game.events.EVENTS;
using game.faction.FACTIONS;
using game.faction.Faction;
using game.faction.diplomacy.DIP;
using game.faction.diplomacy.DipStance;
using game.faction.npc;
using game.time.TIME;
using settlement.stats;
using snake2d.util.file;
using util;

namespace game.events.faction.player
{
    public class EventDiplomacy : EventResource
    {
        private readonly double dTime = TIME.secondsPerDayI();
        private int ii;
        private double timer = 1;

        private readonly War war = new War();
        private readonly Peace peace = new Peace();
        private readonly Stance stance = new Stance();

        private readonly EData[] datas = new EData[FACTIONS.MAX()];

        public EventDiplomacy() : base("DIPLOMACY")
        {
            for (int i = 0; i < datas.Length; i++)
                datas[i] = new EData();

            new FactionActivityListener
            {
                Remove = f => { },
                Add = f => datas[f.index()].clear()
            };

            new DIP.DipActivityListener
            {
                Change = (faction, other, old, nn) =>
                {
                    bool w = datas[faction.index()].welcomed;
                    datas[faction.index()].clear();
                    datas[faction.index()].welcomed = w;
                    w = datas[other.index()].welcomed;
                    datas[other.index()].clear();
                    datas[other.index()].welcomed = w;
                    datas[other.index()].stanceMess = false;
                    datas[faction.index()].stanceMess = false;
                }
            };
        }

        protected override void update(double ds)
        {
            timer -= ds * dTime * FACTIONS.NPCs().size();
            while (timer < 0)
            {
                timer++;
                if (ii >= FACTIONS.NPCs().size())
                {
                    ii = 0;
                    war.updateAll(TIME.secondsPerDay());
                    peace.update();
                }
                FactionNPC fa = FACTIONS.NPCs().get(ii);
                process(fa);
                ii++;
            }
        }

        private void process(FactionNPC fa)
        {
            if (fa.request.has())
                return;

            if (DIP.WAR().is(fa))
                return;

            if (war.updateDay(fa))
                return;

            EData data = datas[fa.index()];
            Induvidual king = fa.court().king().roy().induvidual;

            if (stance.process(fa, king, data))
                return;
        }

        protected override void save(FilePutter file)
        {
            file.i(ii);
            file.d(timer);
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i].save(file);
            }
            war.save(file);
        }

        protected override void load(FileGetter file)
        {
            ii = file.i();
            timer = file.d();
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i].load(file);
            }
            war.load(file);
        }

        protected override void clear()
        {
            ii = 0;
            timer = 0;
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i].clear();
            }
            war.clear();
        }

        public void dismissWelcome(FactionNPC f)
        {
            datas[f.index()].welcomed = true;
        }

        public class EData : SAVABLE
        {
            public bool welcomed = false;
            public bool stanceMess = false;

            public override void save(FilePutter file)
            {
                file.bool(welcomed);
                file.bool(stanceMess);
            }

            public override void load(FileGetter file)
            {
                welcomed = file.bool();
                stanceMess = file.bool();
            }

            public override void clear()
            {
                welcomed = false;
                stanceMess = false;
            }
        }

        public void debug(Debugger d, FactionNPC npc)
        {
            d.title(GetType().Name);
            d.debug("timer").add(timer);
            d.debug("Faction Prev").add(ii).s().add(FACTIONS.NPCs().getC(ii).name);
            d.debugObject("data", datas[npc.index()]);
        }
    }
}