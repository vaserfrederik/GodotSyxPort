using System;
using System.Collections.Generic;
using System.Linq;
using Snake2D;
using Snake2D.Util.Colors;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Sprite;
using Util.Colors;
using Util.Rendering;
using Util.Text;

namespace Settlement.Room.Main.Throne
{
    internal class InstanceConstruction : Room.RoomInstanceImp, ROOM_JOBBER
    {
        private static readonly long SerialVersionUID = 1L;
        private readonly RECTANGLE body;
        private readonly int rot;
        private transient SPRITE icon;

        private bool active;
        private int jobs;

        private static readonly CharSequence ¤¤name = "Throne Construction";
        private const int WORK = 8;
        static
        {
            D.ts(typeof(InstanceConstruction));
        }

        public InstanceConstruction(int x1, int y1, int rot) : base(SETT.ROOMS(), SETT.ROOMS().THRONE, false)
        {
            THRONE p = SETT.ROOMS().THRONE;
            body = new Rec().MoveX1Y1(x1, y1).SetDim(Sprite.Width(rot), Sprite.Height(rot));
            if (SETT.ROOMS().Map.Get(p.construction) is InstanceConstruction)
            {
                ((InstanceConstruction)SETT.ROOMS().Map.Get(p.construction)).Remove();
            }
            BlueprintI().construction.Set(body.CX(), body.CY());

            this.rot = rot;

            foreach (COORDINATE c in body)
            {
                SetIndex(c.X, c.Y);
                SETT.ROOMS().Data.Set(this, c, 0);
            }
            SETT.ROOMS().Map.Init(this);
            jobs = Area();
            active = !SETT.JOBS().planMode.Is();
            foreach (COORDINATE c in body)
            {
                JobSet(c.X, c.Y, active, null);
            }
            SETT.ROOMS().Map.Init(this);
        }

        public override bool Is(int tile)
        {
            return body.HoldsPoint(tile % TWIDTH, tile / TWIDTH);
        }

        public override bool Is(int tx, int ty)
        {
            return body.HoldsPoint(tx, ty);
        }

        public override int Area()
        {
            return Body().Width() * Body().Height();
        }

        public override RECTANGLE Body()
        {
            return body;
        }

        private bool Active()
        {
            return body.Width() > 0;
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            if (SETT.ROOMS().Data.Get(it.Tile()) < WORK)
            {
                COLOR c = active ? GCOLOR.MAP().JOB_ACTIVE : GCOLOR.MAP().JOB_DORMANT;
                c.Bind();
                SPRITES.cons().BIG.solid.Render(r, 0, it.X, it.Y);
                COLOR.Unbind();
            }
            return false;
        }

        protected override bool RenderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            if (SETT.ROOMS().Data.Get(i.Tile()) >= WORK)
            {
                BlueprintI().Sprite.RenderFloor(r, shadowBatch, i);
            }
            return false;
        }

        private THRONE BlueprintI()
        {
            return ROOMS().THRONE;
        }

        protected override AVAILABILITY GetAvailability(int tile)
        {
            return AVAILABILITY.ROOM;
        }

        public override int MX()
        {
            return body.X1();
        }

        public override int MY()
        {
            return body.Y1();
        }

        public override ROOM_DEGRADER Degrader(int tx, int ty)
        {
            return null;
        }

        public override void DestroyTile(int tx, int ty)
        {
        }

        public override bool DestroyTileCan(int tx, int ty)
        {
            return false;
        }

        public override CharSequence Name(int tx, int ty)
        {
            return ¤¤name;
        }

        public override SPRITE Icon()
        {
            if (icon == null)
                icon = new SPRITE.Twin(blueprintI().Sprite.Icon, SPRITES.icons().s.hammer);
            return icon;
        }

        public override int ResAmount(int ri, int upgrade)
        {
            return 0;
        }

        public override void JobFinsih(int tx, int ty, RESOURCE r, int ram)
        {
            SETT.ROOMS().Data.Inc(this, tx, ty, 1);
            if (SETT.ROOMS().Data.Get(tx, ty) >= WORK)
            {
                jobs--;
                if (jobs == 0)
                {
                    Remove(body.X1(), body.Y1(), false, this, false).Clear();
                    new Instance(body.X1(), body.Y1(), rot);
                }
            }
            else
            {
                JobSet(tx, ty, active, null);
            }
        }

        public override void JobToggle(bool toggle)
        {
            active = toggle;
        }

        public override bool JobToggleIs()
        {
            return active;
        }

        public override bool NeedsFertilityToBeCleared(int tx, int ty)
        {
            return true;
        }

        public override bool BecomesSolid(int tx, int ty)
        {
            return false;
        }

        public override int TotalResourcesNeeded(int x, int y)
        {
            return 0;
        }

        public override TmpArea Remove(int tx, int ty, bool scatter, object user, bool forced)
        {
            foreach (COORDINATE c in body)
            {
                if (Is(c))
                {
                    JobClear(c.X, c.Y);
                }
            }
            TmpArea t = base.Delete(tx, ty, user);
            SETT.ROOMS().Map.Init(t);
            return t;
        }

        public override bool NeedsTerrainToBeCleared(int tx, int ty)
        {
            TerrainTile t = SETT.TERRAIN().Get(tx, ty);
            if (t.RoofIs())
                return false;
            return ROOM_JOBBER.base.NeedsTerrainToBeCleared(tx, ty);
        }

        public void Remove()
        {
            Remove(MX(), MY(), false, this, true).Clear();
        }

        public override bool IsJobActive()
        {
            return active;
        }
    }
}