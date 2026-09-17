using System;
using System.Collections.Generic;
using game.audio;
using game.time;
using init.resources;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.misc.job;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.stats;
using snake2d;
using snake2d.util.bit;
using snake2d.util.datatypes;
using util;

public class PoliceWork
{
    private readonly ROOM_POLICE b;
    private PoliceInstance ins;
    private readonly Coo coo = new Coo();

    private readonly RoomBits reserved = new RoomBits(coo, new Bits(0b0001));
    private readonly RoomBits used = new RoomBits(coo, new Bits(0b0010));

    private readonly RoomBits clientTime = new RoomBits(coo, new Bits(0b11110000))
    {
        public override void set(ROOMA r, int t)
        {
            if (get() > 0)
                ins.prisoners--;
            if (t == 0)
            {
                ENTITY e = SETT.ENTITIES().getAtTileSingle(coo.x(), coo.y());
                if (e != null && e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;
                    if (RND.oneIn(5))
                        h.kill(false, CAUSE_LEAVES.PUNISHED());
                    h.interrupt();
                }
            }
            base.set(r, t);
            if (get() > 0)
                ins.prisoners++;
        }
    };

    public PoliceWork(ROOM_POLICE b)
    {
        this.b = b;
    }

    private bool init(int tx, int ty)
    {
        PoliceInstance ins = SETT.ROOMS().POLICE.get(tx, ty);
        if (ins == null)
            return false;
        int c = SETT.ROOMS().fData.tileData.get(tx, ty);
        if (c == 0)
            return false;
        this.ins = ins;
        this.coo.set(tx, ty);
        return true;
    }

    public SETT_JOB job(int tx, int ty)
    {
        if (init(tx, ty))
            return job;
        return null;
    }

    public Humanoid clientToFetch(int tx, int ty)
    {
        if (!init(tx, ty))
            return null;

        if (ins.prisoners >= ins.prisonersMax())
            return null;
        if (clientTime.get() > 0)
            return null;
        if (SETT.ENTITIES().hasAtTile(coo.x(), coo.y()))
            return null;
        if ((SETT.ROOMS().fData.tileData.get(tx, ty) & PoliceConstructor.bitService) != PoliceConstructor.bitService)
            return null;

        for (int i = 0; i < 10; i++)
        {
            Humanoid h = pollVictim();
            if (!validVictim(h) || !b.access(h.indu().popCL()).is())
                continue;
            return h;
        }
        return null;
    }

    private Humanoid[] queue = new Humanoid[256];
    private int hi = queue.Length;

    private Humanoid pollVictim()
    {
        if (hi >= queue.Length)
        {
            hi = 0;
            GUTIL.flooder().init(this);

            ENTITY[] es = SETT.ENTITIES().getAllEnts();
            for (int i = 0; i < es.Length; i++)
            {
                ENTITY e = es[i];
                if (validVictim(e))
                    GUTIL.flooder().pushSmaller(i % SETT.TWIDTH, i / SETT.TWIDTH, RND.rFloat());
            }
            hi = queue.Length - 1;
            while (hi >= 0 && GUTIL.flooder().hasMore())
            {
                PathTile t = GUTIL.flooder().pollSmallest();
                int i = t.x() + t.y() * SETT.TWIDTH;
                ENTITY e = es[i];
                queue[hi] = (Humanoid)e;
                hi--;
            }
            hi++;
            GUTIL.flooder().done();
        }
        return queue[hi++];
    }

    private bool validVictim(ENTITY e)
    {
        if (e == null || !(e is Humanoid))
            return false;
        Humanoid h = (Humanoid)e;

        if (h.isRemoved())
            return false;

        RoomInstance ins = STATS.WORK().EMPLOYED.get(h);
        if (ins != null && (ins.blueprintI() == SETT.ROOMS().GUARD || ins.blueprintI() == b))
            return false;
        if (b.is(h.tc()))
            return false;
        return true;
    }

    public Humanoid client(int tx, int ty)
    {
        if (!init(tx, ty))
        {
            return null;
        }

        if ((SETT.ROOMS().fData.tileData.get(tx, ty) & PoliceConstructor.bitService) != PoliceConstructor.bitService)
            return null;

        if (clientTime.get() == 0)
            return null;

        ENTITY e = SETT.ENTITIES().getAtTileSingle(tx, ty);
        if (e == null)
            return null;

        if (e is Humanoid)
            return (Humanoid)e;
        return null;
    }

    public bool isLay(int tx, int ty)
    {
        return SETT.ROOMS().fData.tileData.get(tx, ty) == PoliceConstructor.bitBed;
    }

    public DIR victimDir(int tx, int ty)
    {
        FurnisherItem f = SETT.ROOMS().fData.item.get(tx, ty);
        if (f == null)
            return DIR.NE;
        return DIR.ORTHO.get(f.rotation);
    }

    public void deliverClient(int tx, int ty)
    {
        if (!init(tx, ty))
            return;
        clientTime.set(ins, 2 + RND.rInt(2));
    }

    public void dispose(int tx, int ty)
    {
        if (!init(tx, ty))
            return;
        clientTime.set(ins, 0);
    }

    public void update(int tx, int ty)
    {
        if (!init(tx, ty))
            return;
        clientTime.inc(ins, -1);
    }

    private readonly SETT_JOB job = new SETT_JOB()
    {
        public override void jobReserve(RESOURCE r)
        {
            reserved.set(ins, 1);
        }

        public override bool jobReservedIs(RESOURCE r)
        {
            return reserved.get() == 1;
        }

        public override void jobReserveCancel(RESOURCE r)
        {
            used.set(ins, 0);
            reserved.set(ins, 0);
        }

        public override bool jobReserveCanBe()
        {
            return reserved.get() == 0;
        }

        public override RBIT jobResourceBitToFetch()
        {
            return null;
        }

        public override double jobPerformTime(Humanoid a)
        {
            return 45;
        }

        public override void jobStartPerforming()
        {
            used.set(ins, 1);
        }

        public override RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm)
        {
            jobReserveCancel(r);
            if (clientTime.get() == 0)
            {
                ENTITY e = SETT.ENTITIES().getAtTileSingle(coo.x(), coo.y());
                if (e != null && e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;
                    h.interrupt();
                }
            }
            return null;
        }

        public override COORDINATE jobCoo()
        {
            return coo;
        }

        public override CharSequence jobName()
        {
            return ins.blueprintI().employment().verb;
        }

        public override bool jobUseTool()
        {
            return false;
        }

        public override bool jobUseHands()
        {
            return ((coo.x() + coo.y() * SETT.TWIDTH + TIME.hours().bitsSinceStart()) & 1) == 1;
        }

        public override SoundRace jobSound()
        {
            return ins.blueprintI().employment().sound();
        }
    };
}