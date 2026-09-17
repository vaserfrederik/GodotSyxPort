using System;
using System.IO;
using System.Collections.Generic;
using game;
using game.faction;
using game.time;
using init.constant;
using init.trade;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using util.rendering;
using view.main;
using world;
using world.entity;
using world.map.pathing;
using world.map.regions;

namespace world.entity.caravan
{
    public sealed class Shipment : WEntity
    {
        public const int MAX_DISTANCE = 550;
        private static double speed = C.TILE_SIZE * 0.1;

        private readonly WPath path = new P();
        private short destReg;
        private short destFaction;
        private readonly int[] payload = Alloc.Ii(TR.ALL().Size());

        private byte type;
        private byte size = -1;

        public Shipment() : base(C.TILE_SIZE, C.TILE_SIZE)
        {
        }

        protected override void Save(FilePutter file)
        {
            file.S(destReg);
            file.S(destFaction);
            file.B(type);
            TR.MAP().Saver().Save(payload, file);
            path.Save(file);
        }

        protected override WEntity Load(FileGetter file)
        {
            destReg = file.S();
            destFaction = file.S();
            type = file.B();
            TR.MAP().Loader().Load(payload, file, 0);
            path.Load(file);
            size = -1;
            return this;
        }

        protected override void RenderAboveTerrain(Renderer r, ShadowBatch s, float ds, int x, int y)
        {
            if (WORLD.FOW().Is(ctx(), cty()))
                return;

            int i;

            if (WORLD.WATER().IsBig.Is(body().cX() >> C.T_SCROLL, body().cY() >> C.T_SCROLL))
            {
                i = 8 * 3;
            }
            else
            {
                i = (int)GAME.Intervals().Get05() % 3;
                i *= 8;
            }

            if (size == -1)
            {
                int size = 0;
                foreach (int py in payload)
                    size += py;
                size /= 256;
                this.size = (byte)CLAMP.I(size, 0, 2);
            }

            i += size * 8 * 4;

            s.SetDistance2Ground(0).SetHeight(2);
            WORLD.ENTITIES().Caravans.Caravan.Render(r, i + path.Dir().Id(), x, y);
            WORLD.ENTITIES().Caravans.Caravan.Render(s, i + path.Dir().Id(), x, y);

            int am = 2;
            if (am > 1)
            {

            }

            if (TIME.Light().NightIs() && (TIME.Light().PartOfCircular() * 16 > (destReg & 0x07)))
            {
                DIR d = path.Dir();
                x += 8 * d.X();
                y += 8 * d.Y();
                x += C.TILE_SIZEH / 2 + (4 - (GAME.Intervals().Get05() % 8));
                y += C.TILE_SIZEH / 2 + (4 - (GAME.Intervals().Get04() % 8));
                CORE.Renderer().RenderUniLight(x, y, 2, 128);
            }
        }

        public static void Render(SPRITE_RENDERER r, float ds, int x, int y)
        {
            int i = (int)(VIEW.RenderSecond() * 2) % 3;
            i *= 8;
            i += 8 * 4;
            WORLD.ENTITIES().Caravans.Caravan.Render(r, i + DIR.SW.Id(), x, y);
        }

        protected override void RenderBelowTerrain(Renderer r, ShadowBatch s, float ds, int x, int y)
        {
            // TODO Auto-generated method stub
        }

        protected override void Update(double ds)
        {
            path.Move(this, speed * ds);
            if (!path.IsValid())
            {
                Cancel();
                return;
            }

            if (path.Arrived())
            {
                Region c = Destination();
                if (c != null && Math.Abs(path.X() - c.Cx()) * Math.Abs(path.Y() - c.Cy()) <= 1)
                {
                    Arrive();
                    return;
                }
                else
                {
                    Cancel();
                    return;
                }
            }
        }

        private void Cancel()
        {
            Remove();
        }

        private void Arrive()
        {
            Faction f = Faction();
            if (f == null || f.CapitolRegion() != Destination())
            {
                Cancel();
                return;
            }

            foreach (TRADABLE tt in TR.ALL())
            {
                int am = payload[tt.Index()];
                if (am != 0)
                {
                    TRADE_TYPE t = TRADE_TYPE.All.Get(type);
                    f.Buyer(tt).AddDeliver(am, t);
                    payload[tt.Index()] = 0;
                }
            }

            Remove();
        }

        void Add(int tx, int ty, Faction destination, TRADE_TYPE type)
        {
            for (int i = 0; i < payload.Length; i++)
                payload[i] = 0;
            body().MoveX1Y1(tx * C.TILE_SIZE, ty * C.TILE_SIZE);
            path.Clear();
            this.destFaction = (short)destination.Index();
            this.destReg = (short)destination.CapitolRegion().Index();

            path.Find(tx, ty, destination.CapitolRegion().Cx(), destination.CapitolRegion().Cy());

            this.type = (byte)type.Index;

            Add();
        }

        protected override void AddAction()
        {

        }

        protected override void RemoveAction()
        {
            if (!Constructor().Free.IsFull())
                Constructor().Free.Push(this);
            if (Destination() == null)
            {
                return;
            }
            Faction f = Faction();
            if (f == null || f.CapitolRegion() != Destination())
            {
                return;
            }

            foreach (TRADABLE tt in TR.ALL())
            {
                int am = payload[tt.Index()];
                if (am != 0)
                {
                    TRADE_TYPE t = Type();
                    f.Buyer(tt).AddDeliver(am, t);
                    payload[tt.Index()] = 0;
                }
            }
        }

        protected override Shipments Constructor()
        {
            return WORLD.ENTITIES().Caravans;
        }

        public TRADE_TYPE Type()
        {
            return TRADE_TYPE.All.Get(type);
        }

        public override int GetZ()
        {
            return -1;
        }

        public Shipment Load(TRADABLE r, int amount)
        {
            payload[r.Index()] = CLAMP.I(amount + payload[r.Index()], 0, int.MaxValue);
            size = -1;
            return this;
        }

        public Shipment LoadAndReserve(TRADABLE r, int amount)
        {
            if (amount == 0)
                return this;
            faction().Buyer(r).AddReserve(-payload[r.Index()], Type(), 0, null);
            payload[r.Index()] = CLAMP.I(amount + payload[r.Index()], 0, int.MaxValue);
            faction().Buyer(r).AddReserve(payload[r.Index()], Type(), 0, null);
            size = -1;
            return this;
        }

        public int LoadGet(TRADABLE r)
        {
            return payload[r.Index()];
        }

        public Region Destination()
        {
            return WORLD.REGIONS().GetByIndex(destReg);
        }

        public override WPath Path()
        {
            return path;
        }

        private class P : WPath
        {
            public override Treaty Treaty()
            {
                return Treaty.DUMMY;
            }
        }

        public override Faction Faction()
        {
            return FACTIONS.GetByIndex(destFaction);
        }
    }
}