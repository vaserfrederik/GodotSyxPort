using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.law.court;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class Judged : AIPLAN.PLANRES
    {
        private static string ¤¤verb = "Pleading case in court";

        static Judged()
        {
            D.ts(typeof(Judged));
        }

        public Judged() : base("prisJudged")
        {
            // TODO Auto-generated constructor stub
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            CourtStation s = SETT.ROOMS().COURT.exectuionReserve();

            if (s == null)
                return null;
            d.planTile.set(s.cooCriminal());
            return walk.set(a, d);
        }

        private readonly Resumer walk = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                AISubActivation s = AI.SUBS().walkTo.cooFull(a, d, d.planTile);
                if (s != null)
                {
                    return s;
                }
                cancel(a, d);
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return ready.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer ready = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                CourtStation s = SETT.ROOMS().COURT.executionSpot(d.planTile);
                s.criminalUse();
                d.planByte1 = (byte)TIME.hours().bitCurrent();
                d.planByte2 = (byte)(TIME.days().bitCurrent() & 0x0F);
                a.speed.setDirCurrent(s.criminalDir());
                d.planByte3 = 8;
                return res(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                CourtStation s = SETT.ROOMS().COURT.executionSpot(d.planTile);
                if (s.criminalIsBeeingHeard())
                {
                    if (d.planByte3-- <= 0)
                    {
                        PrisonerData.self.judged.set(d, 1);
                        cancel(a, d);

                        if (RND.rFloat() < ROOM_COURT.freeRate)
                        {
                            return freed.set(a, d);
                        }
                        return null;
                    }

                    if (RND.oneIn(4))
                    {
                        a.speed.setDirCurrent(s.criminalDir().next(RND.rInt0(2)));
                        return AI.SUBS().STAND.activateTime(a, d, 1 + RND.rInt(5));
                    }
                    else
                    {
                        return AI.SUBS().single.activate(a, d, RND.rBoolean() ? AI.STATES().anima.box : AI.STATES().anima.wave, 1 + RND.rFloat() * 4);
                    }
                }
                else
                {
                    if (Math.Abs(d.planByte2 - (TIME.days().bitCurrent() & 0x0F)) > 1)
                    {
                        if (s != null)
                            s.criminalClear();
                        return null;
                    }
                    a.speed.setDirCurrent(a.speed.dir().next(RND.rInt0(2)));
                    return AI.SUBS().STAND.activateTime(a, d, 1 + RND.rInt(5));
                }
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer freed = ResFree.make(this);

        protected override void cancel(Humanoid a, AIManager d)
        {
            CourtStation s = SETT.ROOMS().COURT.executionSpot(d.planTile);
            if (s != null)
                s.criminalClear();
            base.cancel(a, d);
        }

        protected override bool shouldContinue(Humanoid a, AIManager d)
        {
            if (getResumer(d) == freed)
                return true;
            CourtStation s = SETT.ROOMS().COURT.executionSpot(d.planTile);
            if (s != null && s.criminalReserved())
                return base.shouldContinue(a, d);
            return false;
        }
    }
}