using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.GUTIL;
using util.gui.misc;
using util.rendering;

namespace settlement.thing
{
    public sealed class ThingsResources : ThingFactory<ScatteredResource>
    {
        public const int MAX_AMOUNT = 10000;
        private const int MAX = 4096 * 2;
        private readonly ScatteredResource[] all = new ScatteredResource[MAX];
        private readonly int[] hoverRes = Alloc.ii(RESOURCES.ALL().Count);
        private readonly int[] allclaimed = Alloc.ii(RESOURCES.ALL().Count);

        private RBITImp hasMask = new RBITImp();

        public ThingsResources(LISTE<ThingFactory<?>> alllllll) : base(alllllll, MAX)
        {
            for (int i = 0; i < all.Length; i++)
            {
                all[i] = new ScatteredResource(i);
            }
        }

        protected override ScatteredResource[] All => all;

        protected override void Save(FilePutter file)
        {
            base.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            hasMask.Clear();
            base.Load(file);
            foreach (ScatteredResource rr in all)
            {
                if (!rr.IsRemoved && rr.FindableReservedCanBe())
                    hasMask.Or(rr.Resource().Bit);
            }
        }

        public void RenderZoomed(Renderer r, RECTANGLE renWin, int offX, int offY)
        {
            foreach (ScatteredResource res in all)
            {
                if (!res.IsRemoved && renWin.HoldsPoint(res.Body().x1(), res.Body().y1()))
                {
                    res.Resource().MiniC().Bind();
                    SPRITES.cons().TINY.low.Render(r, 0, offX + res.Body().x1() - renWin.x1(), offY + res.Body().y1() - renWin.y1());
                }
            }
            COLOR.Unbind();
        }

        public void Create(int tx, int ty, RESOURCE r, int amount)
        {
            int i = 0;
            if (r == null)
                throw new RuntimeException();
            if (amount <= 0)
                return;

            while (GUTIL.circle().Radius(i) < 3 && amount > 0)
            {
                int x = tx + GUTIL.circle().Get(i).X();
                int y = ty + GUTIL.circle().Get(i).Y();
                if (Has(x, y, r.Bit))
                {
                    ScatteredResource t = (ScatteredResource)THINGS().Get(x, y);
                    amount = t.IncreaseAmount(amount);
                }
                i++;
            }
        }

        public void CreatePrecise(int tx, int ty, RESOURCE r, int amount)
        {
            if (amount > MAX_AMOUNT)
                amount = MAX_AMOUNT;

            if (amount <= 0)
                throw new RuntimeException("" + amount);

            ScatteredResource t = (ScatteredResource)THINGS().Get(tx, ty);
            t.IncreaseAmount(amount);
        }

        public void CreatePrecise(int tx, int ty, RESOURCE r, int amount, int random)
        {
            if (amount > MAX_AMOUNT)
                amount = MAX_AMOUNT;

            if (amount <= 0)
                throw new RuntimeException("" + amount);

            ScatteredResource t = (ScatteredResource)THINGS().Get(tx, ty);
            t.IncreaseAmount(amount, random);
        }

        public void RemoveUnreserved(int x, int y, int a)
        {
            ScatteredResource t = (ScatteredResource)THINGS().Get(x, y);
            t.RemoveUnreserved(a);
        }

        public bool Has(int x, int y, RBIT bit)
        {
            ScatteredResource t = (ScatteredResource)THINGS().Get(x, y);
            return t.Has(bit);
        }

        public ScatteredResource GetReservable(int x, int y, RBIT bit)
        {
            ScatteredResource t = (ScatteredResource)THINGS().Get(x, y);
            return t.GetReservable(bit);
        }

        public bool AnyReservable(int x, int y, RBIT bit)
        {
            ScatteredResource t = (ScatteredResource)THINGS().Get(x, y);
            return t.AnyReservable(bit);
        }

        public override ThingFactory<?> Factory => SETT.THINGS().resources;

        public class ScatteredResource : Thing, IResource, IHoverable
        {
            private readonly RECTANGLE body;
            private int resourceIndex;
            private short amount;
            private short claimed;
            private readonly int random;

            public ScatteredResource(int index) : base(index)
            {
                body = new RECTANGLE(C.TILE_SIZE, C.TILE_SIZE);
                random = rnd.nextInt();
            }

            protected override void Render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY)
            {
                int x = body.x1() + offsetX;
                int y = body.y1() + offsetY;
                if (amount <= 0)
                {
                    UI.icons().s.cancel.Render(r, x, y);
                    return;
                }
                Resource().RenderLaying(r, x, y, random, amount);
                shadows.SetDistance2Ground(1).SetHeight(0);
                Resource().RenderLaying(shadows, x, y, random, amount);
            }

            private int IncreaseAmount(int amount)
            {
                if (amount == 0)
                    return 0;
                if (this.amount == MAX_AMOUNT)
                    return amount;

                bool res = FindableReservedCanBe();
                int ret = 0;

                int a = this.amount + amount;

                if (a > MAX_AMOUNT)
                {
                    ret = a - MAX_AMOUNT;
                    a = MAX_AMOUNT;
                }
                this.amount = (short)a;
                if (!res)
                {
                    PATH().finders.resource.ReportPresence(this);
                    Evaluate(x(), y());
                }
                return ret;
            }

