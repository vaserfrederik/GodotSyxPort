using settlement.entity.humanoid.ai.types.prisoner;
using static settlement.main.SETT;
using game.time;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.main;
using settlement.misc.util;
using settlement.room.law.prison;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class Prison : AIPLAN.PLANRES
    {
        private readonly ROOM_PRISON b = SETT.ROOMS().PRISON;

        public Prison() : base("prisPrison")
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            if (setCell(a, d))
            {
                if (PrisonerData.self.prisonReported.get(d) == 0)
                {
                    PrisonerData.self.prisonReported.set(d, 1);
                    PrisonerData.self.punish(a, d, CRIME_PUNISHMENTS.PRISON());
                }
                STATS.NEEDS().EXPOSURE.fix(a.indu());
                d.planByte1 = 8;
                AISubActivation s = init.set(a, d);
                if (s != null)
                    return s;
                cancel(a, d);
            }
            return null;
        }

        private bool setCell(Humanoid a, AIManager d)
        {
            COORDINATE c = b.registerPrisoner(AI.modules().coo(d), a.tc());
            if (c == null)
                return false;
            AI.modules().coo(d).set(c);
            return true;
        }

        private readonly Resumer init = new Resumer(CRIME_PUNISHMENTS.PRISON().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte1 <= 0)
                {
                    if (!SETT.PATH().connectivity.is(a.tc()))
                    {
                        return unfuck.set(a, d);
                    }
                    if (AIModule_Prisoner.DATA().prisonTimeLeft.get(d) == 0)
                    {
                        return free.set(a, d);
                    }

                    return null;
                }
                d.planByte1--;

                if (!b.isWithinCell(a.tc().x(), a.tc().y(), AI.modules().coo(d)))
                {
                    return walkToDoor.set(a, d);
                }

                if (NEEDS.TYPES().HUNGER.stat().getPrio(a.indu()) > 0)
                {
                    AISubActivation s = unfuck(eat, a, d);
                    if (s != null)
                        return s;
                }

                if (TIME.light().nightIs())
                {
                    AISubActivation s = sleep.set(a, d);
                    if (s != null)
                        return s;
                }

                // hunger, popo
                if (RND.oneIn(5))
                {
                    if (RND.oneIn(8))
                    {
                        AISubActivation s = unfuck(poop, a, d);
                        if (s != null)
                            return s;
                    }
                    else
                    {
                        AISubActivation s = changeSpot.set(a, d);
                        if (s != null)
                            return s;
                    }
                }

                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }

            private AISubActivation unfuck(Resumer res, Humanoid a, AIManager d)
            {
                if (!SETT.PATH().connectivity.is(a.tc()))
                {
                    return unfuck.set(a, d);
                }
                return res.set(a, d);
            }
        };

        private readonly Resumer walkToDoor = new Resumer(CRIME_PUNISHMENTS.PRISON().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().walkTo.cooFull(a, d, AI.modules().coo(d));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer unfuck = new Resumer(CRIME_PUNISHMENTS.PRISON().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer sleep = new Resumer(Dic.¤¤Sleep)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.sleep, 10);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer eat = new Resumer(CRIME_PUNISHMENTS.PRISON().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                FSERVICE c = b.getFood(AI.modules().coo(d));
                if (c != null)
                {
                    c.consume();
                }
                return eat2.set(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                FSERVICE c = b.getFood(AI.modules().coo(d));
                if (c != null)
                {
                    c.consume();
                }
                return eat2.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                FSERVICE c = b.getFood(AI.modules().coo(d));
                if (c != null)
                {
                    c.consume();
                }
            }
        };

        private readonly Resumer eat2 = new Resumer(CRIME_PUNISHMENTS.PRISON().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.grab, 3);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                STATS.FOOD().eat(a, 0, 0);
                NEEDS.TYPES().HUNGER.stat().fix(a.indu());
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer changeSpot = new Resumer(CRIME_PUNISHMENTS.PRISON().verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                DIR dir = DIR.ORTHO.rnd();
                int dx = a.tc().x() + dir.x();
                int dy = a.tc().y() + dir.y();
                if (b.isWithinCell(dx, dy, AI.modules().coo(d)))
                {
                    if (!SETT.ENTITIES().hasAtTile(dx, dy))
                        return AI.SUBS().walkTo.cooFull(a, d, dx, dy);
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        Resumer free = new Resumer(Dic.¤¤Free)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                HTYPE t = STATS.LAW().prisonerType.get(a.indu()).cl == HCLASSES.SLAVE() ? HTYPES.SLAVE() : HTYPES.SUBJECT();

                a.HTypeSet(t, null, CAUSE_ARRIVES.PAROLE());
                STATS.LAW().EX_CON.indu().setD(a.indu(), 1.0);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.ROOM_REMOVED && e.room.blueprintI() == b && e.room.is(AI.modules().coo(d)))
            {
                d.overwrite(a, AI.plans().NOP);
                AI.modules().coo(d).set(-1, -1);
                return true;
            }
            return false;
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            b.unregisterPrisoner(AI.modules().coo(d));
            AI.modules().coo(d).set(-1, -1);
            base.cancel(a, d);
        }

        protected override AISubActivation resume(Humanoid a, AIManager d)
        {
            AISubActivation s = base.resume(a, d);
            if (s == null)
            {
                b.unregisterPrisoner(AI.modules().coo(d));
                AI.modules().coo(d).set(-1, -1);
            }
            return s;
        }
    }
}