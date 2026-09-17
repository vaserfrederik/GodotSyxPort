using System;
using game.audio;
using game.faction;
using init.constant;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.main.util;
using snake2d;
using snake2d.util.bit;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.rendering;
using view.sett;

namespace settlement.room.food.orchard
{
    final class OTile
    {
        private Instance ins;
        private Coo coo = new Coo();
        private Coo mCoo = new Coo();
        private readonly ROOM_ORCHARD b;

        public const int WORK_TIME = 45;
        public const int INOTHING = 0;
        public readonly STATE ISAPLING;
        public readonly STATE ISMALL;
        public readonly STATE IBIG;
        public readonly STATE IDEAD;
        private readonly STATE[] states;
        private readonly RoomBits bState = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0111))
        {
            public override void Set(int tx, int ty, ROOMA r, int t)
            {
                if (Get() == IBIG.index)
                {
                    ins.trees--;
                }
                base.Set(tx, ty, r, t);
                if (t == IBIG.index)
                {
                    ins.trees++;
                }
            }
        };
        private readonly RoomBits bHarvested = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0100));
        private readonly RoomBits bReserved = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0010));
        private readonly RoomBits bRan = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001));
        private readonly RoomBits bWorkedDay = new RoomBits(coo, new Bits(0b0000_0000_0000_0000_0000_0000_0011_1111));

        public OTile(ROOM_ORCHARD b)
        {
            this.b = b;
        }

        private void setState(STATE state)
        {
            int ox = coo.x();
            int oy = coo.y();

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    int tx = mCoo.x() + x;
                    int ty = mCoo.y() + y;
                    if (Get(tx, ty) == null)
                    {

                    }
                    else
                    {
                        Get(tx, ty);
                        bState.Set(ins, state.index);
                    }
                }
            }
            Get(ox, oy);
            return;
        }

        public STATE State()
        {
            return states[bState.Get()];
        }

        public OTile Get(int tx, int ty)
        {
            ins = b.Get(tx, ty);
            if (ins == null)
                return null;
            if (SETT.ROOMS().fData.tileData.Get(tx, ty) != Constructor.TREE)
                return null;
            coo.Set(tx, ty);
            if (bState.Get() == INOTHING)
                return null;
            DIR d = dirs[bdir.Get()];
            mCoo.Set(tx + d.x(), ty + d.y());
            return this;
        }

        public OTile GetM(int tx, int ty)
        {
            if (Get(tx, ty) != null && bdir.Get() == 0)
                return this;
            return null;
        }

        public bool DestroyTileCan()
        {
            return bState.Get() > ISAPLING.index;
        }

        public void DestroyTile()
        {
            setState(ISAPLING);
        }

        public void RenderDebug(SPRITE_RENDERER r, RenderIterator it)
        {
            it.SetOff(0, 0);
            if (bReserved.Get() == 0)
            {
                COLOR.BLUE100.Render(r, it.x(), it.y());
            }
            else
            {
                COLOR.GREEN100.Render(r, it.x(), it.y());
            }

            if (bWorkedDay.Get() == ((b.time.DayI()) & bWorkedDay.Max()))
            {
                COLOR.YELLOW100.Render(r, it.x() + C.TILE_SIZEH, it.y());
            }
        }

        public void UpdateDay()
        {
            if (Bits.GetDistance(bWorkedDay.Get(), b.time.DayI(), bWorkedDay.Max()) > 2)
            {
                State().Fail();
                bWorkedDay.Set(ins, (b.time.DayI() - 1) & bWorkedDay.Max());
                bReserved.Set(ins, 0);
            }
            State().Update();
        }

        public SETT_JOB Job()
        {
            return job;
        }

        private readonly SETT_JOB job = new SETT_JOB()
        {
            public override bool JobUseTool()
            {
                return State() == IDEAD;
            }

            public override void JobStartPerforming()
            {
                // TODO Auto-generated method stub
            }

            public override SoundRace JobSound()
            {
                return State() == IDEAD ? SETT.TERRAIN().TREES.SMALL.clearing().sound(coo.x(), coo.y()) : ins.blueprintI().employment().sound();
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override bool JobReservedIs(RESOURCE r)
            {
                return bReserved.Get() == 1;
            }

            public override void JobReserveCancel(RESOURCE r)
            {
                bReserved.Set(ins, 0);
            }

            public override bool JobReserveCanBe()
            {
                return bReserved.Get() == 0 && bWorkedDay.Get() != ((b.time.DayI()) & bWorkedDay.Max());
            }

            public override void JobReserve(RESOURCE r)
            {
                bReserved.Set(ins, 1);
            }

            public override double JobPerformTime(Humanoid a)
            {
                return WORK_TIME;
            }

            public override CharSequence JobName()
            {
                return b.employment().verb;
            }

            public override COORDINATE JobCoo()
            {
                return coo;
            }

            public override RESOURCE JobPerform(Humanoid skill, RESOURCE r, int rAm)
            {
                bWorkedDay.Set(ins, (b.time.DayI()) & bWorkedDay.Max());
                bReserved.Set(ins, 0);
                double s = IndustryUtil.CalcProductionRate(1, skill, ins.industry(), ins);
                ins.incSkill(s);
                int am = (int)s;
                if (s - am > RND.rFloat())
                    am++;
                State().Work(skill, am);
                return null;
            }
        };

        public static class STATE
        {
            public readonly int index;

            public STATE(int i)
            {
                this.index = i;
            }

            public virtual void Work(Humanoid a, int skill)
            {

            }

            public virtual void Fail()
            {

            }

            public virtual double DeadAmount()
            {
                return 0;
            }

            public virtual double FruitAmount()
            {
                return 0;
            }

            public virtual int DaysTillGrown()
            {
                return int.MaxValue;
            }

            public virtual void Update()
            {

            }
        }
    }
}