            private int IncreaseAmount(int amount, int random)
            {
                if (amount == 0)
                    return 0;
                if (this.amount == MAX_AMOUNT)
                    return amount;

                bool res = FindableReservedCanBe();
                int ret = 0;

                int a = this.amount + amount;

                if (a > MAX_AMOUNT)
                {
                    ret = a - MAX_AMOUNT;
                    a = MAX_AMOUNT;
                }
                this.amount = (short)a;
                this.random = random;
                if (!res)
                {
                    PATH().finders.resource.ReportPresence(this);
                    Evaluate(x(), y());
                }
                return ret;
            }

            protected override void AddAction()
            {
                Evaluate(x(), y());
            }

            public RECTANGLE Body => body;

            public RESOURCE Resource => RESOURCES.ALL()[resourceIndex];

            public int x() => body.cX() >> C.T_SCROLL;

            public int y() => body.cY() >> C.T_SCROLL;

            public void FindableReserve()
            {
                if (claimed < amount)
                {
                    claimed++;
                    if (claimed == amount)
                    {
                        PATH().finders.resource.ReportAbsence(this);
                        Evaluate(x(), y());
                    }
                }
                else
                {
                    GAME.Error(Debug());
                }
            }

            private string Debug()
            {
                return Resource().name + " " + x() + " " + y() + " amount:" + amount + " claimed:" + claimed;
            }

            public void FindableReserveCancel()
            {
                if (claimed == 0)
                    return;
                if (claimed == amount)
                {
                    claimed--;
                    PATH().finders.resource.ReportPresence(this);
                    Evaluate(ctx(), cty());
                }
                else
                {
                    claimed--;
                }
            }

            public void ResourcePickup()
            {
                if (claimed == 0)
                    GAME.Error(Debug());
                claimed--;
                amount--;
                if (amount < claimed)
                    GAME.Error(Debug());
                if (amount == 0)
                {
                    Remove();
                }
            }

            public void RemoveUnreserved(int a)
            {
                if (claimed < amount)
                {
                    claimed += a;
                    if (claimed > amount)
                        throw new RuntimeException();
                    if (claimed == amount)
                    {
                        PATH().finders.resource.ReportAbsence(this);
                        Evaluate(x(), y());
                    }
                    amount -= a;
                    claimed -= a;
                    if (amount == 0)
                        Remove();
                }
                else
                {
                    GAME.Error(Debug());
                }
            }

            public bool FindableReservedIs()
            {
                return claimed > 0;
            }

            public int AmountReserved()
            {
                return claimed;
            }

            public int Amount => amount;

            public int Reservable => amount - claimed;

            public bool FindableReservedCanBe()
            {
                return amount > 0 && claimed < amount;
            }

            public void Hover(GBox box)
            {
                for (int i = 0; i < hoverRes.Length; i++)
                {
                    hoverRes[i] = 0;
                    allclaimed[i] = 0;
                }

                foreach (Thing t in THINGS().Get(ctx(), ctx() + 1, cty(), cty() + 1))
                {
                    if (t is ScatteredResource r)
                    {
                        if (r.Body.cX() != this.body.cX() || r.Body.cY() != this.body.cY())
                            continue;
                        hoverRes[r.resourceIndex] += r.amount;
                        allclaimed[r.resourceIndex] += r.claimed;
                    }
                }

                for (int i = 0; i < hoverRes.Length; i++)
                {
                    if (hoverRes[i] != 0)
                    {
                        box.text(RESOURCES.ALL()[i].name);
                        box.setResource(RESOURCES.ALL()[i], hoverRes[i]);
                        if (S.get().developer)
                        {
                            GText text = box.text();
                            text.add(' ').add(allclaimed[i]);
                            text.add(' ').add(Has(x(), y(), RESOURCES.ALL()[i].Bit));
                            text.add(' ').add(GetReservable(x(), y(), RESOURCES.ALL()[i].Bit) != null);
                            box.add(text);
                        }
                        box.NL();
                    }
                }
            }

            public bool CanBeClicked()
            {
                return false;
            }

            protected override void RemoveAction()
            {
                if (claimed < amount)
                {
                    PATH().finders.resource.ReportAbsence(this);
                    claimed = 0;
                    amount = 0;

                    Evaluate(x(), y());
                    resourceIndex = -1;
                }
            }

            protected override int Z => 99;

            public override ThingFactory<?> Factory => SETT.THINGS().resources;

            public override bool IsStorage => false;

            public override bool IsPrio => false;

            public bool Has(RBIT bit)
            {
                return Resource().Bit.Has(bit);
            }

            public ScatteredResource GetReservable(RBIT bit)
            {
                if (FindableReservedCanBe() && Resource().Bit.Has(bit))
                    return this;
                return null;
            }

            public bool AnyReservable(RBIT bit)
            {
                return FindableReservedCanBe() && Resource().Bit.Has(bit);
            }
        }
    }
}