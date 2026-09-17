using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.service.hygine.bath;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using System.Collections.Generic;

namespace settlement.entity.humanoid.ai.service
{
    internal class M_PlanBath : MPlan<ROOM_BATH>
    {
        public M_PlanBath() : base("Bath", SETT.ROOMS().BATHS, true)
        {
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.set(a, d);
        }

        private readonly AISUB sub = new AISUB.Simple("Bathing")
        {
            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                if (!a.speed.isZero())
                    a.speed.magnitudeInit(0);

                d.subByte++;

                if (d.subByte > 20)
                {
                    cancel(a, d);
                    return null;
                }

                if (d.subByte == 1)
                {
                    foreach (DIR dir in DIR.ORTHO)
                    {
                        int x = a.physics.tileC().x() + dir.x();
                        int y = a.physics.tileC().y() + dir.y();
                        if (ROOM_BATH.isPool(x, y))
                            return AI.STATES().WALK2.dirTile(a, d, dir);
                    }
                    d.debug(a, "No bath!");
                    return AI.STATES().STAND.aDirRND(a, d, 1 + RND.rFloat(2));
                }

                if (d.subByte > 1)
                {
                    STATS.POP().NAKED.set(a.indu(), 0);
                }

                return AI.STATES().STAND.aDirRND(a, d, 1 + RND.rFloat(2));
            }

            public override bool event(Humanoid a, AIManager ai, HEventData e)
            {
                if (e.event == HEvent.MEET_HARMLESS)
                {
                    if (a.speed.isZero())
                    {
                        DIR d = DIR.ORTHO.get(RND.rInt(4));
                        for (int i = 0; i < DIR.ORTHO.size(); i++)
                        {
                            int x = a.physics.tileC().x() + d.x();
                            int y = a.physics.tileC().y() + d.y();
                            if (ROOM_BATH.isPool(x, y) && !ENTITIES().hasAtTile(a, x, y))
                            {
                                ai.overwrite(a, AI.STATES().WALK2.tile(a, ai, x, y));
                            }
                            d = d.next(2);
                        }
                    }
                    return false;
                }
                return base.event(a, ai, e);
            }
        };

        private readonly Resumer first = new Resumer("1")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                STATS.POP().NAKED.set(a.indu(), 1);
                return sub.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                can(a, d);
                STATS.NEEDS().EXPOSURE.fix(a.indu());
                STATS.NEEDS().DIRTINESS.set(a.indu(), 0);
                return walk2Bench.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                FINDABLE s = blue(d).bath(d.planTile.x(), d.planTile.y());
                return s != null && s.findableReservedIs();
            }

            public override void can(Humanoid a, AIManager d)
            {
                Bath s = blue(d).bath(d.planTile.x(), d.planTile.y());
                if (s != null && s.findableReservedIs())
                {
                    s.consume();
                }
                STATS.POP().NAKED.set(a.indu(), 0);
            }
        };

        private readonly Resumer walk2Bench = new Resumer("2")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                BathInstance b = blue(d).get(a.physics.tileC().x(), a.physics.tileC().y());
                if (b != null)
                {
                    COORDINATE c = b.getBench();
                    if (c != null)
                    {
                        STATS.POP().NAKED.set(a.indu(), 1);
                        return trySub(a, d, AI.SUBS().walkTo.cooFull(a, d, c), null);
                    }
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return relax.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return blue(d).isBench(d.path.destX(), d.path.destY());
            }

            public override void can(Humanoid a, AIManager d)
            {
                BathInstance b = blue(d).get(a.physics.tileC().x(), a.physics.tileC().y());
                if (b != null)
                {
                    b.returnBench(d.path.destX(), d.path.destY());
                }

                STATS.POP().NAKED.set(a.indu(), 0);
            }
        };

        private readonly Resumer relax = new Resumer("3")
        {
            private readonly AISUB sub = new AISUB.Simple("Relaxing")
            {
                protected override AISTATE resume(Humanoid a, AIManager d)
                {
                    d.subByte++;
                    if (d.subByte == 1)
                        return AI.STATES().anima.layoff.activate(a, d, 10 + RND.rInt(20));
                    return null;
                }
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                DIR dir = blue(d).getBenchDir(a.physics.tileC().x(), a.physics.tileC().y());
                a.speed.setDirCurrent(dir.perpendicular());
                return sub.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                can(a, d);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return blue(d).isBench(d.path.destX(), d.path.destY());
            }

            public override void can(Humanoid a, AIManager d)
            {
                BathInstance b = blue(d).get(a.physics.tileC().x(), a.physics.tileC().y());
                if (b != null)
                {
                    b.returnBench(d.path.destX(), d.path.destY());
                }
                STATS.POP().NAKED.set(a.indu(), 0);
            }
        };
    }
}