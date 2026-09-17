using System;
using System.IO;
using game.audio;
using game.time;
using init.paths;
using init.resources;
using init.sprite;
using settlement.path;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.rnd;
using snake2d.util.sprite;
using util.rendering;
using util.spritecomposer;
using util.text;

namespace settlement.tilemap.terrain
{
    public sealed class TMushroom : TerrainTile
    {
        private readonly TILE_SHEET sheet;
        private const int SET = 16;
        public const double TARGET_FERTILITY = 0.55;
        public const double DELTA_FERTILITY = 0.25;
        private static readonly CharSequence ¤¤name = "¤Shrooms";

        static TMushroom()
        {
            D.ts(typeof(TMushroom));
        }

        private readonly TerrainClearing clearing = new TerrainClearing()
        {
            private SoundRace sound = AUDIO.race("CLEAR_BUSH"),

            public RESOURCE clear1(int tx, int ty)
            {
                shared.NADA.placeFixed(tx, ty);
                return null;
            },

            public bool can()
            {
                return true;
            },

            public int clearAll(int tx, int ty)
            {
                shared.NADA.placeFixed(tx, ty);
                return 0;
            },

            public SoundRace sound(int tx, int ty)
            {
                return sound;
            },

            public bool isEasilyCleared()
            {
                return true;
            }
        };

        public TMushroom(Terrain t) : base("MUSHROOM", t, ¤¤name, SPRITES.icons().m.cancel, null)
        {
            sheet = new ITileSheet(PATHS.SPRITE_SETTLEMENT_MAP().get("Mushroom"), 716, 94)
            {
                protected TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
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
            base.placeRaw(tx, ty);
            shared.data.set(tx, ty, RND.rInt(3));
            return false;
        }

        protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            return false;
        }

        protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            if (TIME.seasons().current() == TIME.seasons().AUTUMN)
            {
                int x = i.x();
                int y = i.y();
                int t = i.ran() & 0x0F;

                data = (int)Math.Round(((i.ran() >> 4) & 3) * TIME.seasons().bitPartOfC());
                t += data * SET;
                sheet.render(r, t, x, y);
                s.setDistance2Ground(0).setHeight(2);
                sheet.render(s, t, x, y);
                i.countVegetation();
            }

            return false;
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