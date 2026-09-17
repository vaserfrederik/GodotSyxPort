using System;
using game.faction;
using game.faction.diplomacy;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering;
using world;
using world.map.regions;

class Army : WorldOverlays.OverlayTile
{
    private Faction army;
    private readonly COLOR rebel = new ColorImp(GCOLOR.MAP().F_REBEL).shadeSelf(0.3f);
    private readonly COLOR ally = new ColorImp(GCOLOR.MAP().F_ALLY).shadeSelf(0.3f);
    private readonly COLOR enemy = new ColorImp(GCOLOR.MAP().F_ENEMY).shadeSelf(0.3f);
    private readonly COLOR soso = new ColorImp(GCOLOR.MAP().SOSO).shadeSelf(0.3f);

    public Army()
        : base(true, true)
    {
    }

    public void Add(Faction army)
    {
        this.army = army;
        base.Add();
    }

    protected override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
    {
        int m = 0x0F;

        if (WORLD.PATH().map.is.is(it.tile()))
        {
            COLOR.WHITE100.bind();
            renderUnder(m, r, it);
            return;
        }

        Region reg = WORLD.REGIONS().map.get(it.tile());
        if (WORLD.REGIONS().border().is(it.tile()))
        {
            m = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (!WORLD.IN_BOUNDS(it.tx(), it.ty(), d) || reg == WORLD.REGIONS().map.get(it.tx(), it.ty(), d))
                {
                    m |= d.mask();
                }
            }
        }

        COLOR c = rebel;

        if (reg != null && reg.faction() != null)
        {
            if (reg.faction() == army)
                c = ally;
            else if (DIP.WAR().is(army, reg.faction()))
                c = enemy;
            else
                c = soso;
        }
        c.bind();
        renderUnder(m, r, it);
    }

    public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
    {
        WORLD.OVERLAY().regNames.renderAbove(r, s, data);
        COLOR.WHITE100.bind();
        base.renderAbove(r, s, data);
    }

    protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
    {
        if (WORLD.PATH().map.is.is(it.tile()) && WORLD.WATER().isBig.is(it.tile()))
            SPRITES.cons().BIG.line.render(r, 0, it.x(), it.y());
    }
}