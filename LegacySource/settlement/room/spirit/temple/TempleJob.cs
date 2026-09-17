using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.audio;
using game.faction;
using init.resources;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main.util;
using snake2d.util.datatypes;

namespace settlement.room.spirit.temple
{
    public abstract class TempleJob
    {
        protected readonly ROOM_TEMPLE blue;
        protected TempleInstance ins;
        protected readonly Coo coo = new Coo();
        protected TempleAltar altar;

        private TempleJob(ROOM_TEMPLE blue)
        {
            this.blue = blue;
        }

        public TempleJob Get(int tx, int ty)
        {
            ins = blue.Get(tx, ty);
            if (ins != null)
            {
                if (SETT.ROOMS().fData.tile.Is(tx, ty, blue.constructor.ap))
                {
                    coo.Set(tx, ty);
                    for (int di = 0; di < DIR.ORTHO.size; di++)
                    {
                        int dx = tx + DIR.ORTHO.Get(di).X();
                        int dy = ty + DIR.ORTHO.Get(di).Y();
                        if (ins.Is(dx, dy) && blue.altar.Get(dx, dy) != null)
                            altar = blue.altar.Get(dx, dy);
                    }

                    return this;
                }
            }
            return null;
        }

        public abstract void JobReserve();

        public abstract bool JobReservedIs();
        public abstract void JobReserveCancel();

        public abstract RBIT JobResourceBitToFetch();

        public abstract void JobStartPerforming();

        public abstract void JobPerform(Humanoid skill, int r);

        public SoundRace JobSound()
        {
            return blue.employment().Sound();
        }

        public COORDINATE Coo()
        {
            return coo;
        }

        public COORDINATE FaceCoo()
        {
            return altar.Coo();
        }

        public CharSequence JobName()
        {
            return blue.employment().Verb;
        }

        public bool ShouldKill()
        {
            return false;
        }

        public void Kill()
        {

        }

        public void ReportMissingResource()
        {
            ins.resHas = false;
        }

        public sealed class Resources : TempleJob
        {
            private readonly RoomBits reserved = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_0000_0001);
            private readonly RESOURCE res;

            public Resources(ROOM_TEMPLE blue, RESOURCE resources) : base(blue)
            {
                this.res = resources;
            }

            public override void JobReserve()
            {
                reserved.Set(ins, 1);
            }

            public override bool JobReservedIs()
            {
                return reserved.Get() == 1;
            }

            public override void JobReserveCancel()
            {
                reserved.Set(ins, 0);
            }

            public override RBIT JobResourceBitToFetch()
            {
                if (altar.ResourceNeeds())
                {
                    return res.bit;
                }
                return null;
            }

            public override void JobStartPerforming()
            {
            }

            public override void JobPerform(Humanoid skill, int res)
            {
                JobReserveCancel();
                if (res > 0)
                {
                    altar.ResourceInc(res);
                    FACTIONS.player().res().Inc(this.res, RTYPE.CONSUMED, -res);
                }
            }

            public override bool ShouldKill()
            {
                return altar.ShouldKill();
            }

            public override void Kill()
            {
                altar.Kill();
            }

            public override SoundRace JobSound()
            {
                return blue.employment().Sound();
            }
        }

        public sealed class None : TempleJob
        {
            private readonly RoomBits reserved = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_0000_0001);

            public None(ROOM_TEMPLE blue) : base(blue)
            {
            }

            public override void JobReserve()
            {
                reserved.Set(ins, 1);
            }

            public override bool JobReservedIs()
            {
                return reserved.Get() == 1;
            }

            public override void JobReserveCancel()
            {
                reserved.Set(ins, 0);
            }

            public override RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public override void JobStartPerforming()
            {
            }

            public override void JobPerform(Humanoid skill, int res)
            {
                JobReserveCancel();
            }

            public override bool ShouldKill()
            {
                return altar.ShouldKill();
            }

            public override void Kill()
            {
                altar.Kill();
            }

            public override SoundRace JobSound()
            {
                return blue.employment().Sound();
            }
        }
    }
}