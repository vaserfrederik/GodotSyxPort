using settlement.entity.humanoid.ai.subject;
using static settlement.main.SETT;
using init.constant;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUBS;
using settlement.main;
using settlement.misc.util;
using settlement.path;
using settlement.room.main;
using settlement.room.service.module;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.subject
{
    class Activity : AIPLAN.PLANRES
    {
        public readonly ROOM_ACTIVITY s;
        private readonly CharSequence verb;
        private readonly int radius;

        public Activity(ROOM_ACTIVITY_HASER t, int radius, CharSequence verb) : base("ACTIVITY_" + ((RoomBlueprint)t).key)
        {
            this.s = t.spec();
            this.verb = verb;
            this.radius = radius;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return walk.set(a, d);
        }

        private readonly R walk = new R()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                COORDINATE c = s.finder().reserve(a.tc(), radius);
                if (c == null)
                    return null;
                d.planTile.set(c);
                c = s.getDestination(c);
                AISubActivation sub = AI.SUBS().walkTo.cooFull(a, d, c);
                if (sub == null)
                {
                    can(a, d);
                    return null;
                }
                return sub;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                d.planByte1 = (byte)(8 + RND.rInt(8));
                return move.set(a, d);
            }
        };

        private readonly R move = new R()
        {
            private readonly int[] order = new int[] { 0, 1, 2 };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (!shouldMove(a, a.tc().x(), a.tc().y()))
                    return stand.set(a, d);

                DIR dd = DIR.get(a.body().cX(), a.body().cY(), s.lookAt(d.planTile.x(), d.planTile.y()));

                foreach (int i in order)
                {
                    dd = dd.next(i);
                    int dx = a.tc().x() + dd.x();
                    int dy = a.tc().y() + dd.y();
                    if (isSpot(dx, dy))
                    {
                        if (!shouldMove(a, dx, dy))
                        {
                            return AI.SUBS().walkTo.cooFull(a, d, dx, dy);
                        }
                    }
                }

                return stand.set(a, d);
            }

            private bool shouldMove(Humanoid a, int cx, int cy)
            {
                foreach (ENTITY e in SETT.ENTITIES().getAtTile(cx, cy))
                {
                    if (e != a && e is Humanoid && e.speed.magnitude() == 0)
                        return true;
                }
                return false;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return stand.set(a, d);
            }
        };

        private readonly R stand = new R()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (!s.is(d.planTile.x(), d.planTile.y()))
                    return null;

                if (d.planByte1 <= 0)
                {
                    if (!AIModules.current(d).moduleCanContinue(a, d) || RND.oneIn(5))
                    {
                        can(a, d);
                        return null;
                    }

                }
                else
                {
                    FINDABLE f = s.finder().getReserved(d.planTile.x(), d.planTile.y());
                    if (f == null)
                    {
                        f = s.finder().getReservable(d.planTile.x(), d.planTile.y());
                        if (f == null)
                        {
                            d.planByte1 -= 3;
                        }
                        else
                            f.findableReserve();
                    }

                    d.planByte1--;
                }

                DIR dd = DIR.get(a.body().cX(), a.body().cY(), s.lookAt(d.planTile.x(), d.planTile.y()));
                if (RND.oneIn(5))
                    dd = dd.next(RND.rInt0(1));
                a.speed.setDirCurrent(dd);

                if (s.shouldCheer(d.planTile.x(), d.planTile.y()))
                {
                    return cheer.set(a, d);
                }

                if (s.shouldBoo(d.planTile.x(), d.planTile.y()))
                {
                    return boo.set(a, d);
                }

                return AI.SUBS().STAND.activateTime(a, d, 10);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return move.set(a, d);
            }
        };

        private readonly R cheer = new R()
        {
            private Animation[] anima = new Animation[] {
                AI.STATES().anima.wave,
                AI.STATES().anima.box,
                AI.STATES().anima.lay,
                AI.STATES().anima.stand,
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, anima[RND.rInt(anima.Length)], 2 + RND.rInt(4));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return stand.set(a, d);
            }
        };

        private readonly R boo = new R()
        {
            private readonly AISUB ss = new AISUBS.Throw(key + "specThrow")
            {
                public override int destY(Humanoid a, AIManager d)
                {
                    if (s != null)
                        return s.lookAt(d.planTile.x(), d.planTile.y()).y();
                    return d.planTile.y() * C.TILE_SIZE + C.TILE_SIZEH;
                }

                public override int destX(Humanoid a, AIManager d)
                {
                    if (s != null)
                        return s.lookAt(d.planTile.x(), d.planTile.y()).x();
                    return d.planTile.x() * C.TILE_SIZE + C.TILE_SIZEH;
                }
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (RND.oneIn(5))
                    return ss.activate(a, d);
                if (RND.oneIn(2))
                    return AI.SUBS().single.activate(a, d, AI.STATES().anima.fist, 4);
                return AI.SUBS().STAND.activateTime(a, d, 4);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return stand.set(a, d);
            }
        };

        private bool isSpot(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return false;
            if (SETT.ROOMS().map.is(tx, ty))
                return false;
            AVAILABILITY av = PATH().availability.get(tx, ty);
            if (av.player >= 0 && av.player < AVAILABILITY.Penalty && av.from == 0)
            {
                return true;
            }
            return false;
        }

        private abstract class R : Resumer
        {
            protected R() : base("")
            {
            }

            protected override void name(Humanoid a, AIManager d, Str string)
            {
                if (s != null)
                    string.add(verb);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                FINDABLE f = s.finder().getReserved(d.planTile.x(), d.planTile.y());
                if (f != null)
                    f.findableReserveCancel();
            }
        }
    }
}