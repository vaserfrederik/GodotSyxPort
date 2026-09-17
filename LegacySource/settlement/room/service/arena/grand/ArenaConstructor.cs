using System;
using System.IO;
using init.sprite;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.misc;
using util.colors;
using util.gui.misc;
using util.info;
using util.rendering;
using util.text;
using view.main;

namespace settlement.room.service.arena.grand
{
    internal sealed class ArenaConstructor : Furnisher
    {
        private readonly int minDim = 24;
        private static readonly CharSequence ¤¤notRec = "The blueprint must be in the shape of a rectangle";
        private static readonly CharSequence ¤¤small = "This is too small to be considered a grand arena. Minimum dimensions are 24 x 24 tiles.";
        private static readonly CharSequence ¤¤onEdge = "Entrance must be placed on the edge of the room.";
        private static readonly CharSequence ¤¤onEdgeC = "Entrance must be placed in the center of an edge of the room.";

        static ArenaConstructor()
        {
            D.ts(typeof(ArenaConstructor));
        }

        private readonly ROOM_ARENA blue;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat spectators;
        public readonly CUtil util;

        private double ri = 0;
        private int cGlad = 0;
        private int cSpec = 0;
        private bool err = false;

        private readonly RoomSprite1x1 csprite;

        protected ArenaConstructor(ROOM_ARENA blue, RoomInitData init)
            : base(init, 1, 2)
        {
            Json sp = init.data().json("SPRITES");
            util = new CUtil(this, sp);

            csprite = new RoomSprite1x1(sp, "CONSTRUCT_1X1");

            this.blue = blue;

            workers = new FurnisherStat(this)
            {
                Format = (t, value) => GFORMAT.i(t, (long)value),
                Get = (area, acc) =>
                {
                    init(area);
                    return cGlad;
                }
            };
            spectators = new FurnisherStat(this)
            {
                Format = (t, value) =>
                {
                    GFORMAT.i(t, (int)Math.Ceiling(value));
                    t.s();
                    t.add('(');
                    GFORMAT.i(t, (int)Math.Ceiling(value * blue.service().totalMultiplier()));
                    t.add(')');
                    return t;
                },
                Get = (area, acc) =>
                {
                    init(area);
                    return cSpec;
                }
            };

            FurnisherItemTile ee = new FurnisherItemTile(
                this,
                true,
                RoomSprite1x1.DUMMY,
                AVAILABILITY.SOLID,
                false)
            {
                IsPlacable = (tx, ty, roomIs, it, rx, ry) =>
                {
                    if (util.getLevel(tx, ty) != 0)
                        return ¤¤onEdge;
                    if (!util.canBeEntrance(tx, ty))
                        return ¤¤onEdgeC;

                    return null;
                }
            };

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ee }
            }, 1);
            flush(4, 0);
        }

        private void init(AREA area)
        {
            if (ri != VIEW.renderSecond())
            {
                err = false;
                ri = VIEW.renderSecond();
                cSpec = 0;
                cGlad = 0;
                if (area.body().width() < minDim || area.body().height() < minDim)
                    err = true;
                foreach (COORDINATE c in area.body())
                {
                    if (!area.is(c))
                    {
                        err = true;
                        break;
                    }
                    FurnisherItemTile it = util.get(c.x(), c.y(), area);
                    if (it == util.iSeat1 || it == util.iSeat2)
                        cSpec++;
                    if (it == util.iArena)
                        cGlad++;
                }

                cSpec -= 10;
                cGlad /= 6;

                if (err)
                {
                    cSpec = 0;
                    cGlad = 0;
                }

                if (cGlad > 0)
                {
                    cGlad = CLAMP.i(cSpec / 40, 1, cGlad);
                }

                cSpec = Math.Max(cSpec, 0);
            }
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            RECTANGLE aa = util.init(area);
            ArenaInstance a = new ArenaInstance(blue, area, init, aa);
            foreach (COORDINATE c in a.body())
            {
                if (a.is(c) && util.tile(c.x(), c.y()) == util.iTorch)
                {
                    SETT.LIGHTS().candle(c.x(), c.y(), 0);
                }
            }
            return a;
        }

        public override CharSequence constructionProblem(AREA area)
        {
            if (area.body().width() < minDim || area.body().height() < minDim)
                return ¤¤small;
            foreach (COORDINATE c in area.body())
            {
                if (!area.is(c))
                    return ¤¤notRec;
            }
            return base.constructionProblem(area);
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            FurnisherItemTile it = util.get(tx, ty);

            if (it == util.iRim || it == util.iArena)
                base.putFloor(tx, ty, upgrade, area);
        }

        public override void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, bool floored)
        {
            if (floored && util.get(it.tx(), it.ty()) != util.iArena)
            {
                csprite.render(r, s, 0, it, 0, false);
            }
            base.renderTileBelow(r, s, it, floored);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override void renderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, bool isFloored, AREA area, bool active)
        {
            init(area);

            if (err)
            {
                if (active)
                    GCOLOR.MAP().BAD.bind();
                base.renderEmbryo(r, mask, it, isFloored, area, active);
            }
            else
            {
                FurnisherItemTile tile = util.get(it.tx(), it.ty(), area);
                int m = 0;
                foreach (DIR d in DIR.ORTHO)
                {
                    if (util.get(it.tx() + d.x(), it.ty() + d.y(), area).availability == tile.availability)
                        m |= d.mask();
                }

                if (tile.availability.player < 0)
                    SPRITES.cons().BIG.solid.render(r, m, it.x(), it.y());
                else
                    SPRITES.cons().BIG.outline.render(r, m, it.x(), it.y());
            }
        }
    }
}