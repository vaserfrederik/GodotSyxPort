using System;
using System.Collections.Generic;
using init.resources;
using settlement.room.food.hunter;
using settlement.room.industry.module;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.room.tests.production
{
    public sealed class Production
    {
        private readonly Li all = new Li();
        private readonly Li[] resMap = new Li[RESOURCES.ALL().Size()];
        private readonly Li[] insMap = new Li[Industry.All().Size()];

        public Production()
        {
            int[] industrycount = Alloc.Ii(Industry.All().Size());

            foreach (RESOURCE res in RESOURCES.ALL())
            {
                resMap[res.Index()] = new Li();
            }

            Li allList = new Li();
            foreach (Industry ins in Industry.All())
            {
                insMap[ins.Index()] = new Li();
                if (ins.Outs().Size() == 0)
                    continue;
                if (ins.Blue is ROOM_HUNTER)
                    continue;

                foreach (IndustryResource o in ins.Outs())
                {
                    TestRecipe r = new TestRecipe(industrycount[ins.Index()]++, ins, o);
                    allList.Add(r);
                    resMap[r.Res.Index()].Add(r);
                }
            }

            foreach (TestRecipe r in allList)
            {
                PopulateInputs(r, resMap, industrycount);
            }

            foreach (RESOURCE res in RESOURCES.ALL())
            {
                this.all.Add(resMap[res.Index()]);
            }

            foreach (TestRecipe r in allList)
            {
                insMap[r.Ins.Index()].Add(r);
            }
        }

        private void PopulateInputs(TestRecipe r, Li[] resMap, int[] industrycount)
        {
            foreach (IndustryResource inResource in r.Ins.Ins())
            {
                foreach (TestRecipe prod in resMap[inResource.Resource.Index()])
                {
                    PopulateInputs(prod, resMap, industrycount);
                }
            }

            if (r.Inputs.Size() > 0)
                return;

            foreach (IndustryResource inResource in r.Ins.Ins())
            {
                Input i = new Input(inResource.Rate, resMap[inResource.Resource.Index()].Get(0));
                r.Inputs.Add(i);
            }

            int extras = 1;
            foreach (IndustryResource inResource in r.Ins.Ins())
            {
                extras *= resMap[inResource.Resource.Index()].Size();
            }

            ArrayListGrower<TestRecipe> newRecs = new ArrayListGrower<TestRecipe>();

            for (int i = 1; i < extras; i++)
            {
                int k = i;

                ArrayListGrower<Input> inputs = new ArrayListGrower<Input>();

                foreach (IndustryResource in2 in r.Ins.Ins())
                {
                    int ri = k % resMap[in2.Resource.Index()].Size();

                    TestRecipe rr = resMap[in2.Resource.Index()].Get(ri);
                    inputs.Add(new Input(in2.Rate, rr));

                    k /= resMap[in2.Resource.Index()].Size();
                }

                TestRecipe res = new TestRecipe(industrycount[r.Ins.Index()]++, r);
                res.Inputs.Add(inputs);
                newRecs.Add(res);
            }

            resMap[r.Res.Index()].Add(newRecs);
        }

        public LIST<TestRecipe> All()
        {
            return all;
        }

        public LIST<TestRecipe> Get(RESOURCE res)
        {
            return resMap[res.Index()];
        }

        public TestRecipe Best(RESOURCE resource, ProductionSpec ibonuses)
        {
            LIST<TestRecipe> rs = Get(resource);
            TestRecipe r = null;
            double best = double.MaxValue;
            for (int ri = 0; ri < rs.Size(); ri++)
            {
                TestRecipe r2 = rs.Get(ri);
                double w = r2.WPerItem(ibonuses);
                if (w < best)
                {
                    best = w;
                    r = r2;
                }
            }
            return r;
        }

        public LIST<TestRecipe> Get(Industry res)
        {
            return insMap[res.Index()];
        }

        public double Price(RESOURCE res, ProductionSpec ibonuses)
        {
            LIST<TestRecipe> rs = Get(res);
            double min = double.MaxValue;

            foreach (TestRecipe rr in rs)
            {
                double t = rr.PricePerItem(ibonuses);
                if (t < min)
                {
                    min = t;
                }
            }
            return min;
        }

        private sealed class Li : ArrayListGrower<TestRecipe>
        {
            private static readonly long serialVersionUID = 1L;
        }
    }
}