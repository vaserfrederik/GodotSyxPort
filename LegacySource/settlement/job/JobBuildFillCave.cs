using System;
using System.Collections.Generic;
using game.audio;
using game.faction;
using init.resources;
using init.sprite;
using settlement.entity.humanoid;
using settlement.main;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using util.gui.misc;
using util.text;
using view.main;
using view.tool;

namespace settlement.job
{
    final class JobBuildFillCave : JobBuild
    {
        private static readonly CharSequence ¤¤name = "¤Refill Mountain Cave";
        private static readonly CharSequence ¤¤desc = "¤Refills dug tunnels or natural mountain caves";

        static
        {
            D.ts(typeof(JobBuildFillCave));
        }

        JobBuildFillCave() : base(
            "FILL_MOUNTAIN",
            RESOURCES.STONE(),
            2, true,
            ¤¤name,
            ¤¤desc,
            SETT.TERRAIN().MOUNTAIN.getIcon())
        {
            this.placer = new Placer(this, RESOURCES.STONE(), 2, ¤¤desc)
            {
                private readonly string jobs = "Jobs: ";

                public override void placeInfo(GBox b, int okTiles, AREA a)
                {
                    base.placeInfo(b, okTiles, a);
                    if (okTiles > 0)
                    {
                        VIEW.hoverBox().add(VIEW.hoverBox().text().add(jobs).add(okTiles));
                    }
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return SETT.JOBS().clearss.butts;
                }
            };
        }

        public override void renderAbove(SPRITE_RENDERER r, int x, int y, int mask, int tx, int ty)
        {
            SPRITES.cons().ICO.unclear.render(r, x, y);
        }

        protected override CharSequence problem(int tx, int ty, bool overwrite)
        {
            if (ROOMS().map.is(tx, ty))
                return PlacableMessages.¤¤ROOM_BLOCK;
            if (SETT.PLACA().willBlock.is(tx, ty))
            {
                return PlacableMessages.¤¤BLOCK_WILL;
            }
            if (!overwrite)
            {
                if (JOBS().getter.is(tx, ty))
                {
                    return PlacableMessages.¤¤JOB_BLOCK;
                }
            }

            if (!TERRAIN().CAVE.is(tx, ty))
                return PlacableMessages.¤¤CAVE_MUST;
            return null;
        }

        public override bool terrainNeedsClear(int tx, int ty)
        {
            return false;
        }

        protected override double constructionTime(Humanoid h)
        {
            return 20;
        }

        protected override bool construct(int tx, int ty)
        {
            GAME.player().res().inc(RESOURCES.STONE(), RTYPE.CONSTRUCTION, -2);
            SETT.FLOOR().clearer.clear(tx, ty);
            TERRAIN().MOUNTAIN.placeFixed(tx, ty);
            TERRAIN().MOUNTAIN.strengthSet(tx, ty, 0);

            for (DIR d : DIR.ALLC)
            {
                if (TERRAIN().CAVE.canFix(tx + d.x(), ty + d.y()))
                {
                    TERRAIN().CAVE.fix(tx + d.x(), ty + d.y());
                }
            }
            return false;
        }

        protected override SoundRace constructSound()
        {
            return TERRAIN().MOUNTAIN.clearing().sound(coo.x(), coo.y());
        }

        public override bool becomesSolid()
        {
            return true;
        }

        public override bool isConstruction()
        {
            return true;
        }

        public override TerrainTile becomes(int tx, int ty)
        {
            return TERRAIN().MOUNTAIN;
        }
    }
}