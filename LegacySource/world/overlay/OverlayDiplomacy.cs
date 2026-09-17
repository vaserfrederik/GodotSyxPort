using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc;
using game.faction.royalty.opinion;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.colors;
using util.rendering;
using util.text;
using world;
using world.map.regions;

class OverlayDiplomacy : WorldOverlays.OverlayTileNormal
{
    private static readonly string ¤¤name = "¤Diplomacy";
    private static readonly string ¤¤desc = "¤Shows a clear view of factions and their loyalty towards you.";

    static OverlayDiplomacy()
    {
        D.ts(typeof(OverlayDiplomacy));
    }

    public OverlayDiplomacy() : base(¤¤name, ¤¤desc, true, true)
    {
    }

    protected override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
    {
        int m = 0x0F;
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

        COLOR c = GCOLOR.MAP().F_REBEL;
        if (reg != null && reg.faction() != null && reg.faction() != FACTIONS.player())
        {
            ColorImp col = ColorImp.TMP;
            GCOLOR.UI().badToGood(col, ROPINION.get(((FactionNPC)reg.faction()).court().king().roy()));
            c = col;
        }

        c.bind();
        renderUnder(m, r, it);
    }

    public override void renderAbove(Renderer r, ShadowBatch s, RenderData data)
    {
        WORLD.OVERLAY().regNames.renderAbove(r, s, data);
    }
}