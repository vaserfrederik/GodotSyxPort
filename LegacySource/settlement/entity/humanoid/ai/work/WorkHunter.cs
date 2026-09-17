using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.work;
using settlement.room.food.hunter;
using settlement.room.main;
using settlement.stats;
using settlement.thing.ThingsCadavers;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.work
{
    public sealed class WorkHunter : PlanBlueprint
    {
        private readonly ROOM_HUNTER b;

        protected WorkHunter(ROOM_HUNTER b, AIModule_Work module, PlanBlueprint[] map) : base(module, b, map)
        {
            this.b = b;
        }

        public override AISubActivation init(Humanoid a, AIManager d)
        {
            RoomInstance inInstance = work(a);

            if (!PATH().finders.entryPoints.anyHas(a.tc().x(), a.tc().y()))
                return null;

            COORDINATE j = b.reserveWork(inInstance, a);

            if (j == null)
            {
                GAME.Notify("Weird " + inInstance.mX() + " " + inInstance.mY());
                return null;
            }
            d.planTile.set(j);

            if (getCadaver(a, d) != null)
            {
                AISubActivation s = walk.set(a, d);

                if (s == null)
                    b.workFinish(d.planTile);
                else
                    return s;
            }

            if (STATS.WORK().WORK_TIME.indu().getD(a.indu()) < 0.5)
            {
                AISubActivation s = leave.set(a, d);
                return s;
            }

            AISubActivation s = walk.set(a, d);

            if (s == null)
                b.workFinish(d.planTile);
            return s;
        }

        private readonly Resumer walk = new Resumer
        {
            setAction = (a, d) =>
            {
                STATS.WORK().proximityStart(a);
                return AI.SUBS().walkTo.coo(a, d, d.planTile);
            },
            res = (a, d) =>
            {
                STATS.WORK().proximityEnd(a);
                return butcher.set(a, d);
            },
            can = (a, d) => b.workFinish(d.planTile),
            con = (a, d) => work(a) != null && work(a).blueprint() == b
        };

        private readonly Resumer leave = new Resumer
        {
            setAction = (a, d) =>
            {
                if (PATH().finders.entryPoints.any(a.tc().x(), a.tc().y(), d.path, int.MaxValue))
                {
                    b.reportSkill(work(a), a);
                    return AI.SUBS().walkTo.pathFull(a, d);
                }
                return null;
            },
            res = (a, d) => hunt.set(a, d),
            can = (a, d) => b.workFinish(d.planTile),
            con = (a, d) => work(a) != null && work(a).blueprint() == b
        };

        private readonly Resumer hunt = new Resumer
        {
            setAction = (a, d) =>
            {
                SETT.ENTITIES().moveIntoTheTheUnknown(a);
                a.speed.magnitudeInit(0);
                return AI.SUBS().STAND.activate(a, d);
            },
            res = (a, d) =>
            {
                b.reportSkill(work(a), a);
                if (STATS.WORK().WORK_TIME.indu().getD(a.indu()) > 0.5 || !AIModules.current(d).moduleCanContinue(a, d))
                {
                    can(a, d);
                    return drag.set(a, d);
                }
                return AI.SUBS().STAND.activate(a, d);
            },
            can = (a, d) =>
            {
                SETT.ENTITIES().returnFromTheTheUnknown(a);
                b.workFinish(d.planTile);
            },
            con = (a, d) => work(a) != null && work(a).blueprint() == b
        };

        private readonly Resumer drag = new Resumer
        {
            setAction = (a, d) =>
            {
                AnimalSpecies ss = spe();
                Cadaver c = SETT.THINGS().cadavers.normal(a.tc().x(), a.tc().y(), ss.mass() * RND.rFloat1(1.1), 1, ss, 2);
                if (c == null)
                    return null;

                d.planObject = c.index();
                RoomInstance inInstance = work(a);

                COORDINATE j = b.reserveWork(inInstance, a);

                if (j == null)
                {
                    GAME.Notify("Weird " + inInstance.mX() + " " + inInstance.mY());
                    return null;
                }
                d.planTile.set(j);

                AISubActivation ac = AI.SUBS().walkTo.drag(a, d, THINGS().cadavers.draggable, c.index(), d.planTile);
                if (ac != null)
                    return ac;

                can(a, d);
                return null;
            },
            res = (a, d) =>
            {
                Cadaver old = THINGS().cadavers.tGet.get(d.planTile);
                if (old != null)
                    old.remove();
                Cadaver c = getCadaver(a, d);
                c.drag(DIR.ORTHO.get(ROOMS().fData.item.get(d.planTile).rotation), (d.planTile.x() << C.T_SCROLL) + C.TILE_SIZEH, (d.planTile.y() << C.T_SCROLL) + C.TILE_SIZEH, 0);
                return butcher.set(a, d);
            },
            can = (a, d) => b.workFinish(d.planTile),
            con = (a, d) => work(a) != null && work(a).blueprint() == b && getCadaver(a, d) != null
        };

        private readonly Resumer butcher = new Resumer
        {
            res = (a, d) =>
            {
                if (AIModules.current(d).moduleCanContinue(a, d))
                {
                    Cadaver prey = getCadaver(a, d);
                    if (prey != null)
                    {
                        b.work(work(a), d.planTile, a, true);
                        can(a, d);
                        if (prey.resHas())
                            prey.resRemove();
                        b.employment().sound().rnd(a);
                        return AI.SUBS().WORK_HANDS.activate(a, d, 20);
                    }
                    else
                    {
                        b.work(work(a), d.planTile, a, false);
                        return AI.SUBS().STAND.activateRndDir(a, d);
                    }
                }

                b.workFinish(d.planTile);
                return null;
            },
            con = (a, d) => work(a) != null && work(a).blueprint() == b,
            can = (a, d) => b.workFinish(d.planTile),
            setAction = (a, d) =>
            {
                b.reportSkill(work(a), a);
                return AI.SUBS().STAND.activate(a, d);
            }
        };

        private Cadaver getCadaver(Humanoid a, AIManager d)
        {
            return THINGS().cadavers.tGet.get(d.planTile);
        }

        protected override void name(Humanoid a, AIManager d, Str string)
        {
            string.add(b.employment().verb);
        }

        private AnimalSpecies spe()
        {
            double tot = 0;
            foreach (AnimalSpecies s in SETT.ANIMALS().sett())
            {
                tot += s.occurence(SETT.WORLD_AREA().climate());
            }
            tot *= RND.rFloat();
            foreach (AnimalSpecies s in SETT.ANIMALS().sett())
            {
                tot -= s.occurence(SETT.WORLD_AREA().climate());
                if (tot <= 0)
                    return s;
            }
            return SETT.ANIMALS().sett().rnd();
        }
    }
}