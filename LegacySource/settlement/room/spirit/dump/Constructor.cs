using System;
using System.IO;
using settlement.main;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d;
using snake2d.util.datatypes;
using util.gui.misc;
using util.info;
using util.rendering;
using util.spritecomposer;
using util.text;

namespace settlement.room.spirit.dump
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_DUMP p;
        private readonly TILE_SHEET sheet;

        public readonly FurnisherStat total = new FurnisherStat(this, 1)
        {
            public override double get(AREA area, double acc)
            {
                int a = 0;
                foreach (COORDINATE c in area.body())
                {
                    if (area.is(c))
                    {
                        if (!isEdge(c.x(), c.y(), area))
                            a++;
                    }
                }
                return a;
            }

            public override GText format(GText t, double value)
            {
                return GFORMAT.i(t, (int)value);
            }
        };

        protected Constructor(ROOM_DUMP p, RoomInitData init) : base(init, 0, 1, 384, 108)
        {
            this.p = p;

            sheet = new ITileSheet(init.sp(), 384, 64)
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house2.init(0, 0, 2, 1, d.s16);
                    s.house2.setVar(0).paste(1, true);
                    s.house2.setVar(1).paste(1, true);
                    return d.s16.saveGame();
                }
            }.get();
        }

        public override bool usesArea()
        {
            return true;
        }

        private static readonly CharSequence ¤¤TooThin = "¤Area is too thin at places. Expand the area to at least 3x3 everywhere.";

        public override bool joinsWithFloor()
        {
            return true;
        }

        static
        {
            D.ts(typeof(Constructor));
        }

        public override CharSequence constructionProblem(AREA area)
        {
            foreach (COORDINATE c in area.body())
            {
                if (area.is(c))
                {
                    bool any = false;
                    for (int di = 0; di < DIR.ALLC.size(); di++)
                    {
                        DIR d = DIR.ALLC.get(di);
                        if (area.is(c, d) && !isEdge(c.x() + d.x(), c.y() + d.y(), area))
                        {
                            any = true;
                            break;
                        }
                    }
                    if (!any)
                        return ¤¤TooThin;
                }
            }
            return null;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override bool mustBeOutdoors()
        {
            return false;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new DumpInstance(p, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return p;
        }

        public override void renderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, bool floored)
        {
            int m = SETT.ROOMS().fData.spriteData.get(it.tile());
            if ((m & 0b010000) != 0)
            {
                sheet.render(r, (m & 0x0F) + 16 * (it.ran() & 0b011), it.x(), it.y());
            }
            else if (blue().is(it.tile()))
            {
                floors.get(0).tint.color.bind();
                floors.get(0).sheet.render(r, it.ran() & 0x0F, it.x(), it.y());
            }
        }

        public override void doBeforePlanning(int tx, int ty)
        {
            SETT.ROOMS().fData.spriteData.set(tx, ty, 0);
            base.doBeforePlanning(tx, ty);
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            if (isEdge(tx, ty, area))
            {
                set(tx, ty, area);
                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR d = DIR.ALL.get(di);
                    if (isEdge(tx + d.x(), ty + d.y(), area))
                        set(tx + d.x(), ty + d.y(), area);
                }
            }
            else
            {
                //super.putFloor(tx, ty, upgrade, area);
                SETT.ROOMS().fData.spriteData.set(tx, ty, 0);
            }
        }

        private void set(int tx, int ty, AREA area)
        {
            int m = 0;
            foreach (DIR d in DIR.NORTHO)
            {
                if (joins(tx, ty, d, area) && joins(tx, ty, d.next(-1), area) && joins(tx, ty, d.next(1), area))
                    m |= d.mask();
            }
            SETT.ROOMS().fData.spriteData.set(tx, ty, 0b10000 | m);
        }

        private bool joins(int tx, int ty, DIR d, AREA area)
        {
            tx += d.x();
            ty += d.y();
            if (!area.is(tx, ty))
                return true;
            if (isEdge(tx, ty, area))
                return true;
            return false;
        }

        public bool isEdge(int tx, int ty, AREA area)
        {
            if (!area.is(tx, ty))
                return false;
            foreach (DIR d in DIR.ALL)
                if (!area.is(tx, ty, d))
                    return true;
            return false;
        }
    }
}