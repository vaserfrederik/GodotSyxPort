using System;
using settlement.main;
using settlement.maintenance;
using settlement.room.main;
using settlement.path;
using snake2d;
using util.rendering;

namespace settlement.room.main.throne
{
    internal sealed class Instance : Room.RoomInstanceImp
    {
        private static readonly long serialVersionUID = 1L;
        private readonly int size = Sprite.width(0) * Sprite.height(0);
        private readonly RECTANGLE body;
        private readonly int rot;

        public Instance(int x1, int y1, int rot) : base(SETT.ROOMS(), SETT.ROOMS().THRONE, false)
        {
            if (SETT.ROOMS().map.get(THRONE.coo()) is Instance existingInstance)
            {
                existingInstance.remove();
            }

            body = new Rec().moveX1Y1(x1, y1).setDim(Sprite.width(rot), Sprite.height(rot));

            blueprintI().setInstance(body.cX(), body.cY());
            this.rot = rot;

            foreach (COORDINATE c in body)
            {
                setIndex(c.x(), c.y());
                SETT.ROOMS().data.set(this, c, 0);
            }
            SETT.ROOMS().map.init(this);

            DIR td = DIR.ORTHO.getC(rot).perpendicular();

            foreach (COORDINATE c in body)
            {
                int tx = c.x();
                int ty = c.y();
                SETT.GRASS().current.set(c, 0);
                if (!TERRAIN().get(tx, ty).roofIs())
                    TERRAIN().NADA.placeFixed(tx, ty);

                int d = 0;
                foreach (DIR dir in DIR.ORTHO)
                {
                    if (body.holdsPoint(c, dir))
                        d |= dir.mask();
                }
                d = DIR.toBoxID(d);
                SETT.ROOMS().data.set(this, tx, ty, d);

                if (!body.holdsPoint(c, td))
                {
                    if (!body.holdsPoint(c, td.next(-2)) || !body.holdsPoint(c, td.next(2)))
                        candle(tx, ty);
                }
            }
            SETT.ROOMS().map.init(this);
        }

        public override bool is(int tile)
        {
            return body.holdsPoint(tile % TWIDTH, tile / TWIDTH);
        }

        public override bool is(int tx, int ty)
        {
            return body.holdsPoint(tx, ty);
        }

        public override int area()
        {
            return size;
        }

        public override RECTANGLE body()
        {
            return body;
        }

        public override TmpArea remove(int tx, int ty, bool scatter, object obj, bool force)
        {
            throw new Exception();
        }

        private void remove()
        {
            foreach (COORDINATE c in body())
            {
                if (!is(c))
                    continue;
                SETT.LIGHTS().remove(c.x(), c.y());
                FLOOR().clearer.clear(c.x(), c.y());
            }
            TmpArea t = base.delete(body().x1(), body().y1(), this);
            SETT.ROOMS().map.init(t);
            t.clear();
        }

        private void candle(int tx, int ty)
        {
            SETT.LIGHTS().candle(tx, ty, 0);
            SETT.ROOMS().data.set(this, tx, ty, SETT.ROOMS().data.get(tx, ty) | 0x10);
        }

        protected override bool render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            if (it.tx() == body().cX() && it.ty() == body().cY())
            {
                blueprintI().sprite.renderThrone(r, shadowBatch, it, rot);
            }
            else if ((SETT.ROOMS().data.get(it.tile()) & 0x010) != 0)
            {
                blueprintI().sprite.renderTorch(r, shadowBatch, it, rot);
            }
            return false;
        }

        protected override bool renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            blueprintI().sprite.renderFloor(r, shadowBatch, i);
            return true;
        }

        public THRONE blueprintI()
        {
            return SETT.ROOMS().THRONE;
        }

        protected override AVAILABILITY getAvailability(int tile)
        {
            return AVAILABILITY.ROOM;
        }

        public override int mX()
        {
            return body.x1();
        }

        public override int mY()
        {
            return body.y1();
        }

        public override ROOM_DEGRADER degrader(int tx, int ty)
        {
            return null;
        }

        public override void destroyTile(int tx, int ty)
        {
        }

        public override bool destroyTileCan(int tx, int ty)
        {
            return false;
        }

        public override string name(int tx, int ty)
        {
            return blueprintI().info.name;
        }

        public override Icon icon()
        {
            return blueprintI().sprite.icon;
        }

        public override int resAmount(int ri, int upgrade)
        {
            return 0;
        }
    }
}