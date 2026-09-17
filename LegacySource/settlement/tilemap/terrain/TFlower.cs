using System;
using System.IO;

namespace settlement.tilemap.terrain
{
    public sealed class TFlower : TerrainTile
    {
        private static readonly string ¤¤name = "Flower";
        private readonly GrowableSprite sprite;

        static TFlower()
        {
            D.ts(typeof(TFlower));
        }

        private readonly TerrainClearing clearing = new TerrainClearing
        {
            sound = AUDIO.race("CLEAR"),

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

        public TFlower(Terrain t) : base("FLOWER", t, ¤¤name, SPRITES.icons().m.cancel, null)
        {
            sprite = RESOURCES.growable().sprite("_Flower", 1.0, 1.0);

            sprite.trunk.sheight = 4;
            sprite.trunk.sheightoverGround = 0;
            sprite.trunk.setColors(null, new ColorImp(27, 90, 22), new ColorImp(19, 52, 15));

            sprite.growth.sheight = 0;
            sprite.growth.sheightoverGround = 5;
            sprite.growth.setColors(null, new ColorImp(27, 90, 22).shade(1.2), null);

            for (int i = 0; i < sprite.growth.cripe.Length; i++)
            {
                if (i % 4 == 0)
                {
                    sprite.growth.cripe[i] = new ColorImp(50 + RND.rInt(50), RND.rInt(25), 50 + RND.rInt(50));
                }
            }

            sprite.setPollenColor(new ColorImp(110, 110, 100));

            sprite.makeSheet("_FLOWER");
        }

        public override TerrainClearing clearing()
        {
            return clearing;
        }

        protected override bool place(int tx, int ty)
        {
            if (!is(tx, ty))
            {
                shared.data.set(tx, ty, 0);
                base.placeRaw(tx, ty);
            }
            return false;
        }

        protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            return false;
        }

        protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            double a = (data + 1) * amount.maxI;
            sprite.render(r, s, i, a, a);
            return false;
        }

        public void render(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i)
        {
            sprite.render(r, s, i, 1.0, 1.0);
        }

        public override AVAILABILITY getAvailability(int x, int y)
        {
            return null;
        }

        public override bool isPlacable(int tx, int ty)
        {
            return true;
        }

        public readonly TAmount amount = new TAmount(16, "Flower")
        {
            get = tile =>
            {
                if (TERRAIN().get(tile) == this)
                {
                    return 1 + (shared.data.get(tile) & 0x0F);
                }
                return 0;
            },

            set = (tile, value) =>
            {
                if (value == 0)
                {
                    if (TERRAIN().get(tile) == this)
                        TERRAIN().NADA.placeFixed(tile % TWIDTH, tile / TWIDTH);
                }
                else
                {
                    if (TERRAIN().get(tile) != this)
                        this.placeFixed(tile % TWIDTH, tile / TWIDTH);
                    shared.data.set(tile, value - 1);
                }
                return this;
            }
        };
    }
}