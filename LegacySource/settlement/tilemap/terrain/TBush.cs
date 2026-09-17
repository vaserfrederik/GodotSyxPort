using System;
using System.IO;
using game;
using game.audio;
using init.constant;
using init.paths;
using init.resources;
using init.settings;
using init.sprite;
using settlement.main;
using settlement.path;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.rendering;
using util.spritecomposer;

namespace settlement.tilemap.terrain
{
    public sealed class TBush : TerrainTile
    {
        private readonly TILE_SHEET sheet;
        private const int SET = 16;

        private readonly TileTextureScroller dis1 = SPRITES.textures().dis_low.scroller(12 * 6, -12 * 5.5);

        private readonly TerrainClearing clearing = new TerrainClearing
        {
            sound = AUDIO.race("CLEAR_BUSH"),

            clear1 = (tx, ty) =>
            {
                shared.NADA.placeFixed(tx, ty);
                return null;
            },

            can = () => true,

            clearAll = (tx, ty) =>
            {
                shared.NADA.placeFixed(tx, ty);
                return 0;
            },

            sound = (tx, ty) => sound,

            isEasilyCleared = () => true
        };

        public TBush(Terrain t) : base("BUSH", t, "bush", SPRITES.icons().m.cancel, null)
        {
            sheet = new ComposerThings.ITileSheet(PATHS.SPRITE_SETTLEMENT_MAP().get("Bush"), 716, 94)
            {
                init = (c, s, d) =>
                {
                    s.singles.init(0, 0, 1, 1, 16, 4, d.s16);
                    s.singles.paste(true);
                    return d.s16.saveGame();
                }
            }.get();
        }

        public override TerrainClearing clearing()
        {
            return clearing;
        }

        protected override bool place(int tx, int ty)
        {
            double v = 0;
            for (int i = 0; i < DIR.ALL.size(); i++)
            {
                DIR d = DIR.ALL.get(i);
                if (is(tx, ty, d) || shared.TREES.isTree(tx + d.x(), ty + d.y()))
                    v += 0.5;
            }
            int d = (int)(v - 0.1);
            base.placeRaw(tx, ty);
            shared.data.set(tx, ty, d * SET);
            return false;
        }

        protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            return false;
        }

        protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            render(i, r, s, i.x(), i.y(), i.ran(), data);
            i.countVegetation();
            return false;
        }

        public void update(double ds)
        {
            double w = Math.Pow(SETT.WEATHER().wind.getD(), 1.5);
            if (w > 0.1)
                dis1.update(ds * w);
        }

        private void render(RenderData.RenderIterator i, SPRITE_RENDERER r, ShadowBatch s, int x, int y, int ran, int data)
        {
            int d = data / SET;
            data = CLAMP.i((int)((d + 1) * SETT.WEATHER().moisture.getD() + (i.ran() & 0x03)), 0, d) * SET;

            int t = ran & 0x0F;
            t += data;
            SETT.TERRAIN().colors.tree.get(ran >> 4).bind();
            if (t < 0 || t > SET * 4)
                GAME.Notify("nono");
            sheet.render(r, t, x, y);

            if (S.get().graphics.get() > 0)
            {
                OPACITY.O50.bind();
                TextureCoords ti = SPRITES.textures().dots.get(i.tx(), i.ty(), 0, 0);
                CORE.renderer().renderDisplaced(x, x + C.TILE_SIZE, y, y + C.TILE_SIZE, dis1.get(i.tx(), i.ty()), ti);
                OPACITY.unbind();
            }

            s.setDistance2Ground(0).setHeight(2);
            sheet.render(s, t, x, y);
            COLOR.unbind();
        }

        public void render(RenderData.RenderIterator i, SPRITE_RENDERER r, ShadowBatch s, int x, int y, int ran)
        {
            int ss = (ran & 3) * SET;
            ran = ran >> 2;
            render(i, r, s, x, y, ran, ss);
        }

        public override AVAILABILITY getAvailability(int x, int y)
        {
            return null;
        }

        public override bool isPlacable(int tx, int ty)
        {
            return true;
        }
    }
}