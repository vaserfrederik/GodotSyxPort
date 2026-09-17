using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using world.entity.haven;

namespace world.entity
{
    final class Generator
    {
        public Generator()
        {
            WHavens cc = WORLD.ENTITIES().havens;

            List<WW> spots = new List<WW>(cc.types.size());
            for (int i = 0; i < cc.types.size(); i++)
                spots.Add(new WW(cc.types.get(i)));


            foreach (COORDINATE c in TBOUNDS())
            {
                if (!REGIONS().map.is(c))
                    continue;
                if (WORLD.REGIONS().isCentre.is(c))
                    continue;
                if (WORLD.MOUNTAIN().haser.is(c.x(), c.y()))
                    continue;
                if (WORLD.WATER().has.is(c))
                    continue;
                if (WORLD.FOREST().amount.get(c) > 0.25)
                    continue;

                CLIMATE cl = CLIMATE().getter.get(c);
                foreach (WW w in spots)
                {
                    w.add(c.x(), c.y(), cl);
                }
            }

            int[] ni = Alloc.ii(cc.types.size());

            other:
            while (!spots.isEmpty())
            {
                WW w = spots.get(RND.rInt(spots.size()));
                if (w.am <= 0 || !w.spots.hasMore())
                {
                    spots.remove(w);
                    continue;
                }

                Coovalue c = w.spots.pollGreatest();

                foreach (DIR d in DIR.ALLC)
                {
                    if (WORLD.ENTITIES().havens.fillTile(c.tx + d.x(), c.ty + d.y()).size() > 0)
                    {
                        continue other;
                    }
                }

                Str.TMP.clear().add(w.w.names.getC(ni[w.w.index()]++)).insert(0, w.w.race.appearance().lastNamesNoble.getC(RND.rInt(0x0FFFF)));

                cc.create(c.tx, c.ty, w.w, RND.rFloat(), Str.TMP);
                w.am--;
            }
        }

        private class WW
        {
            double am = 0;
            private readonly Tree<Coovalue> spots = new Tree<Coovalue>(1024)
            {
                protected override bool isGreaterThan(Coovalue current, Coovalue cmp)
                {
                    return current.value > cmp.value;
                }
            };
            public readonly WHavenType w;

            WW(WHavenType w)
            {
                this.w = w;
            }

            void add(int tx, int ty, CLIMATE cl)
            {
                double res = 0;
                foreach (TERRAIN t in TERRAINS.ALL())
                {
                    res += w.climates[cl.index()] * w.terrains[t.index()] * t.value(tx, ty);
                }
                am += res;

                if (!spots.hasRoom())
                {
                    if (spots.smallest().value < res)
                        spots.pollSmallest();
                    else
                        return;
                }

                Coovalue v = new Coovalue();
                v.value = res * RND.rFloat();
                v.tx = (short)tx;
                v.ty = (short)ty;
                spots.add(v);
            }
        }

        private class Coovalue
        {
            public short tx;
            public short ty;
            public double value;
        }
    }
}