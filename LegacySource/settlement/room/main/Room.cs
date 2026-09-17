using System;
using System.IO;
using settlement.main;
using settlement.maintenance;
using settlement.misc.util;
using settlement.path;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.rendering;

namespace settlement.room.main
{
    [Serializable]
    public abstract class Room : INDEXED
    {
        public const int MAX_SIZE = 2048;
        public const int MAX_DIM = 55;
        private static readonly long serialVersionUID = 1L;
        protected readonly int roomI;
        protected short bI;
        public readonly bool singleton;

        protected Room(ROOMS m, RoomBlueprint p, bool singleton)
        {
            roomI = m.map.create(this, singleton);
            bI = (short)p.index();
            this.singleton = singleton;
        }

        public RoomBlueprint blueprint()
        {
            return ROOMS().all().get(bI);
        }

        protected abstract bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i);

        protected virtual bool renderAbove(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            return false;
        }

        protected virtual bool renderBelow(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator i)
        {
            return false;
        }

        protected virtual void loadFix()
        {
        }

        public abstract CharSequence name(int tx, int ty);

        protected abstract AVAILABILITY getAvailability(int tile);

        public abstract TmpArea remove(int tx, int ty, bool scatter, object user, bool forced);

        protected virtual void saveExtra(FilePutter file)
        {
        }

        protected virtual bool loadExtra(FileGetter file)
        {
            return true;
        }

        public override int index()
        {
            return roomI;
        }

        public virtual RESOURCE_TILE resourceTile(int tx, int ty)
        {
            return null;
        }

        public virtual TILE_STORAGE storage(int tx, int ty)
        {
            return null;
        }

        public abstract bool destroyTileCan(int tx, int ty);
        public abstract void destroyTile(int tx, int ty);

        public double getDegrade(int tx, int ty)
        {
            ROOM_DEGRADER deg = degrader(tx, ty);
            if (deg != null)
                return deg.get();
            return 0;
        }

        public abstract ROOM_DEGRADER degrader(int tx, int ty);

        public abstract int mX(int tx, int ty);
        public abstract int mY(int tx, int ty);
        public abstract int x1(int tx, int ty);
        public abstract int y1(int tx, int ty);
        public abstract int width(int tx, int ty);
        public abstract int height(int tx, int ty);

        public Furnisher constructor()
        {
            return null;
        }

        public abstract SPRITE icon();

        public double isolation(int tx, int ty)
        {
            return 1.0;
        }

        public void isolationSet(int tx, int ty, double isolation)
        {
        }

        public void updateTileDay(int tx, int ty)
        {
        }

        public abstract int resAmount(int ri, int upgrade);

        public int upgrade(int tx, int ty)
        {
            return 0;
        }

        public void upgradeSet(int tx, int ty, int upgrade)
        {
        }

        public bool wallJoiner()
        {
            return false;
        }

        public RoomState makeState(int tx, int ty, bool broken)
        {
            return RoomState.DUMMY;
        }

        protected final void setIndex(int tx, int ty)
        {
            SETT.ROOMS().map.set(tx + ty * SETT.TWIDTH, this);
        }

        protected final void clearIndex(int tx, int ty)
        {
            SETT.ROOMS().map.clear(tx + ty * SETT.TWIDTH, this);
        }

        protected final TmpArea delete(int mx, int my, object o)
        {
            TmpArea a = ROOMS().map.delete(this, mx, my, o);
            return a;
        }

        public abstract int area(int tx, int ty);

        public bool isBadMaintenanceTile(int tx, int ty)
        {
            return false;
        }

        public abstract bool isSame(int tx, int ty, int ox, int oy);

        public abstract class RoomInstanceImp : Room, ROOMA
        {
            private static readonly long serialVersionUID = 1L;

            protected RoomInstanceImp(ROOMS m, RoomBlueprint p, bool singleton) : base(m, p, singleton)
            {
            }

            public override bool isSame(int tx, int ty, int ox, int oy)
            {
                return SETT.IN_BOUNDS(ox, oy) && SETT.ROOMS().map.indexGetter.get(tx, ty) == roomI && SETT.ROOMS().map.indexGetter.get(ox, oy) == roomI;
            }

            public override int mX(int tx, int ty)
            {
                return mX();
            }

            public override int mY(int tx, int ty)
            {
                return mY();
            }

            public override int x1(int tx, int ty)
            {
                return body().x1();
            }

            public override int y1(int tx, int ty)
            {
                return body().y1();
            }

            public override int width(int tx, int ty)
            {
                return body().width();
            }

            public override int height(int tx, int ty)
            {
                return body().height();
            }

            public override bool is(int tx, int ty)
            {
                return SETT.IN_BOUNDS(tx, ty) && is(tx + ty * SETT.TWIDTH);
            }

            public override int area(int tx, int ty)
            {
                return area();
            }

            public override int upgrade(int tx, int ty)
            {
                return upgrade();
            }

            public override void upgradeSet(int tx, int ty, int upgrade)
            {
                upgradeSet(upgrade);
            }

            public virtual int upgrade()
            {
                return 0;
            }

            public virtual void upgradeSet(int upgrade)
            {
            }
        }
    }
}