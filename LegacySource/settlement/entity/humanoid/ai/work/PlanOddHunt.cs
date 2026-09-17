using System;
using settlement.main;
using game.audio;
using game.faction;
using init.resources;
using settlement.entity;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.thing;
using snake2d.util.datatypes;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.work
{
    class PlanOddHunt
    {
        private static readonly CharSequence ¤¤verb = "Hunting";

        private readonly SoundRace sound = AUDIO.race("SLAUGHTER");

        static
        {
            D.ts(typeof(PlanOddHunt));
        }

        private readonly ResumerRaw stalk;
        private readonly ResumerRaw drag_back;
        private readonly ResumerRaw butcher;

        private double progress = 0;

        public AISubActivation init(Humanoid a, AIManager d)
        {
            Animal prey = SETT.PATH().finders.prey.findAndReserve(a.physics.tileC(), d.path, int.MaxValue);

            if (prey == null)
            {
                return null;
            }
            d.planObject = prey.id();
            return stalk.set(a, d);
        }

        protected PlanOddHunt(AIPLAN.PLANRES res)
        {
            stalk = new HResumer(res)
            {
                poll = (a, d, e) =>
                {
                    if (e.type == HPoll.SCARE_ANIMAL_NOT)
                        return 1;
                    return base.poll(a, d, e);
                },

                event = (a, d, e) =>
                {
                    if (e.event == HEvent.COLLISION_SOFT)
                    {
                        if (e.other is Animal)
                        {
                            Animal prey = getPrey(a, d);
                            Animal an = (Animal)e.other;
                            if (prey != an)
                            {
                                if (an.huntReservable())
                                {
                                    if (prey != null)
                                        prey.huntReserveCancel();
                                    prey = an;
                                    prey.huntReserve();
                                    d.planObject = prey.id();
                                }
                            }

                            if (prey == an)
                            {
                                a.speed.magnitudeInit(0);
                                AISubActivation s = drag_back.trySet(a, d);
                                if (s != null)
                                {
                                    d.overwrite(a, s);
                                    return false;
                                }
                            }
                        }
                    }
                    return base.event(a, d, e);
                },

                setAction = (a, d) =>
                {
                    AISubActivation ac = AI.SUBS().walkTo.path(a, d);
                    if (ac != null)
                        return ac;
                    can(a, d);
                    return null;
                },

                res = (a, d) =>
                {
                    Animal prey = getPrey(a, d);
                    int dx = prey.physics.tileC().x() - a.physics.tileC().x();
                    int dy = prey.physics.tileC().y() - a.physics.tileC().y();

                    if (Math.Abs(dx) + Math.Abs(dy) == 1)
                    {
                        return drag_back.set(a, d);
                    }
                    else
                    {
                        AISubActivation ac = AI.SUBS().walkTo.coo(a, d, prey.physics.tileC());
                        if (ac != null)
                            return ac;
                        can(a, d);
                        return null;
                    }
                },

                con = (a, d) => getPrey(a, d) != null,

                can = (a, d) =>
                {
                    Animal prey = getPrey(a, d);
                    if (prey != null)
                        prey.huntReserveCancel();
                }
            };

            drag_back = new HResumer(res)
            {
                con = (a, d) => getCadaver(a, d) != null,

                can = (a, d) => { },

                setAction = (a, d) =>
                {
                    Cadaver c = getPrey(a, d).slaugher();

                    if (c != null)
                    {
                        d.planObject = c.index();
                        Tuple<COORDINATE, RESOURCE> coo = SETT.PATH().finders.storage.reserve(a.tc().x(), a.tc().y(), c.spec().rBit, int.MaxValue);
                        if (coo == null)
                            return butcher.set(a, d);
                        d.planTile.set(coo.a());
                        SETT.PATH().finders.storage.cancelReservation(d.planTile, coo.b().bIndex());
                        AISubActivation ac = AI.SUBS().walkTo.drag(a, d, THINGS().cadavers.draggable, c.index(), d.planTile);
                        if (ac != null)
                            return ac;
                        return butcher.set(a, d);
                    }
                    can(a, d);
                    return null;
                },

                res = (a, d) => butcher.set(a, d)
            };

            butcher = new HResumer(res)
            {
                res = (a, d) =>
                {
                    Cadaver prey = getCadaver(a, d);

                    if (prey == null)
                    {
                        can(a, d);
                        return null;
                    }
                    a.speed.setDirCurrent(DIR.get(a.tc(), prey.ctx(), prey.cty()));
                    if (prey.resHas())
                    {
                        RESOURCE r = prey.resRemove();
                        THINGS().resources.create(prey.ctx(), prey.cty(), r, 1);
                        FACTIONS.player().res().inc(r, RTYPE.PRODUCED, 1);
                    }

                    if (prey.resHas())
                    {
                        sound.rnd(a);
                        return AI.SUBS().WORK_HANDS.activate(a, d, 5);
                    }
                    else
                    {
                        return null;
                    }
                },

                con = (a, d) => true,

                can = (a, d) => { },

                setAction = (a, d) => AI.SUBS().WORK_HANDS.activate(a, d, 12)
            };
        }

        private Animal getPrey(Humanoid a, AIManager d)
        {
            if (d.planObject == -1)
                return null;
            ENTITY e = ENTITIES().getByID(d.planObject);

            if (e == null || !(e is Animal) || !((Animal)e).huntReserved())
            {
                d.planObject = -1;
                return null;
            }
            return (Animal)e;
        }

        private Cadaver getCadaver(Humanoid a, AIManager d)
        {
            if (d.planObject == -1)
                return null;
            Cadaver e = THINGS().cadavers.getByIndex(d.planObject);

            if (e == null || e.isRemoved() || !e.resHas())
            {
                d.planObject = -1;
                return null;
            }
            return e;
        }

        private abstract class HResumer : ResumerRaw
        {
            public HResumer(PLANRES daddy) : base(daddy) { }

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.type == HPoll.SCARE_ANIMAL_NOT)
                    return 1;
                return base.poll(a, d, e);
            }

            protected override void name(Humanoid a, AIManager d, Str str)
            {
                str.add(¤¤verb);
            }
        }
    }
}