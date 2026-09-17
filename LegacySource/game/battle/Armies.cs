using System;
using System.Collections.Generic;
using System.IO;
using Game.Battle.Div;
using Game.Battle.Factors;
using Game.Battle.Formation;
using Game.Battle.Setting;
using Game.Debug;
using Game.Save;
using Init.Constant;
using Settlement.Entity;
using Settlement.Entity.Humanoid;
using Settlement.Main;
using Snake2D.Util.File;
using Snake2D.Util.Misc;
using Snake2D.Util.Sets;
using Util.Updating;
using View.Interrupter;
using View.Main;

namespace Game.Battle
{
    public class Armies : GameResource
    {
        public static readonly int ARMIES = 2;
        public static readonly int ARMIES_BITS = 0b011;
        public static readonly int DIVISIONS = ARMIES * Config.Battle().DIVISIONS_PER_ARMY;
        private readonly ArrayList<Div> divisions = new ArrayList<Div>(DIVISIONS);
        private readonly ArrayList<ArmyDiv> adivisions = new ArrayList<ArmyDiv>(DIVISIONS);
        private readonly ArrayList<Army> armies = new ArrayList<Army>(ARMIES);
        private readonly PrevMen prevMen = new PrevMen();
        public readonly DivDeployerUser placer;

        public readonly DivisionBanners banners = new DivisionBanners();
        public readonly TargetMap map = new TargetMap();
        public readonly DivFactors factors;
        public readonly BattleSettings settings;

        public readonly ArmySounds sound = new ArmySounds();

        public Armies(GAME game) : base("ARMIES", true)
        {
            for (int i = 0; i < ARMIES; i++)
            {
                new Army(armies, divisions);
            }

            placer = new DivDeployerUser(armies)
            {
                protected override bool blocked(int x, int y, Army a)
                {
                    if (VIEW.b().state() != null && VIEW.b().state().deploying())
                    {
                        if (a == GAME.ARMIES().player())
                            return !VIEW.b().state().deploymentBounds().holdsPoint(x >> C.T_SCROLL, y >> C.T_SCROLL);
                        return VIEW.b().state().deploymentBounds().holdsPoint(x >> C.T_SCROLL, y >> C.T_SCROLL);
                    }
                    return false;
                }
            };

            foreach (Div d in divisions)
                adivisions.add(d);

            settings = new BattleSettings(this);

            IDebugPanel.add("checkDivisionSpotOrder", new ACTION()
            {
                public override void exe()
                {
                    Bitmap2D check = new Bitmap2D(Config.Battle().MEN_PER_DIVISION, Config.Battle().DIVISIONS_PER_BATTLE, false);

                    new EntityIterator.Humans()
                    {
                        protected override bool processAndShouldBreakH(Humanoid h, int ie)
                        {
                            if (h.division() != null)
                            {
                                check.set(h.division().reporter.positionSpot(h), h.division().index(), true);
                            }
                            return false;
                        }
                    }.iterate();

                    foreach (Div d in divisions)
                    {
                        for (int i = 0; i < d.menNrOf(); i++)
                        {
                            if (!check.is(i, d.index()))
                            {
                                LOG.ln("errors in division " + d.index());
                                break;
                            }
                        }

                        if (d.menNrOf() > 0)
                        {
                            GAME.Notify(d.index());

                            for (int i = 0; i < d.menNrOf(); i++)
                            {
                                check.set(i, d.index(), false);
                            }

                            new EntityIterator.Humans()
                            {
                                protected override bool processAndShouldBreakH(Humanoid h, int ie)
                                {
                                    for (int i = 0; i < d.menNrOf(); i++)
                                    {
                                        check.set(i, d.index(), false);
                                    }

                                    if (h.division() == d)
                                    {
                                        if (h.divSpot() != d.reporter.positionSpot(h))
                                            LOG.ln(h.divSpot() + " -> " + d.reporter.positionSpot(h));

                                        check.set(h.division().reporter.positionSpot(h), h.division().index(), true);
                                    }

                                    return false;
                                }
                            }.iterate();
                            LOG.ln();
                            for (int i = 0; i < d.menNrOf(); i++)
                            {
                                int pi = d.reporter.positionSpot(i);
                                if (i != pi)
                                    LOG.ln(i + " -> " + pi);
                            }
                        }
                    }
                    GAME.Notify("test completed");
                }
            });
        }

        public void clear()
        {
            foreach (Army t in armies)
                t.saver.clear();
            foreach (ArmyDiv d in adivisions)
                d.clear();
            banners.clear();
            for (int i = 0; i < 4; i++)
            {
                GAME.ARMIES().divisions.get(i).info.menSet(50);
            }
        }

        protected override void save(FilePutter file)
        {
            foreach (Army t in armies)
                t.saver.save(file);
            foreach (ArmyDiv d in adivisions)
                d.save(file);
            banners.save(file);
        }

        protected override void load(FileGetter file)
        {
            foreach (Army t in armies)
                t.saver.load(file);
            foreach (ArmyDiv d in adivisions)
                d.load(file);
            banners.load(file);
        }

        protected override void loadFail()
        {
            clear();
        }

        private double ti = 0;
        protected override void update(double ds, Profiler profiler)
        {
            profiler.logStart(typeof(Div));
            profiler.logEnd(typeof(Div));
            profiler.logStart(typeof(DivFactors));
            factors.update(ds);
            profiler.logEnd(typeof(DivFactors));
            profiler.logStart(typeof(BattleSettings));
            settings.update(ds);
            profiler.logEnd(typeof(BattleSettings));

            ti += ds;
            if (ti > 0.1)
            {
                ti -= 0.1;
                SETT.BATTLE().info.update();
            }

            prevMen.update(ds);
        }

        public Army player()
        {
            return armies.get(0);
        }

        public Army enemy()
        {
            return armies.get(1);
        }

        public Div division(short armyDivisionID)
        {
            return divisions.get(armyDivisionID);
        }

        public LIST<Div> divisions()
        {
            return divisions;
        }

        public LIST<Army> armies()
        {
            return armies;
        }

        public void initAndTeleport(LIST<Div> divs)
        {
            foreach (Div d in divs)
            {
                settings.init(d);
            }
            GAME.BATTLE_THREADS().initAndTeleport(divs);
            foreach (Div d in divs)
            {
                factors.init(d);
                prevMen.men[d.index()] = d.menNrOf();
            }
        }

        public int prevMen(Div div)
        {
            return prevMen.men[div.index()];
        }

        private class PrevMen : IUpdater
        {
            private int[] men = Alloc.ii(Config.Battle().DIVISIONS_PER_BATTLE);

            public PrevMen() : base(Config.Battle().DIVISIONS_PER_BATTLE, 10)
            {
                new Savable("BATTLE_DIV_PREVIOUS_MEN")
                {
                    protected override void save(FilePutter file)
                    {
                        file.is(men);
                    }

                    protected override void load(FileGetter file)
                    {
                        file.is(men);
                    }
                };
            }

            protected override void update(int i, double timeSinceLast)
            {
                men[i] = GAME.ARMIES().division((short)i).men();
            }
        }
    }
}