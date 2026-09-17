using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Law.Police
{
    public sealed class RoomPolice : RoomBlueprintIns<PoliceInstance>
    {
        private readonly PoliceConstructor constructor;
        private readonly List<BOOLEANImp> access = new List<BOOLEANImp>();
        public readonly PoliceWork work = new PoliceWork(this);
        public readonly BoostSpecs spec;

        public RoomPolice(RoomInitData init, RoomCategorySub block) : base(0, init, "_POLICE", block)
        {
            foreach (var hcr in HCLASS_RACE.ALL())
                access.Add(new BOOLEANImp(hcr.cl == HCLASSES.SLAVE()));

            constructor = new PoliceConstructor(this, init);

            BValue v = new BValue()
            {
                upIs = Alloc.ii(HCLASS_RACE.ALL().Count),
                vv = new double[HCLASS_RACE.ALL().Count],

                vGet = f =>
                {
                    return 0;
                },

                vGetPlayer = f =>
                {
                    return vGet(HCLASS_RACE.clP());
                },

                vGetHClassRace = cl =>
                {
                    if (upIs[cl.index] != GAME.updateI())
                    {
                        upIs[cl.index] = GAME.updateI();
                        if (cl.cl == null)
                        {
                            double pop = 0;
                            double v = 0;
                            for (int ci = 0; ci < HCLASSES.ALLP().Count; ci++)
                            {
                                double p = POP.pop(HCLASSES.ALLP()[ci], cl.race);
                                pop += p;
                                v += p * vGet(HCLASS_RACE.clP(cl.race, HCLASSES.ALLP()[ci]));
                            }
                            if (pop == 0)
                                vv[cl.index] = 0;
                            else
                                vv[cl.index] = v / pop;
                        }
                        else if (cl.race == null)
                        {
                            double pop = 0;
                            double v = 0;
                            for (int ri = 0; ri < RACES.all().Count; ri++)
                            {
                                double p = POP.pop(cl.cl, RACES.all()[ri]);
                                pop += p;
                                v += p * vGet(HCLASS_RACE.clP(RACES.all()[ri], cl.cl));
                            }
                            if (pop == 0)
                                vv[cl.index] = 0;
                            else
                                vv[cl.index] = v / pop;
                        }
                        else
                        {
                            if (!access[cl.index].is())
                                vv[cl.index] = 0;
                            else
                                vv[cl.index] = value();
                        }
                    }

                    return vv[cl.index];
                },

                vGetDiv = div =>
                {
                    return vGet(HCLASS_RACE.clP(div.race(), HCLASSES.CITIZEN()));
                },

                vGetInduvidual = indu =>
                {
                    return vGet(indu.popCL());
                },

                vGetRegion = reg =>
                {
                    return 0;
                }
            };

            spec = new BoostSpecs(new BSourceInfo(info.name, icon), true);
            spec.read(init.data(), v);
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override Furnisher constructor()
        {
            return constructor;
        }

        protected override void saveP(FilePutter f)
        {
            HCLASS_RACE.MAP().saver().save(access, f);
        }

        protected override void loadP(FileGetter f)
        {
            HCLASS_RACE.MAP().loader().load(access, f);
        }

        protected override void clearP()
        {
            foreach (var b in access)
                b.set(false);
        }

        public override SFinderRoomService service(int tx, int ty)
        {
            return null;
        }

        public override void appendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).make());
        }

        public BOOLEAN_MUTABLE access(HCLASS_RACE g)
        {
            return access[g.index];
        }

        public double value()
        {
            double pop = 0;
            foreach (var r in HCLASS_RACE.ALL())
            {
                if (access[r.index].is())
                    pop += STATS.POP().POP.data(r.cl).get(r.race);
            }

            if (pop == 0)
                return employment().employed() > 0 ? 1 : 0;

            return Math.Sqrt(CLAMP.d(employment().employed() / pop, 0, 1));
        }

        public double value(HCLASS cl, Race race)
        {
            if (!access[HCLASS_RACE.clP(race, cl).index].is())
                return 0;

            double pop = 0;
            foreach (var r in HCLASS_RACE.ALL())
            {
                if (access[r.index].is())
                    pop += STATS.POP().POP.data(r.cl).get(r.race);
            }

            if (pop == 0)
                return employment().employed() > 0 ? 1 : 0;

            return Math.Sqrt(CLAMP.d(employment().employed() / pop, 0, 1));
        }
    }
}