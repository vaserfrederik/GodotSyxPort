using System;
using System.Collections.Generic;
using System.IO;
using game.debug;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.thing.halfEntity.caravan;
using settlement.thing.halfEntity.dingy;
using settlement.thing.halfEntity.halfCorpse;
using settlement.thing.halfEntity.transport;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;
using util.rendering;

namespace settlement.thing.halfEntity
{
    public class HalfEnts : SettResource
    {
        private readonly ArrayListResize<HalfEntity> ents;
        private readonly _WMap map;
        private readonly ArrayList<HalfEntity> tmp = new ArrayList<HalfEntity>(2056);
        private readonly ArrayList<Factory<?>> constructors = new ArrayList<>(20);

        public readonly Caravans caravans = new Caravans(constructors);
        public readonly TransportFactory transports = new TransportFactory(constructors);
        public readonly MovingCorpseFactory corpses = new MovingCorpseFactory(constructors);
        public readonly DingyFactory dingy = new DingyFactory(constructors);
        private readonly Tree<HalfEntity> renderables = new Tree<HalfEntity>(2056)
        {
            protected override bool IsGreaterThan(HalfEntity current, HalfEntity cmp)
            {
                return current.Z < cmp.Z;
            }
        };

        public HalfEnts() : base("HALF_ENTS", true)
        {
            ents = new ArrayListResize<HalfEntity>(1024, 64000);
            map = new _WMap(SETT.PWIDTH, SETT.PHEIGHT);
        }

        protected override void Load(FileGetter file)
        {
            ents.Clear();
            map.Clear();
            foreach (Factory<?> f in constructors)
                f.Clear();
            int am = file.i();

            for (int i = 0; i < am; i++)
            {
                int ci = file.i();
                Factory<?> c = constructors.Get(ci);
                HalfEntity e = c.Create();
                e = e.Load(file);
                e.hitBox.Load(file);
                Clear(e);
                e.Index = ents.Add(e);
                e.renderNext = null;
                map.Add(e);
            }
            foreach (Factory<?> f in constructors)
                f.Load(file);
        }

        private void Clear(HalfEntity e)
        {
            e.Index = -1;
            e.renderNext = null;
            e.gridX = -1;
            e.gridY = -1;
        }

        public override void Save(FilePutter file)
        {
            file.i(ents.Size());
            foreach (HalfEntity e in ents)
            {
                file.i(e.constructor().Index);
                e.Save(file);
                e.hitBox.Save(file);
            }
            foreach (Factory<?> f in constructors)
                f.Save(file);
        }

        protected override void Clear()
        {
            map.Clear();
            foreach (HalfEntity e in ents)
                Clear(e);
            ents.Clear();
            foreach (Factory<?> f in constructors)
                f.Clear();
        }

        void Add(HalfEntity e)
        {
            if (e.Index != -1)
                throw new RuntimeException();
            int i = ents.Add(e);
            Clear(e);
            e.Index = i;
            map.Add(e);
        }

        void Remove(HalfEntity e)
        {
            if (e.Index == -1)
                throw new RuntimeException();
            map.Remove(e);

            HalfEntity e2 = ents.Remove(ents.Size() - 1);

            if (e2 != e)
            {
                ents.Replace(e.Index, e2);
                e2.Index = e.Index;
            }
            Clear(e);
            e.constructor().returnT(e);
        }

        public override void Update(double ds, Profiler profiler)
        {
            for (int i = 0; i < ents.Size(); i++)
            {
                HalfEntity e = ents.Get(i);
                e.Update(ds);
                if (!e.Added())
                    i--;
                else
                    map.Move(e);
            }
        }

        public LIST<HalfEntity> All()
        {
            return ents;
        }

        public void Fill(COORDINATE coo, LISTE<HalfEntity> result)
        {
            Fill(coo.X(), coo.Y(), result);
        }

        public void Fill(int x, int y, LISTE<HalfEntity> result)
        {
            map.Fill(x, x, y, y, result);
        }

        public void Fill(int x1, int x2, int y1, int y2, LISTE<HalfEntity> result)
        {
            map.Fill(x1, x2, y1, y2, result);
        }

        public HalfEntity GetTallest(COORDINATE coo)
        {
            tmp.Clear();
            Fill(coo, tmp);
            HalfEntity tallest = null;
            double dist = double.MaxValue;
            foreach (HalfEntity e in tmp)
            {
                double d = coo.Distance(e.body().cX(), e.body().cY());
                if (tallest == null || d < dist)
                {
                    tallest = e;
                    dist = d;
                }
            }
            return tallest;
        }

        public void RenderInit(RECTANGLE renWindow)
        {
            renderables.Clear();
            int min = C.TILE_SIZE * 7;
            map.Fill(renWindow.x1() - min, renWindow.x2() + min, renWindow.y1() - min, renWindow.y2() + min, renderables);
            tmp.Clear();

            HalfEntity e;
            while (renderables.HasMore())
            {
                e = renderables.PollGreatest();
                tmp.Add(e);
            }
        }

        public void RenderBelow(Renderer r, ShadowBatch s, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            offX = offX - renWindow.x1();
            offY = offY - renWindow.y1();

            foreach (HalfEntity e in tmp)
            {
                e.RenderBelow(r, s, ds, e.body().x1() + offX, e.body().y1() + offY);
            }
        }

        public void Render(Renderer r, ShadowBatch s, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            offX = offX - renWindow.x1();
            offY = offY - renWindow.y1();

            COLOR.Unbind();

            foreach (HalfEntity e in tmp)
            {
                e.Render(r, s, ds, e.body().x1() + offX, e.body().y1() + offY);
            }
        }

        public void RenderAbove(Renderer r, ShadowBatch s, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            offX = offX - renWindow.x1();
            offY = offY - renWindow.y1();

            foreach (HalfEntity e in tmp)
            {
                e.RenderAbove(r, s, ds, e.body().x1() + offX, e.body().y1() + offY);
            }
        }

        public void RenderZoomed(Renderer r, ShadowBatch shadowBatch, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            foreach (HalfEntity e in All())
            {
                if (e == null)
                    continue;

                int x1 = e.body().cX();
                int y1 = e.body().cY();

                if (!renWindow.HoldsPoint(x1, y1))
                    continue;
                x1 -= (renWindow.x1() - offX) + C.TILE_SIZEH;
                y1 -= (renWindow.y1() - offY) + C.TILE_SIZEH;
                COLOR.BROWN.Bind();
                SPRITES.cons().TINY.high.Get(0).Render(r, x1, y1);
            }
            COLOR.Unbind();
        }
    }
}