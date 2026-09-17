using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Food.Farm
{
    using static Settlement.Main.SETT.TERRAIN;

    using Init.Constant;
    using Init.Sprite;
    using Init.Sprite.UI;
    using Init.Sprite.Game;
    using Settlement.Main;
    using Settlement.Overlay;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Main.Util;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Sets;
    using Util.Gui.Misc;
    using Util.Info;
    using Util.Rendering.RenderData;
    using Util.Text;

    internal sealed class Constructor : Furnisher
    {
        private static readonly CharSequence ¤¤warning = "Fertility for this farm is very low, which will result in low yields. It can be improved by digging water around the farm. Proceed anyway?";

        static Constructor()
        {
            D.ts(typeof(Constructor));
        }

        internal readonly bool isIndoors;
        private readonly ROOM_FARM blue;
        internal readonly FurnisherStat fertility = new FurnisherStat(this, 1.0 / 10000.0)
        {
            public GText Format(GText t, double value)
            {
                return GFORMAT.perc(t, value);
            }

            public double Get(AREA area, double fromItems)
            {
                double v = 0;
                foreach (COORDINATE c in area.Body())
                {
                    if (area.Is(c))
                    {
                        v += Fertility(c.X(), c.Y());
                    }
                }
                return v / area.Area();
            }

            public double Max()
            {
                return isIndoors ? 1.0 : 1.2;
            }

            public double Min()
            {
                return isIndoors ? 0.9 : 0;
            }
        };

        internal readonly FurnisherStat workers = new FurnisherStat(this, 0.01)
        {
            public GText Format(GText t, double value)
            {
                return GFORMAT.f(t, value);
            }

            public double Get(AREA area, double fromItems)
            {
                return area.Area() * ROOM_FARM.WORKERPERTILEI;
            }
        };

        public double Fertility(int tx, int ty)
        {
            if (!isIndoors)
            {
                return SETT.GROUND().MAP.Get(tx, ty).farm;
            }
            else
            {
                return SETT.TERRAIN().MOUNTAIN.IsMountain(tx, ty) ? 1.0 : 0.9;
            }
        }

        internal readonly FurnisherStat output;

        internal readonly FurnisherStat irri;

        private readonly LIST<Sheet> sheets;

        protected Constructor(ROOM_FARM blue, RoomInitData init) : base(init, 0, 4, 88, 44)
        {
            irri = new FurnisherStat.FurnisherStatIrrigation(this, blue);

            output = new FurnisherStat.FurnisherStatProduction2(this, blue, 0.01)
            {
                protected override double GetBase(AREA area, double[] acc)
                {
                    double f = 0;
                    foreach (COORDINATE c in area.Body())
                    {
                        if (area.Is(c))
                        {
                            f += Fertility(c.X(), c.Y());
                        }
                    }
                    return ROOM_FARM.WORKERPERTILEI * f * blue.Industries().Get(0).Outs().Get(0).Rate;
                }
            };

            isIndoors = init.Data().Bool("INDOORS");
            this.blue = blue;

            sheets = SPRITES.GAME().Sheets(SheetType.s1x1, "_FARM_DIRT", null);
            // for (Sheet s : sheets) {
            //     s.hasRotation = true;
            //     s.hasShadow = false;
            // }
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return isIndoors;
        }

        public override bool MustBeOutdoors()
        {
            return !isIndoors;
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override void PutFloor(int tx, int ty, int upgrade, AREA area)
        {
            int m = 0;
            foreach (DIR d in DIR.ORTHO)
            {
                if (area.Is(tx, ty, d))
                {
                    m |= d.Mask();
                }
            }
            SETT.ROOMS().fData.SpriteData.Set(tx, ty, m);
            SETT.FLOOR().Clearer.Clear(tx, ty);
        }

        // public override CharSequence Placable(int tx, int ty)
        // {
        //     if (SETT.MINERALS().amountD.Get(tx, ty) > 0)
        //     {
        //         return PLACABLE.E;
        //     }
        //     return base.Placable(tx, ty);
        // }

        public override void RenderEmbryo(SPRITE_RENDERER r, int mask, RenderIterator it, bool isFloored, AREA area, bool active)
        {
            if (isFloored && active)
            {
                COLOR c = CORE.Renderer().ColorGet();
                COLOR.Unbind();
                RenderTill(r, it, area, 0);
                c.Bind();
            }
            base.RenderEmbryo(r, mask, it, isFloored, area, active);
        }

        private readonly Addable overlay = new Addable(true, false)
        {
            public void RenderBelow(Renderer r, RenderIterator it)
            {
                double d = SETT.GROUND().MAP.Get(it.Tile()).farm / SETT.GROUND().types.NORMAL.farm;
                d = CLAMP.d(d, 0, 1);
                d *= d;
                RenderUnder(d, r, it, false);
                if (!SETT.ROOMS().Placement.Embryo.Is(it.Tile()) && TERRAIN().Get(it.Tile()).Clearing().Can() && !SETT.TERRAIN().WATER.DEEP.Is(it.Tile()))
                {
                    double w = SETT.GROUND().MOISTURE_TOT.Get(it.Tile());
                    w = CLAMP.d(w, 0, 1);
                    if (w > 0)
                    {
                        ColorImp.TMP.Interpolate(COLOR.ORANGE100, COLOR.BLUE100, w).Bind();
                        int s = (int)(C.TILE_SIZE / 4 + w * 3 * C.TILE_SIZE / 4);
                        int x1 = it.X() + (C.TILE_SIZE - s) / 2;
                        int y1 = it.Y() + (C.TILE_SIZE - s) / 2;

                        UI.icons().s.drop.Render(r, x1, x1 + s, y1, y1 + s);
                    }
                }
            }
        };

        public override Addable Overlay()
        {
            if (!isIndoors)
            {
                return overlay;
            }
            return null;
        }

        void RenderTill(SPRITE_RENDERER r, RenderIterator it, AREA area, double till)
        {
            int d = Direction(it, area);

            int sheet = 0;
            int rot = 0;

            if (area.Is(it.Tx(), it.Ty(), DIR.ORTHO.Get(d)))
            {
                if (area.Is(it.Tx(), it.Ty(), DIR.ORTHO.Get(d + 2)))
                {
                    sheet = 2;
                    rot = d + 2 * (it.Ran() & 1);
                }
                else
                {
                    sheet = 1;
                    rot = d;
                }
            }
            else if (area.Is(it.Tx(), it.Ty(), DIR.ORTHO.Get(d + 2)))
            {
                sheet = 1;
                rot = d + 2;
            }
            else
            {
                rot = d + 2 * (it.Ran() & 1);
            }

            RenderTill(r, it, till, sheet, rot);
        }

        private void RenderTill(SPRITE_RENDERER r, RenderIterator it, double till, int t, int rot)
        {
            till = 1.0 - till;
            int aa = (int)(till * (sheets.Size() / 3 - 1));

            t = (int)(3 * aa) + t;
            int data = SheetType.s1x1.Tile(sheets.Get(0), SheetData.DUMMY, 0, it.Ran(), rot);
            sheets.Get(t).Render(SheetData.DUMMY, it.X(), it.Y(), it, r, data, it.Ran(), 0);
        }

        int Direction(RenderIterator it, AREA area)
        {
            return (it.Ran(area.Body().X1(), area.Body().Y1()) & 1);
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new FarmInstance(blue, area, init);
        }

        public override CharSequence Warning(AREA area)
        {
            double d = fertility.Get(area, 0);
            if (d < 0.5)
            {
                return ¤¤warning;
            }
            return null;
        }
    }
}