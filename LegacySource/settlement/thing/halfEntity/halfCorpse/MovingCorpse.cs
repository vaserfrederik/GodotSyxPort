using System;
using System.IO;

using init.constant;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.spirte;
using settlement.main;
using settlement.stats;
using settlement.thing.halfEntity;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.rendering;

public sealed class MovingCorpse : HalfEntity
{
    private double z;
    private double dx;
    private double dy;
    private double mag;

    private static int rsize = 24 * C.SCALE;

    private double x;
    private double y;
    private double dirD;
    private Induvidual indu;
    private bool gore;
    private byte cl;

    public MovingCorpse() : base(rsize, rsize)
    {
    }

    protected override void Save(FilePutter f)
    {
        f.d(x);
        f.d(y);
        f.d(z);
        f.d(mag);
        f.d(dx);
        f.d(dy);
        f.d(dirD);
        f.b(cl);
        indu.Save(f);
    }

    protected override HalfEntity Load(FileGetter f)
    {
        x = f.d();
        y = f.d();
        z = f.d();
        mag = f.d();
        dx = f.d();
        dy = f.d();
        dirD = f.d();
        cl = f.b();
        indu = new Induvidual(f);
        return this;
    }

    protected void Init(Humanoid h, bool gore, CAUSE_LEAVE l)
    {
        x = h.body().cX();
        y = h.body().cY();
        dirD = h.speed.dir().id();
        mag = h.speed.magnitude();
        dx = h.speed.nX();
        dy = h.speed.nY();
        z = h.physics.getZ();
        indu = h.indu();
        this.gore = gore;
        cl = (byte)l.index();
        Add();
    }

    protected override void Update(double ds)
    {
        z -= ds * C.TILE_SIZE;

        if (z <= 0)
        {
            z = 0;
            mag -= ds * (4 * C.TILE_SIZE + mag * 0.1);
        }
        else
        {
            mag -= ds * (8 * C.TILE_SIZE + mag * 0.1);
        }

        if (mag <= 0)
        {
            Remove();
            SETT.THINGS().corpses.Create(indu, (int)x, (int)y, DIR.ALL.Get((int)dirD), !gore, CAUSE_LEAVES.ALL().Get(cl));
            return;
        }

        dirD += C.ITILE_SIZE * mag * ds;
        if (dirD >= DIR.ALL.Size())
            dirD -= DIR.ALL.Size();

        double nx = x + ds * mag * dx;
        double ny = y + ds * mag * dy;

        if (SETT.PATH().solidity.Is(((int)nx) >> C.T_SCROLL, ((int)ny) >> C.T_SCROLL))
        {
            Remove();
            if (SETT.PATH().solidity.Is(((int)x) >> C.T_SCROLL, ((int)x) >> C.T_SCROLL))
            {
                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.PATH().solidity.Is(((int)(x + d.xN() * C.TILE_SIZE)) >> C.T_SCROLL, ((int)(y + d.yN() * C.TILE_SIZE)) >> C.T_SCROLL))
                    {
                        SETT.THINGS().corpses.Create(indu, (int)(x + d.xN() * C.TILE_SIZE), (int)(y + d.yN() * C.TILE_SIZE), DIR.ALL.Get((int)dirD), !gore, CAUSE_LEAVES.ALL().Get(cl));
                        return;
                    }
                }
                return;
            }

            SETT.THINGS().corpses.Create(indu, (int)x, (int)y, DIR.ALL.Get((int)dirD), !gore, CAUSE_LEAVES.ALL().Get(cl));
            return;
        }

        x = nx;
        y = ny;
        body().MoveC(x, y);
    }

    protected override void RemoveAction()
    {
    }

    protected override MovingCorpseFactory Constructor()
    {
        return SETT.HALFENTS().corpses;
    }

    public override void HoverInfo(GBox box)
    {
    }

    protected override void Render(Renderer r, ShadowBatch s, float ds, int x, int y)
    {
        bool inWater = SETT.ENTITIES().submerged.Is(ctx(), cty());

        DIR d = DIR.ALL.Get((int)dirD);

        if (!gore)
            HCorpseRenderer.RenderCorpse(indu, d.id(), inWater, 0, r, s, x, y, (int)z);
        else
            HCorpseRenderer.RenderGore(indu, d.id(), inWater, 0, r, s, x, y);
    }
}