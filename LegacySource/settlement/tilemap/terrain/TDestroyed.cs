using settlement.main;
using game.audio;
using init.resources;
using init.sprite;
using settlement.job;
using settlement.path;
using settlement.tilemap;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.color;
using snake2d.util.sprite.text;
using util.rendering;
using util.text;
using System;

namespace settlement.tilemap.terrain
{
    public sealed class TDestroyed : TerrainTile, ITILE_FIXABLE, IDiagonalizer
    {
        private static string ¤¤broken = "¤broken {0}";
        static
        {
            D.ts(typeof(TDestroyed));
        }

        private readonly TerrainClearing clearing = new TerrainClearing
        {
            clear1 = (int tx, int ty) =>
            {
                shared.NADA.placeFixed(tx, ty);
                return null;
            },
            can = () => true,
            clearAll = (int tx, int ty) =>
            {
                shared.NADA.placeFixed(tx, ty);
                return 0;
            },
            sound = (int tx, int ty) => get(tx, ty).clearing().sound(tx, ty),
            isEasilyCleared = () => true,
            destroy = (int tx, int ty) =>
            {
                // GAME.Notify("here");
                // base.destroy(tx, ty);
            },
            canDestroy = (int tx, int ty) => false,
            strength = () => 0
        };

        public TDestroyed(Terrain t) : base("DESTROYED", t, "", SPRITES.icons().m.cancel, new ColorImp(80, 60, 60))
        {
        }

        public CharSequence name(int tx, int ty)
        {
            Str.TMP.clear().add(¤¤broken);
            Str.TMP.insert(0, get(tx, ty).name());
            return Str.TMP;
        }

        public TerrainTile get(int tx, int ty)
        {
            int i = shared.data.get(tx, ty) & 0x0FF;
            TerrainTile t = shared.all().get(i);
            if (t != null && t is TDestoryable)
            {
                return t;
            }
            return TERRAIN().NADA;
        }

        public TerrainClearing clearing()
        {
            return clearing;
        }

        public void place(int tx, int ty, TDestoryable t, int data)
        {
            int d = data << 9;
            if (t is IDiagonalizer)
            {
                d |= ((IDiagonalizer)t).getDia(tx, ty) ? 0b0100000000 : 0;
            }

            d |= t.code;
            shared.data.set(tx, ty, d);

            placeFixed(tx, ty);
        }

        public int getData(int tx, int ty)
        {
            return (shared.data.get(tx, ty) >> 8) & 0x0FF;
        }

        protected override bool place(int tx, int ty)
        {
            int d = shared.data.get(tx, ty);
            base.placeRaw(tx, ty);
            shared.data.set(tx, ty, d);
            return false;
        }

        protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator i, int data)
        {
            int x = i.x();
            int y = i.y();
            int ran = i.ran();
            TDestoryable t = (TDestoryable)get(i.tx(), i.ty());
            t.breakableRes().renderDebris(r, s, x, y, ran, t.resAmount());
            return false;
        }

        public AVAILABILITY getAvailability(int tx, int ty)
        {
            return AVAILABILITY.PENALTY2;
        }

        public bool isPlacable(int tx, int ty)
        {
            return false;
        }

        public COLOR miniC(int x, int y)
        {
            return get(x, y).miniC(x, y);
        }

        public COLOR miniColorPimped(ColorImp c, int x, int y, bool northern, bool southern)
        {
            return get(x, y).miniColorPimped(c, x, y, northern, southern);
        }

        protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator i, int data)
        {
            // TODO Auto-generated method stub
            return false;
        }

        public Job fixJob(int tx, int ty)
        {
            return ((TDestoryable)get(tx, ty)).fixJob();
        }

        public interface TDestoryable
        {
            public Job fixJob();
            public int resAmount();
            public RESOURCE breakableRes();
        }

        public void setDia(int tx, int ty, bool dia)
        {
        }

        public bool getDia(int tx, int ty)
        {
            return ((shared.data.get(tx, ty) >> 8) & 1) == 1;
        }

        public TerrainTile getTerrain(int tx, int ty)
        {
            return get(tx, ty);
        }
    }
}