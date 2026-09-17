using System;
using System.Collections.Generic;
using game.faction;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.misc;
using util.rendering;
using world;
using world.overlay;

namespace world.map.regions
{
    public class Gen
    {
        public Gen(ACTION lprinter)
        {
            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    Region reg = WORLD.REGIONS().map.get(it.tile());
                    if (reg == null)
                        return;
                    ColorImp.TMP.set(COLOR.UNIQUE.getC(reg.index())).setBrightnessSelf(2.0);
                    int m = 0;
                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (WORLD.REGIONS().map.get(it.tx(), it.ty(), d) == reg)
                        {
                            m |= d.mask();
                        }
                    }
                    ColorImp.TMP.bind();
                    SPRITES.cons().BIG.outline.render(CORE.renderer(), m, it.x(), it.y());

                    COLOR.unbind();
                }
            };

            if (!GenPlayer.gen())
                return;
            lprinter.exe();
            new GenAssign(lprinter);
            lprinter.exe();
            new GenInit(lprinter);
            lprinter.exe();
            new GenName();
            lprinter.exe();

            WORLD.REGIONS().player.fationSet(FACTIONS.player(), false);
            WORLD.REGIONS().player.setCapitol();

            WORLD.MINIMAP().repaint();

            //WORLD.OVERLAY().debug = null;
        }

        public void clear()
        {
            while (FACTIONS.NPCs().size() > 0)
            {
                FACTIONS.remove(FACTIONS.NPCs().get(0), false);
            }
            WORLD.REGIONS().saver().clear();
        }
    }
}