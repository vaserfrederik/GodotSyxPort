using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Main.Placement
{
    public class Instance : Room.RoomInstanceImp
    {
        private static readonly long SerialVersionUID = 1L;

        private readonly Rec bounds = new Rec();
        private int area = 0;
        public readonly Coo mCoo = new Coo();
        private transient RoomBlueprintImp blue;

        public int Unroofed { get; private set; }
        public int Upgrade { get; private set; }

        private bool bodyChange = false;

        protected void Set(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty) && !SETT.ROOMS.Map.Is(tx, ty))
            {
                int i = tx + ty * SETT.TWIDTH;
                SetSoft(tx, ty);
                SETT.ROOMS.Data.Set(this, i, 0);
                SETT.TILE_MAP().MiniCUpdate(tx, ty);
            }
        }

        void Set(TmpArea area, RoomBlueprintImp blue)
        {
            Init(blue);

            foreach (COORDINATE c in area.Body())
            {
                if (!area.Is(c))
                    continue;

                SETT.LIGHTS().Remove(c.X, c.Y);
            }

            bounds.Set(area);
            this.area = area.Area();
            mCoo.Set(area.Mx(), area.My());

            area.ReplaceAndClear(this);
        }

        public override int Upgrade()
        {
            return Upgrade;
        }

        public override void UpgradeSet(int upgrade)
        {
            if (blue == null)
                this.Upgrade = 0;
            else
                this.Upgrade = CLAMP.I(upgrade, 0, blue.Upgrades().Max());
        }

        private void SetSoft(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                bounds.Unify(tx, ty);
                area++;
                SetIndex(tx, ty);
                SETT.ROOMS.Data.Set(this, tx, ty, 0);
                if (area == 1)
                    mCoo.Set(tx, ty);
                if (!TERRAIN().Get(tx, ty).RoofIs())
                    Unroofed++;
            }
        }

        void Clear(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
            {
                int i = tx + ty * SETT.TWIDTH;

                if (Is(tx, ty))
                {
                    bodyChange = true;
                    SETT.ROOMS.FData.ItemClear(tx, ty, this);
                    SETT.ROOMS.Data.Set(this, tx, ty, 0);
                    ClearIndex(tx, ty);
                    SETT.TILE_MAP().MiniCUpdate(tx, ty);
                    if (dFloored.Is(SETT.ROOMS.Data.Get(i), 1))
                        FLOOR().Clearer.Clear(i);

                    area--;
                    if (!TERRAIN().Get(tx, ty).RoofIs())
                        Unroofed--;
                    if (area == 0)
                    {
                        bounds.Set(TWIDTH, 0, THEIGHT, 0);
                        mCoo.Set(-1, -1);
                    }
                }
            }
        }

        private void SetBlueprint(RoomBlueprintImp blue)
        {
            if (area > 0)
            {
                foreach (COORDINATE c in bounds)
                {
                    Clear(c.X, c.Y);
                }
            }
            ClearRegardless();
            SetBlue(blue);
            UpgradeSet(0);
        }

        void Init(RoomBlueprintImp blue)
        {
            SetBlueprint(blue);
        }

        void Clear(RoomBlueprintImp blue)
        {
            SetBlueprint(blue);
            ClearRegardless();
            this.blue = blue;
            UpgradeSet(0);
        }

        void ClearRegardless()
        {
            bounds.Set(TWIDTH, 0, THEIGHT, 0);
            mCoo.Set(-1, -1);
            area = 0;
            Unroofed = 0;
            SetBlue(null);
            Upgrade = 0;
        }

        private void SetBlue(RoomBlueprintImp blue)
        {
            this.blue = blue;
        }

        protected Instance(ROOMS m, RoomBlueprint p) : base(m, p, true)
        {
        }

        protected override bool LoadExtra(FileGetter file) => false;

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            FurnisherItemTile it = SETT.ROOMS.FData.Tile.Get(i.Tile());
            if (it != null && it.Sprite() != null)
            {
                if (dConstructed.Is(i.Tile(), 0))
                {
                    // SPRITES.cons().color.ok.bind();
                    // it.sprite.renderPlaceholder(r, i.x(), i.y(), ROOMS().fData.spriteData.get(i.tile()), i.tx(), i.ty(), it);
                }
                else if (dBroken.Is(i.Tile(), 1))
                {
                    it.Sprite().RenderBroken(r, shadowBatch, i.X, i.Y, i, SETT.ROOMS.FData.Item.Get(i.Tile()));
                }
                else
                {
                    return it.Sprite().Render(r, shadowBatch, SETT.ROOMS.FData.SpriteData.Get(i.Tile()), i, 0, false);
                }
                // if (it.mustBeReachable)
                //     SPRITES.cons().ICO.arrows_inwards.render(r, i.x(), i.y());
            }
            else
            {
                // if (ConstructionData.dExpensive.is(i.tile(), 1))
                //     GCOLORS_PLACABLE.SOSO.bind();
                // else
                //     SPRITES.cons().color.ok.bind();
                // int m = 0;
                // for (DIR d : DIR.ORTHO) {
                //     if (is(i.tx(), i.ty(), d))
                //         m |= d.mask();
                // }
                // ROOMS().placement.blueprint.constructor().renderEmbryo(r, m, i, dFloored.is(i.tile(), 1), this);
            }

            // if (ROOMS().placement.autoWalls.isOn() && !repair) {
            //     ROOMS().placement.door.renderWall(r, this, i);
            // }

            // COLOR.unbind();
            return false;
        }

        private Rec tmp = new Rec();
        private Rec tmp2 = new Rec();

        protected override bool RenderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator it)
        {
            if (bodyChange && area > 0)
            {
                bodyChange = false;
                bool first = true;
                tmp.Set(body());
                foreach (COORDINATE c in tmp)
                {
                    if (Is(c))
                    {
                        if (first)
                        {
                            first = false;
                            tmp2.SetDim(1).MoveX1Y1(c);
                        }
                        else
                        {
                            tmp2.Unify(c.X, c.Y);
                        }
                    }
                }
                bounds.Set(tmp2);
            }

            if (dConstructed.Is(it.Tile(), 1) && dBroken.Is(it.Tile(), 0))
            {
                RoomSprite sp = SETT.ROOMS.FData.Sprite.Get(it.Tile());
                if (sp != null)
                    sp.RenderAbove(r, shadowBatch, SETT.ROOMS.FData.SpriteData.Get(it.Tile()), it, 0);
            }
            return false;
        }

        protected override bool RenderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator it)
        {
            if (dConstructed.Is(it.Tile(), 1) && dBroken.Is(it.Tile(), 0))
            {
                RoomSprite sp = SETT.ROOMS.FData.Sprite.Get(it.Tile());
                if (sp != null)
                    sp.RenderBelow(r, shadowBatch, SETT.ROOMS.FData.SpriteData.Get(it.Tile()), it, 0);
            }
            if (Constructor() != null)
            {
                Constructor().RenderTileBelow(r, shadowBatch, it, dFloored.Is(it.Tile(), 1));
            }
            return false;
        }

        protected override AVAILABILITY GetAvailability(int tile) => null;

        public override Furnisher Constructor()
        {
            return blue?.Constructor();
        }

        protected object ReadResolve()
        {
            Instance i = SETT.ROOMS.Placement.Placer.Instance;
            i.bounds.Set(bounds);
            i.area = area;
            i.mCoo.Set(mCoo);
            i.Unroofed = Unroofed;
            return i;
        }

        public override RECTANGLE Body() => bounds;

        public override void DestroyTile(int tx, int ty) { }

        public override bool DestroyTileCan(int tx, int ty) => false;

        public override ROOM_DEGRADER Degrader(int tx, int ty) => null;

        public override int MX() => mCoo.X;

        public override int MY() => mCoo.Y;

        public override int Area() => area;

        public override string Name(int tx, int ty) => null;

        public override Icon Icon() => null;

        public override int ResAmount(int ri, int upgrade) => 0;

        public override bool Is(int tile) => SETT.ROOMS.Map.IndexGetter.Get(tile) == RoomI;

        public override TmpArea Remove(int tx, int ty, bool scatter, object user, bool forced) => null;
    }
}