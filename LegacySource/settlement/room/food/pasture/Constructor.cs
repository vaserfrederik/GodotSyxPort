using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using util.colors;
using util.gui.misc;
using util.info;
using util.rendering;
using util.text;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.sprite;
using settlement.tilemap.terrain;

namespace settlement.room.food.pasture
{
    abstract class Constructor : Furnisher
    {
        private static readonly CharSequence ¤¤TooThin = "¤Area is too thin at places. Expand the area to at least 3x3.";

        static Constructor()
        {
            D.ts(typeof(Constructor));
        }

        private ROOM_PASTURE blue;
        public readonly FurnisherStat workers = new FurnisherStat(this, 1)
        {
            public override double Get(AREA area, double fromItems)
            {
                return ROOM_PASTURE.WORKERS_PER_TILE * Ferarea.Get(area, fromItems);
            }

            public override GText Format(GText t, double value)
            {
                return GFORMAT.f(t, value, 1);
            }
        };

        public const int STORAGE1 = 100;
        public const int STORAGE2 = 200;
        public const int STORAGE3 = 300;

        public readonly FurnisherStat Ferarea;
        public readonly FurnisherStat Efficiency;
        public readonly FurnisherStat Irri;

        private readonly RoomSpriteCombo fence;
        private readonly RoomSpriteCombo fenceDia;

        protected Constructor(ROOM_PASTURE blue, RoomInitData init)
            : base(init, 2, 4, 88, 44)
        {
            this.blue = blue;

            Irri = new FurnisherStat.FurnisherStatIrrigation(this, blue);
            Efficiency = new FurnisherStat.FurnisherStatEfficiency(this, workers);

            Ferarea = new FurnisherStat(this, 1)
            {
                public override double Get(AREA area, double fromItems)
                {
                    double f = 0;
                    outer: foreach (COORDINATE c in area.body())
                    {
                        if (!area.Is(c))
                            continue;
                        foreach (DIR d in DIR.ALL)
                        {
                            if (!area.Is(c, d))
                            {
                                continue outer;
                            }
                        }
                        f += Fertility(c.x(), c.y());
                    }
                    return f;
                }

                public override GText Format(GText t, double value)
                {
                    double am = 0;

                    foreach (IndustryResource o in blue.industries().get(0).outs())
                        am += o.rate;
                    //am *= blue.bonus().get(POP_CL.clP(null, HCLASSES.CITIZEN()));
                    return GFORMAT.f(t, (ROOM_PASTURE.WORKERS_PER_TILE * value * am), 1);
                }
            };

            Json js = init.data().json("SPRITES");

            fence = new RoomSpriteCombo(js, "FENCE_COMBO");
            fenceDia = new RoomSpriteCombo(js, "FENCE_D_COMBO");
        }

        protected void MakeAux(Json js) throws IOException
        {
            final RoomSpriteImp auxEdge = new RoomSprite1xN(js, "AUX_EDGE_1X1", false)
            {
                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.Render(r, s, data, it, degrade, false);
                }
            };
            final RoomSpriteImp auxMid = new RoomSprite1xN(js, "AUX_MID_1X1", false)
            {
                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.Render(r, s, data, it, degrade, false);
                }
            };
            final RoomSpriteImp auxEnd = new RoomSpriteImp(js, "AUX_END")
            {
                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.Render(r, s, data, it, degrade, false);
                }
            };

            FurnisherItemTile gc1 = new FurnisherItemTile(auxEdge);
            FurnisherItemTile gc2 = new FurnisherItemTile(auxMid);
            FurnisherItemTile gc3 = new FurnisherItemTile(auxEnd);

            FurnisherItemTile[] items = new FurnisherItemTile[] { gc1, gc2, gc3 };

            foreach (FurnisherItemTile item in items)
            {
                // Add item to the appropriate collection or process as needed
            }
        }

        public override bool UsesWall()
        {
            return true;
        }

        public override bool UsesFloor()
        {
            return true;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new PastureInstance(blue, area, init);
        }

        public override CharSequence ConstructionProblem(AREA area)
        {
            foreach (COORDINATE c in area.body())
            {
                if (area.Is(c))
                {
                    bool ok = false;
                    foreach (DIR d in DIR.ALL)
                    {
                        if (IsFull(c, area, d))
                        {
                            ok = true;
                            break;
                        }
                    }
                    if (!ok)
                    {
                        GUTIL.filler().done();
                        return ¤¤TooThin;
                    }
                }
            }
            return null;
        }

        private bool IsFull(COORDINATE c, AREA a, DIR d)
        {
            int tx = c.x() + d.x();
            int ty = c.y() + d.y();
            if (!a.Is(tx, ty))
                return false;
            foreach (DIR dd in DIR.ALL)
            {
                if (!a.Is(tx, ty, dd))
                    return false;
            }
            return true;
        }

        public override void RenderTileBelow(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, bool floored)
        {
            if (floored)
                RenderFence(r, s, it, 0);
        }

        public bool FenceJoin(ROOMA ii, int tx, int ty)
        {
            if (!ii.Is(tx, ty))
                return false;
            if (!FenceJoin(SETT.ROOMS().fData.tile.Get(tx, ty)))
                return false;

            foreach (DIR d in DIR.ALL)
            {
                if (!ii.Is(tx, ty, d) && !SETT.TERRAIN().Get(tx, ty, d).IsMassiveWall())
                    return true;
            }
            return false;
        }

        protected abstract bool FenceJoin(FurnisherItemTile gc);

        public bool IsFence(ROOMA ii, int tx, int ty)
        {
            if (!ii.Is(tx, ty))
                return false;
            if (SETT.ROOMS().fData.tile.Get(tx, ty) != null)
                return false;
            foreach (DIR d in DIR.ORTHO)
            {
                if (!ii.Is(tx, ty, d))
                    return true;
            }
            return false;
        }

        public void RenderFence(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade)
        {
            ROOMA ii = SETT.ROOMS().map.rooma.Get(it.tx(), it.ty());
            if (ii == null)
                return;
            if (!FenceJoin(ii, it.tx(), it.ty()))
                return;

            int m = 0;

            foreach (DIR d in DIR.ORTHO)
            {
                if (FenceJoin(ii, it.tx() + d.x(), it.ty() + d.y()) || SETT.TERRAIN().Get(it.tx() + d.x(), it.ty() + d.y()).IsMassiveWall())
                    m |= d.mask();
            }
            if (m != 0x0F)
            {
                if (SETT.ROOMS().fData.spriteData.Get(it.tile()) == 1)
                {
                    fenceDia.Render(r, s, m, it, degrade, false);
                }
                else
                {
                    fence.Render(r, s, m, it, degrade, false);
                }
            }
        }

        private readonly Diagonalizer dia = new Diagonalizer()
        {
            public override void SetDia(int tx, int ty, bool dia)
            {
                SETT.ROOMS().fData.spriteData.Set(tx, ty, dia ? 1 : 0);
            }

            public override bool GetDia(int tx, int ty)
            {
                return SETT.ROOMS().fData.spriteData.Get(tx, ty) == 1;
            }
        };

        public override Diagonalizer Dia(int tx, int ty)
        {
            if (blue.Is(tx, ty) && FenceJoin(blue.Get(tx, ty), tx, ty))
                return dia;
            return null;
        }

        public override bool GrowsGrass(int tx, int ty)
        {
            return ROOMS().fData.item.Get(tx, ty) == null;
        }
    }
}