using init.constant;
using init.race;
using init.resources;
using settlement.entity.animal;
using settlement.main;
using settlement.room.main.util;
using settlement.thing;
using settlement.thing.ThingsCadavers;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.rendering.RenderData;
using util.rendering;

public abstract class TempleAltar
{
    protected readonly ROOM_TEMPLE blue;
    protected TempleInstance ins;
    protected Coo coo = new Coo();

    protected readonly RoomBits resources = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_1111_1111);

    private TempleAltar(ROOM_TEMPLE blue)
    {
        this.blue = blue;
    }

    public TempleAltar Get(int tx, int ty)
    {
        ins = blue.Get(tx, ty);
        if (ins != null)
        {
            if (SETT.ROOMS().fData.tile.Is(tx, ty, blue.constructor.es))
            {
                coo.Set(tx, ty);
                return this;
            }
        }
        return null;
    }

    public void UpdateDay(int tx, int ty)
    {
        if (Get(tx, ty) == null)
            return;
        double d = blue.STIME;
        int am = (int)d;
        if (RND.rFloat() < (d - am))
            am++;

        UpdateDay(am);
    }

    protected abstract void UpdateDay(int sac);

    public abstract void Dispose(int tx, int ty);
    protected abstract void Render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it);

    public void ResourceInc(int am)
    {
        resources.Inc(ins, am);
    }

    public bool ResourceNeeds()
    {
        return ins.resHas && resources.Get() < CLAMP.i((int)Math.Ceiling(blue.STIME * 3), 0, 10);
    }

    public COORDINATE Coo()
    {
        return coo;
    }

    public abstract bool ShouldKill();

    public abstract void Kill();

    public class Resource : TempleAltar
    {
        private readonly RESOURCE res;

        public Resource(ROOM_TEMPLE blue, RESOURCE resources) : base(blue)
        {
            this.res = resources;
        }

        protected override void Render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            int am = resources.Get();

            if (am > 0)
            {
                res.RenderLaying(r, it.X(), it.Y(), it.Ran(), am);
            }
        }

        public override void Dispose(int tx, int ty)
        {
        }

        protected override void UpdateDay(int am)
        {
            ins.sacrificesTotal += am;
            int rr = resources.Get();
            am = CLAMP.i(am, 0, rr);
            resources.Inc(ins, -am);
            ins.sacrifices += am;
            ins.consumed += am;
            blue.consumed += am;
        }

        public override bool ShouldKill()
        {
            return false;
        }

        public override void Kill()
        {
            // TODO Auto-generated method stub
        }
    }

    public class Prisoner : TempleAltar
    {
        protected readonly RoomBits needs = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_0000_0001);
        protected readonly RoomBits reserved = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_0000_0010);
        protected readonly RoomBits ready = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_0000_0100);
        protected readonly RoomBits kills = new RoomBits(coo, 0b0000_0000_0000_0000_0000_0000_0111_0000);
        protected readonly RoomBits race = new RoomBits(coo, 0b0000_0000_1111_1111_1111_0000_0000_0000);

        public Prisoner(ROOM_TEMPLE blue) : base(blue)
        {
        }

        protected override void Render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
        {
            long ran = it.BigRan();
            int a = kills.Get();
            COLOR col = RACES.all().Get(race.Get()).appearance().colors.blood;
            col.Bind();
            if (a > 0)
            {
                int cx = it.X() + C.TILE_SIZEH;
                int cy = it.Y() + C.TILE_SIZEH;
                for (int i = 0; i < a; i++)
                {
                    int xx = (int)(cx + (-4 + (ran & 0x07)) * C.SCALE);
                    ran = ran >> 3;
                    int yy = (int)(cy + (-4 + (ran & 0x07)) * C.SCALE);
                    ran = ran >> 3;
                    SETT.THINGS().sprites.bloodPool.Render(r, (int)(ran & 0x0F), xx, yy);
                    ran = ran >> 4;
                }
            }
            COLOR.Unbind();
        }

        public override void Dispose(int tx, int ty)
        {
        }

        protected override void UpdateDay(int am)
        {
            am = CLAMP.i(am, 0, 1);
            ins.sacrificesTotal += am;
            int rr = resources.Get();
            am = CLAMP.i(am, 0, rr);
            resources.Inc(ins, -am);
            ins.sacrifices += am;

            hasSacrifice.Set(ins, am);

            ins.consumed += am;
            blue.consumed += am;

            kills.Set(ins, 0);
            Thing t = SETT.THINGS().GetFirst(coo.X(), coo.Y());
            while (t != null)
            {
                t.Remove();
                t = SETT.THINGS().GetFirst(coo.X(), coo.Y());
            }
        }

        private Cadaver Cadaver()
        {
            Cadaver c = SETT.THINGS().cadavers.tGet.Get(coo);
            if (c == null)
            {
                AnimalSpecies s = SETT.ANIMALS().sett().Get(RND.rInt(SETT.ANIMALS().sett().Size()));
                c = SETT.THINGS().cadavers.normal(coo.X() * C.TILE_SIZE + C.TILE_SIZEH, coo.Y() * C.TILE_SIZE + C.TILE_SIZEH, 0, 0, s, 0);
            }
            return c;
        }
    }
}