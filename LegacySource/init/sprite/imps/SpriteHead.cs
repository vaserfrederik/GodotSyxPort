using System;
using System.Collections.Generic;
using game;
using game.time;
using init.constant;
using init.race;
using init.sprite;
using init.sprite.game;
using settlement.entity.humanoid.spirte;
using settlement.main;
using snake2d;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite;
using util;
using util.rendering;

namespace init.sprite.imps
{
    class SpriteHead : Sheet
    {
        private readonly TILE_SHEET spikes;

        public SpriteHead(Json json) : base(4, true, false)
        {
            SPRITES.GAME().add(SheetType.s1x1, new ArrayList<Sheet>(this), "SEVERED_HEAD_1X1");

            spikes = SPRITES.GAME().raw(SheetType.s1x1, "HEAD_SPIKE_1X1", json);
        }

        public override void render(SheetData da, int x, int y, RenderIterator it, SPRITE_RENDERER sr, int tile, int random, double degrade)
        {
            int rot = tile & 3;
            spikes.render(sr, GUTIL.ran2().get(it.tile()) % (spikes.tiles() / 4) * 4 + rot, x, y);

            if (!is(it))
                return;

            Race r = RACES.all().getC(random & 0x0FF);

            if (RND.oneIn(CORE.getGraphics().fps() * 4))
            {
                SETT.THINGS().gore.bleed(it.tx() * C.TILE_SIZE + C.TILE_SIZEH, it.ty() * C.TILE_SIZE + C.TILE_SIZEH, 0, 0, r.appearance().colors.blood);
            }

            random = random >> 8;
            int gender = random & 0b11;
            random = random >> 2;

            HSprite.renderHead(sr, r, gender, rot, random, x, y);
        }

        public override void renderShadow(SheetData da, int x, int y, RenderIterator it, ShadowBatch shadow, int tile, int random)
        {
            int rot = tile & 3;

            shadow.setDistance2Ground(0);
            shadow.setHeight(2);
            spikes.render(shadow, GUTIL.ran2().get(it.tile()) % (spikes.tiles() / 4) * 4 + rot, x, y);

            if (!is(it))
                return;

            shadow.setDistance2Ground(da.shadowHeight);
            shadow.setHeight(da.shadowLength);

            Race r = RACES.all().getC(random & 0x0FF);
            random = random >> 8;
            int gender = random & 0b11;
            random = random >> 2;

            HSprite.renderHead(shadow, r, gender, tile & 3, random, x, y);
        }

        private bool is(RenderIterator it)
        {
            int count = GAME.count().EXECUTIONS.current() + 16;
            int r2 = (int)(GUTIL.ran2().get(it.tile()) + TIME.days().bitsSinceStart());

            if (count < (r2 & 0x0FF))
                return false;
            return true;
        }

        public override TextureCoords texture(int tile)
        {
            // TODO Auto-generated method stub
            return null;
        }
    }
}