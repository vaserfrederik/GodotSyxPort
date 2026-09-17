using System;
using settlement.entity.humanoid.ai.main;
using game.audio;
using init.constant;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main.AISTATES;
using snake2d.util.datatypes;
using snake2d.util.rnd;

public static class AISUBS
{
    public static Stand STAND = new Stand();
    public static Lay LAY = new Lay("subsLAy");
    public static AISUB_walkTo walkTo = new AISUB_walkTo();
    public static Work WORK = new Work();
    public static WorkHands WORK_HANDS = new WorkHands();
    public static AISUB failure = new AISUB.Simple("rethinking")
    {
        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            if (!a.speed.isZero())
                return AI.STATES().STOP.activate(a, d);
            d.subByte++;
            if (d.subByte == 1)
                return AI.STATES().STAND.activate(a, d, 0.5f + RND.rFloat());
            return null;
        }

        protected override bool isSuccessful(Humanoid a, AIManager d)
        {
            return false;
        }
    };
    public static Single single = new Single("subsSingle");

    public static AISUB.Simple DUMMY = new AISUB.Simple("DUMMY")
    {
        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            // TODO Auto-generated method stub
            return null;
        }
    };

    public static class Work
    {
        private static AISUB.Simple sub = new AISUB.Simple("working")
        {
            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                if (!a.speed.isZero())
                    return AI.STATES().STOP.activate(a, d);
                if (d.subByte == 1)
                {
                    d.subByte++;
                    return AI.STATES().WORK.state;
                }
                if (d.subByte == 2)
                {
                    d.subByte++;
                    return AI.STATES().STAND.activate(a, d, 0.2f);
                }
                return null;
            }

            protected override AISTATE resumeInterrupted(Humanoid a, AIManager d, HEvent event)
            {
                if (d.subByte >= 2)
                    return AI.STATES().STAND.activate(a, d, 0.2f);
                return null;
            }
        };

        public static AISubActivation Activate(Humanoid a, AIManager d, double time)
        {
            AISubActivation k = sub.activate(a, d, AI.STATES().WORK.activate(a, d, time));
            if (time > 0)
            {
                d.subByte = 1;
            }
            else
                d.subByte = 2;
            return k;
        }

        public static AISubActivation Activate(Humanoid a, AIManager d, Animation animation, double time)
        {
            AISubActivation k = sub.activate(a, d);
            d.subByte = 1;
            animation.activate(a, d, time);
            return k;
        }
    }

    public abstract class Throw : AISUB.Simple
    {
        public Throw(string key) : base(key) { }

        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            if (!a.speed.isZero())
                return AI.STATES().STOP.activate(a, d);
            d.subByte++;
            if (d.subByte == 1)
            {
                return AI.STATES().anima.throww.activate(a, d);
            }
            if (d.subByte == 2)
            {
                DIR dd = a.speed.dir();
                int sx = a.body().cX();
                int sy = a.body().cY();
                dd = dd.next(1);
                sx += dd.x() * C.TILE_SIZEH / 2;
                sy += dd.y() * C.TILE_SIZEH / 2;
                THINGS.THINGS.get().tiles().things().get(sx, sy).add(new THING(a));
                return AI.STATES().STAND.activate(a, d, 0.2f);
            }
            return null;
        }

        public abstract int destX(Humanoid a, AIManager d);
        public abstract int destY(Humanoid a, AIManager d);
    }

    public static Single single = new Single("subsSingle");

    public static AISUB.Simple DUMMY = new AISUB.Simple("DUMMY")
    {
        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            // TODO Auto-generated method stub
            return null;
        }
    };

    public static class Work
    {
        private static AISUB.Simple sub = new AISUB.Simple("working")
        {
            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                if (!a.speed.isZero())
                    return AI.STATES().STOP.activate(a, d);
                if (d.subByte == 1)
                {
                    d.subByte++;
                    return AI.STATES().WORK.state;
                }
                if (d.subByte == 2)
                {
                    d.subByte++;
                    return AI.STATES().STAND.activate(a, d, 0.2f);
                }
                return null;
            }

            protected override AISTATE resumeInterrupted(Humanoid a, AIManager d, HEvent event)
            {
                if (d.subByte >= 2)
                    return AI.STATES().STAND.activate(a, d, 0.2f);
                return null;
            }
        };

        public static AISubActivation Activate(Humanoid a, AIManager d, double time)
        {
            AISubActivation k = sub.activate(a, d, AI.STATES().WORK.activate(a, d, time));
            if (time > 0)
            {
                d.subByte = 1;
            }
            else
                d.subByte = 2;
            return k;
        }

        public static AISubActivation Activate(Humanoid a, AIManager d, Animation animation, double time)
        {
            AISubActivation k = sub.activate(a, d);
            d.subByte = 1;
            animation.activate(a, d, time);
            return k;
        }
    }

    public abstract class Throw : AISUB.Simple
    {
        public Throw(string key) : base(key) { }

        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            if (!a.speed.isZero())
                return AI.STATES().STOP.activate(a, d);
            d.subByte++;
            if (d.subByte == 1)
            {
                return AI.STATES().anima.throww.activate(a, d);
            }
            if (d.subByte == 2)
            {
                DIR dd = a.speed.dir();
                int sx = a.body().cX();
                int sy = a.body().cY();
                dd = dd.next(1);
                sx += dd.x() * C.TILE_SIZEH / 2;
                sy += dd.y() * C.TILE_SIZEH / 2;
                THINGS.THINGS.get().tiles().things().get(sx, sy).add(new THING(a));
                return AI.STATES().STAND.activate(a, d, 0.2f);
            }
            return null;
        }

        public abstract int destX(Humanoid a, AIManager d);
        public abstract int destY(Humanoid a, AIManager d);
    }

    public static Single single = new Single("subsSingle");

    public static AISUB.Simple DUMMY = new AISUB.Simple("DUMMY")
    {
        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            // TODO Auto-generated method stub
            return null;
        }
    };

    public static class Work
    {
        private static AISUB.Simple sub = new AISUB.Simple("working")
        {
            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                if (!a.speed.isZero())
                    return AI.STATES().STOP.activate(a, d);
                if (d.subByte == 1)
                {
                    d.subByte++;
                    return AI.STATES().WORK.state;
                }
                if (d.subByte == 2)
                {
                    d.subByte++;
                    return AI.STATES().STAND.activate(a, d, 0.2f);
                }
                return null;
            }

            protected override AISTATE resumeInterrupted(Humanoid a, AIManager d, HEvent event)
            {
                if (d.subByte >= 2)
                    return AI.STATES().STAND.activate(a, d, 0.2f);
                return null;
            }
        };

        public static AISubActivation Activate(Humanoid a, AIManager d, double time)
        {
            AISubActivation k = sub.activate(a, d, AI.STATES().WORK.activate(a, d, time));
            if (time > 0)
            {
                d.subByte = 1;
            }
            else
                d.subByte = 2;
            return k;
        }

        public static AISubActivation Activate(Humanoid a, AIManager d, Animation animation, double time)
        {
            AISubActivation k = sub.activate(a, d);
            d.subByte = 1;
            animation.activate(a, d, time);
            return k;
        }
    }

    public abstract class Throw : AISUB.Simple
    {
        public Throw(string key) : base(key) { }

        protected override AISTATE resume(Humanoid a, AIManager d)
        {
            if (!a.speed.isZero())
                return AI.STATES().STOP.activate(a, d);
            d.subByte++;
            if (d.subByte == 1)
            {
                return AI.STATES().anima.throww.activate(a, d);
            }
            if (d.subByte == 2)
            {
                DIR dd = a.speed.dir();
                int sx = a.body().cX();
                int sy = a.body().cY();
                dd = dd.next(1);
                sx += dd.x() * C.TILE_SIZEH / 2;
                sy += dd.y() * C.TILE_SIZEH / 2;
                THINGS.THINGS.get().tiles().things().get(sx, sy).add(new THING(a));
                return AI.STATES().STAND.activate(a, d, 0.2f);
            }
            return null;
        }

        public abstract int destX(Humanoid a, AIManager d);
        public abstract int destY(Humanoid a, AIManager d);
    }
}