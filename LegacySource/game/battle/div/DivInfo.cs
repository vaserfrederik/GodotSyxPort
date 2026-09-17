using System;
using System.IO;

namespace game.battle.div
{
    public class DivInfo : DIV_SPECE
    {
        private Str name = new Str(32);
        private Div div;
        private int menTarget;
        private int raceI;
        private int exMin;
        private int symbolI;
        private DoubleImp[] trains;

        public DivInfo(Div div)
        {
            this.div = div;
            raceI = 0;
            symbolI = div.index();
            name.clear().add(Dic.¤¤Division).add(' ').add('#').add(div.index());
            trains = new DoubleImp[ROOM_M_TRAINER.ALL().size()];
            for (int i = 0; i < trains.Length; i++)
                trains[i] = new DoubleImp();
        }

        public Div div()
        {
            return div;
        }

        public override int men()
        {
            return menTarget;
        }

        public override void menSet(int am)
        {
            if (div.army() == GAME.ARMIES().player())
            {
                SETT.BATTLE().info.clearTargets();
            }
            menTarget = CLAMP.i(am, 0, div.men.freeSpots() + div.menNrOf());
        }

        public override double equip(EquipBattle e)
        {
            return (double)e.target(div) / e.equipMax;
        }

        public override void equipSet(EquipBattle e, double d)
        {
            e.targetSet(div, (int)Math.Round(d * e.max()));
        }

        public override double training(StatTraining e)
        {
            return trains[e.room.INDEX_TRAINING].getD();
        }

        public override void trainingSet(StatTraining e, double d)
        {
            trains[e.room.INDEX_TRAINING].setD(d);
        }

        public override double experience()
        {
            return STATS.BATTLE().COMBAT_EXPERIENCE.div().getD(div);
        }

        public override Faction faction()
        {
            return FACTIONS.player();
        }

        public DivisionBanner banner()
        {
            return GAME.ARMIES().banners.get(symbolI);
        }

        public override Str name()
        {
            return name;
        }

        public override int bannerI()
        {
            return symbolI;
        }

        public Race race()
        {
            return RACES.all().get(raceI);
        }

        public override void raceSet(Race race)
        {
            int men = men();
            menSet(0);
            raceI = race.index;
            menSet(men);
        }

        public override void experienceSet(double experience)
        {
            // TODO Auto-generated method stub
        }

        public override Str nameE()
        {
            return name;
        }

        public override void bannerISet(int bannerI)
        {
            symbolI = bannerI;
        }

        public override void factionSet(Faction faction)
        {
            // TODO Auto-generated method stub
        }

        private SAVABLE saver = new SAVABLE()
        {
            public void save(FilePutter file)
            {
                foreach (DoubleImp t in trains)
                {
                    t.save(file);
                }

                file.i(menTarget);
                file.i(raceI);
                file.i(exMin);
                file.i(symbolI);
                name.save(file);
            }

            public void load(FileGetter file)
            {
                foreach (DoubleImp t in trains)
                {
                    t.load(file);
                }
                menTarget = file.i();
                raceI = file.i();
                exMin = file.i();
                symbolI = file.i();
                name.load(file);
            }

            public void clear()
            {
                foreach (DoubleImp t in trains)
                {
                    t.setD(0);
                }
                menTarget = 0;
                raceI = FACTIONS.player().race().index;
                exMin = 0;
                symbolI = div.index();
                name.clear().add(Dic.¤¤Division).add(' ').add('#').add(div.index());
            }
        };
    }
}