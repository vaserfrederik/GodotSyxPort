using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.util;
using settlement.main;
using settlement.room.main;
using settlement.room.spirit.temple;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.work
{
    internal sealed class WorkTemple : PlanBlueprint
    {
        private readonly ROOM_TEMPLE temple;

        public WorkTemple(AIModule_Work module, ROOM_TEMPLE blueprint, PlanBlueprint[] map)
            : base(module, blueprint, map)
        {
            this.temple = blueprint;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            if (RND.OneIn(8))
                return walkAround.Set(a, d);

            TempleInstance ins = (TempleInstance)work(a);
            TempleJob j = ins.jobReservable(a.Tc().X, a.Tc().Y);
            if (j == null)
            {
                return walkAround.Set(a, d);
            }

            d.planTile = j.coo;
            d.planByte1 = -1;
            if (j.jobResourceBitToFetch() != null)
            {
                AISubActivation s = fetch.Activate(a, d, j.jobResourceBitToFetch(), maxCarry, 1000, true, true);
                if (s != null)
                {
                    j = ins.job(d.planTile.X, d.planTile.Y);
                    j.jobReserve();
                    return s;
                }
                j = ins.job(d.planTile.X, d.planTile.Y);
                j.reportMissingResource();
            }

            AISubActivation s = walkToJob.Set(a, d);
            if (s != null)
            {
                j = ins.job(d.planTile.X, d.planTile.Y);
                j.jobReserve();
                return s;
            }

            return walkAround.Set(a, d);
        }

        private readonly AIPlanResourceMany fetch = new AIPlanResourceMany(this, 32)
        {
            Next = (a, d) => walkToJob.Set(a, d),
            Cancel = (a, d) => unreserve(a, d)
        };

        private bool reserved(Humanoid a, AIManager d)
        {
            if (work(a) != null)
            {
                TempleInstance ins = (TempleInstance)work(a);
                TempleJob j = ins.job(d.planTile.X, d.planTile.Y);
                return j != null && j.jobReservedIs();
            }
            return false;
        }

        private void unreserve(Humanoid a, AIManager d)
        {
            Room r = SETT.ROOMS().map.Get(d.planTile);
            if (r != null && r is TempleInstance)
            {
                TempleInstance ins = (TempleInstance)r;
                TempleJob j = ins.job(d.planTile.X, d.planTile.Y);
                if (j != null)
                    j.jobReserveCancel();
            }
        }

        private readonly Res walkToJob = new Res
        {
            setAction = (a, d) => AI.SUBS().walkTo.cooFull(a, d, d.planTile),
            res = (a, d) =>
            {
                TempleInstance ins = (TempleInstance)work(a);
                TempleJob j = ins.job(d.planTile.X, d.planTile.Y);
                a.speed.setDirCurrent(DIR.get(a.Tc(), j.faceCoo()));
                return work.Set(a, d);
            },
            con = (a, d) => reserved(a, d),
            can = (a, d) => unreserve(a, d)
        };

        private readonly Res work = new Res
        {
            setAction = (a, d) =>
            {
                TempleInstance ins = (TempleInstance)work(a);
                TempleJob j = ins.job(d.planTile.X, d.planTile.Y);

                if (d.resourceCarried() != null)
                {
                    j.jobPerform(a, d.resourceA());
                    d.resourceCarriedSet(null);
                    return null;
                }
                else if (j.shouldKill())
                {
                    return sacrifice.Set(a, d);
                }
                else
                {
                    d.planByte1 = 10;
                    return res(a, d);
                }
            },
            res = (a, d) =>
            {
                d.planByte1--;
                if (d.planByte1 <= 0)
                {
                    unreserve(a, d);
                    return null;
                }
                if (RND.OneIn(5))
                    temple.employment().sound().rnd(a);
                a.speed.setDirCurrent(a.speed.dir().next(RND.rInt0(1)));
                return AI.SUBS().single.activate(a, d, preach[RND.rInt(preach.Length)], 2 + RND.rFloat(4));
            },
            con = (a, d) => reserved(a, d),
            can = (a, d) => unreserve(a, d)
        };

        private readonly Res sacrifice = new Res
        {
            setAction = (a, d) =>
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.stab, 10);
            },
            res = (a, d) =>
            {
                TempleInstance ins = (TempleInstance)work(a);
                TempleJob j = ins.job(d.planTile.X, d.planTile.Y);

                if (j.shouldKill())
                {
                    j.kill();

                    if (j.shouldKill())
                    {
                        return setAction(a, d);
                    }
                    temple.employment().sound().rnd(a);
                    return AI.SUBS().single.activate(a, d, AI.STATES().anima.armsOut, 4);
                }

                unreserve(a, d);
                return null;
            },
            con = (a, d) => reserved(a, d),
            can = (a, d) => unreserve(a, d)
        };

        private readonly Res walkAround = new Res
        {
            setAction = (a, d) => AI.SUBS().walkTo.room(a, d, work(a)),
            res = (a, d) => standAround.Set(a, d),
            con = (a, d) => true,
            can = (a, d) => { }
        };

        private readonly Res standAround = new Res
        {
            setAction = (a, d) =>
            {
                d.planByte1 = 10;
                return res(a, d);
            },
            res = (a, d) =>
            {
                d.planByte1--;
                if (d.planByte1 <= 0)
                {
                    return null;
                }
                a.speed.setDirCurrent(a.speed.dir().next(RND.rInt0(1)));
                if (RND.OneIn(5))
                    temple.employment().sound().rnd(a);
                return AI.SUBS().single.activate(a, d, preach[RND.rInt(preach.Length)], 2 + RND.rFloat(4));
            },
            con = (a, d) => work(a) != null,
            can = (a, d) => { }
        };

        private readonly Animation[] preach = new Animation[]
        {
            AI.STATES().anima.carry,
            AI.STATES().anima.armsOut,
            AI.STATES().anima.fist,
            AI.STATES().anima.wave,
            AI.STATES().anima.stand,
            AI.STATES().anima.stand,
            AI.STATES().anima.stand,
        };

        private abstract class Res : Resumer
        {
            protected Res()
                : base("")
            { }

            protected override void name(Humanoid a, AIManager d, Str str)
            {
                str.add(temple.employment().verb);
            }
        }
    }
}