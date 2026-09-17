using System;
using System.IO;
using game;
using init.constant;
using init.paths;
using init.sprite;
using settlement.thing.pointlight;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.rendering;
using util.spritecomposer;
using util.spritecomposer.ComposerDests;
using util.spritecomposer.ComposerSources;
using util.spritecomposer.ComposerThings;
using view.main;
using world;
using world.army;

namespace world.entity.army
{
    class WArmySprite
    {
        private readonly TILE_SHEET sheet = new ITileSheet(PATHS.SPRITE().getFolder("world").getFolder("entity").get("Army"), 136, 104).init(new ComposerUtil(), new ComposerSources(), new ComposerDests()).saveGame();

        private const int OFF = 8 * 7;
        private const int OFF_BOAT = 8 * 3;

        public WArmySprite() { }

        public void render(WArmy a, Renderer r, ShadowBatch s, int x, int y, DIR dir)
        {
            COLOR color = a.faction() == null ? COLOR.WHITE50 : a.faction().banner().colorBG();

            color.bind();
            SPRITES.cons().BIG.dashed_hollow.renderBox(r, x + 16, y + 16, a.body().width() - 32, a.body().height() - 32);
            COLOR.unbind();

            s.setHeight(1).setDistance2Ground(1);
            s.setHard();

            int d = 0;
            if (a.path().moving(a.body()))
                d = 8 * GAME.intervals().get05() % 3;
            d *= 8;
            if (WORLD.WATER().has.is(a.ctx(), a.cty()))
                d += OFF_BOAT;
            else if (a.state() == WArmyState.fortified)
            {
                d = 2 * OFF_BOAT;
            }

            double si = 2.0 * AD.men(null).get(a) / Config.battle().MEN_PER_ARMY;
            si = CLAMP.d(si, 0, 1);

            int am = (int)Math.Ceiling(si * 16);
            int wi = (int)Math.Ceiling(si * 4.0);
            int hi = (int)Math.Ceiling(1 + si * 3.0);

            int y1 = (int)(y + C.TILE_SIZE - hi * C.TILE_SIZEH / 2.0);
            for (int dy = 0; dy < hi; dy++)
            {
                int py = y1 + dy * C.TILE_SIZEH;
                int w = am;
                w = CLAMP.i(w, 0, wi);
                am -= w;

                int px = x + C.TILE_SIZE;
                px -= C.TILE_SIZEH * w / 2;
                for (int dx = 0; dx < w; dx++)
                {
                    sheet.render(r, d + WArmySprite.OFF + dir.id(), px, py);
                    color.bind();
                    sheet.render(r, d + dir.id(), px, py);
                    COLOR.unbind();
                    sheet.render(s, d + WArmySprite.OFF + dir.id(), px, py);
                    px += C.TILE_SIZEH;
                }
            }

            if (a.raiding())
            {
                FireSparks.render(VIEW.renderSecond() * 26, x + a.body().width() / 2, y + a.body().height() / 2, 30, 1321342345, 0.5);
            }
        }
    }
}