using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISTATES;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.entity.humanoid.spirte;
using settlement.main;
using settlement.room.law.guard;
using settlement.stats;
using snake2d.util.datatypes;
using util.text;

namespace settlement.entity.humanoid.ai.types.guard
{
    internal class PlanPatrol : AIPLAN.PLANRES
    {
        protected PlanPatrol() : base("GUARD_PATROL")
        {
        }

        private static readonly CharSequence ¤¤Reforming = "¤Patrolling";
        private readonly int cutDistance = 32 * C.TILE_SIZE;

        static PlanPatrol()
        {
            D.ts(typeof(PlanPatrol));
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            int pos = p().reservePosition();

            if (pos < 0)
                return null;

            d.planObject = pos;

            return retry2(a, d);
        }

        private AISubActivation retry2(Humanoid a, AIManager d)
        {
            if (STATS.WORK().WORK_TIME.indu().getD(a.indu()) >= 1)
                return null;

            if (STATS.BATTLE().DIV.get(a) == null)
                return null;

            COORDINATE c = coo(d);

            if (c == null)
                return null;

            if (!SETT.PATH().connectivity.is(c.x() / C.TILE_SIZE, c.y() / C.TILE_SIZE))
            {
                return null;
            }

            if (isInPosition(c, a, d))
            {
                return beBraced.set(a, d);
            }

            if (COORDINATE.tileDistance(c, a.body().cX(), a.body().cY()) < cutDistance)
            {
                return cutToPosition.set(a, d);
            }

            return pathToPosition.set(a, d);
        }

        private bool isInPosition(COORDINATE dest, Humanoid a, AIManager d)
        {
            return dest.isSameAs(a.physics.body().cX(), a.physics.body().cY());
        }

        private Patrols p()
        {
            return SETT.ROOMS().GUARD.patrols;
        }

        private Coo coo(AIManager d)
        {
            return p().pos(d.planObject);
        }

        private bool valid(AIManager d)
        {
            COORDINATE c = p().pos(d.planObject);
            if (c == null)
                return false;
            if (!SETT.PATH().connectivity.is(c.x() / C.TILE_SIZE, c.y() / C.TILE_SIZE))
            {
                return false;
            }
            return true;
        }

        private readonly Resumer cutToPosition = new Resumer(¤¤Reforming)
        {
            private readonly AISUB sub = new AISUB.Simple("GuardCutTo")
            {
                private readonly int distFar = (int)((C.TILE_SIZE + C.TILE_SIZEH) * (C.TILE_SIZE + C.TILE_SIZEH));
                private readonly double distFarI = 1.0 / distFar;
                private readonly int distClose = (int)(C.TILE_SIZEH * C.TILE_SIZEH / 2);
                private readonly double distCloseI = 1.0 / distClose;

                protected override AISTATE resume(Humanoid a, AIManager d)
                {
                    d.subByte++;
                    if (!valid(d))
                        return null;

                    COORDINATE dest = coo(d);

                    if (isInPosition(dest, a, d))
                    {
                        a.speed.magnitudeInit(0);
                        if (d.subByte == 1)
                            return AI.STATES().STAND.activate(a, d, 0.05);
                        return null;
                    }

                    double speed = Patrol.speed * C.TILE_SIZE;

                    int distX = dest.x() - a.physics.body().cX();
                    int distY = dest.y() - a.physics.body().cY();
                    double dist = distX * distX + distY * distY;

                    if (dist > distFar)
                        speed += a.speed.magintudeMax() * (dist - distFar) * distFarI;
                    else if (dist < distClose)
                    {
                        speed *= dist * distCloseI;
                    }

                    speed = CLAMP.d(speed, 0, a.speed.magintudeMax());

                    AISTATE s = AI.STATES().MOVE_TO.move(a, d, dest.x(), dest.y(), 0.05, speed);

                    return s;
                }
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                DIR dir = p().dir(d.planObject);
                if (dir.x() != 0 || dir.y() != 0)
                {
                    if (SETT.PATH().coster.player.getCost(a.tc().x(), a.tc().y(), a.tc().x() + dir.x(), a.tc().y() + dir.y()) < 0)
                        return pathToPosition.set(a, d);
                }
                return sub.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return retry2(a, d);
            }

            public override AISubActivation resFailed(Humanoid a, AIManager d, HEvent event)
            {
                if (event == HEvent.COLLISION_TILE)
                {
                    return pathToPosition.set(a, d);
                }
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

        private readonly Resumer pathToPosition = new Resumer(¤¤Reforming)
        {
            private readonly AISUB sub = new AISUB.Simple("GuardCut")
            {
                protected override AISTATE resume(Humanoid a, AIManager d)
                {
                    d.subByte++;
                    if (d.subByte == 1)
                        return AI.STATES().WALK2.path(a, d);
                    return null;
                }
            };

            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                Coo coo = coo(d);
                int tx = coo.x() / C.TILE_SIZE;
                int ty = coo.y() / C.TILE_SIZE;

                d.path.request(a.physics.tileC(), tx, ty);
                if (!d.path.isSuccessful())
                    return null;
                d.planByte2 = (byte)(8 + d.path.length() / 2);
                return sub.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (!d.path.isSuccessful())
                    return null;
                if (d.path.isDest() || d.planByte2-- <= 0)
                {
                    return retry2(a, d);
                }
                d.path.setNext();
                return sub.activate(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly STOP stand = new AISTATES.STOP("MP_GUARD_STAND", HSprites.SWORD_STAND_SWAY);

        private readonly Resumer beBraced = new Resumer(¤¤Reforming)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d)
            {
                d.subByte = 0;
                return AI.SUBS().single.activate(a, d, stand.activate(a, d, 0.5));
            }

            public override AISubActivation res(Humanoid a, AIManager d)
            {
                d.subByte++;
                if (d.subByte < 50 || !isInPosition(coo(d), a, d))
                {
                    return retry2(a, d);
                }
                a.speed.turn2(p().dir(d.planObject));
                return AI.SUBS().single.activate(a, d, stand.activate(a, d, 0.5));
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        protected override AISubActivation resume(Humanoid a, AIManager d)
        {
            AISubActivation s = base.resume(a, d);
            if (s == null)
                p().returnPosition(d.planObject);
            return s;
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            p().returnPosition(d.planObject);
            base.cancel(a, d);
        }

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
                return 1.0;
            return base.poll(a, d, e);
        }

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.NOTIFY_CRIME)
            {
                if (e.other is Humanoid)
                {
                    d.overwrite(a, AI.listeners().catchCriminal((Humanoid)e.other));
                    return true;
                }
            }
            return base.event(a, d, e);
        }
    }
}