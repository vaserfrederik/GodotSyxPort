using System;
using System.Collections.Generic;
using System.Linq;

namespace View.Sett.UI.Home
{
    public class UIHomeOdd : PlacableMulti
    {
        private static readonly string ¤¤name = "O-Mover";
        private static readonly string ¤¤desc = "Oddjobbers will automatically move out if an employed subject needs their home. This tool manually moves random oddjobbers to desired housing.";
        private static readonly string ¤¤prob = "Must be placed on a house with vacancies.";
        private static readonly string ¤¤odd = "No oddjobbers to move!";
        private static readonly string ¤¤oddNo = "There are no oddjobbers that can be moved into the specific house. They are either full, or their settings doesn't match the oddjobbers species and class.";

        static UIHomeOdd()
        {
            D.ts(typeof(UIHomeOdd));
        }

        private int updateTick;
        private int[,] oddjobbers;
        private int total;

        public UIHomeOdd() : base(¤¤name, ¤¤desc, new SPRITE.Twin(SPRITES.icons().m.workshop, SPRITES.icons().s.arrow_right))
        {
            oddjobbers = new int[HCLASSES.ALL().Count, RACES.all().Count];
        }

        public override string IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            if (updateTick != GAME.updateI())
            {
                updateTick = GAME.updateI();
                total = 0;
                for (int i = 0; i < oddjobbers.GetLength(0); i++)
                {
                    for (int j = 0; j < oddjobbers.GetLength(1); j++)
                    {
                        oddjobbers[i, j] = 0;
                    }
                }
                foreach (var c in HTYPES.ALL())
                {
                    if (!c.isWorks())
                        continue;
                    foreach (var r in RACES.all())
                    {
                        int am = STATS.POP().pop(r, c);

                        total += am;
                        oddjobbers[c.CLASS.index(), r.index()] += am;
                    }
                }

                foreach (var c in HCLASSES.ALL())
                {
                    foreach (var r in RACES.all())
                    {
                        int am = STATS.WORK().EMPLOYED.stat().data(c).get(r);

                        total -= am;
                        oddjobbers[c.index(), r.index()] -= am;
                    }
                }
            }

            if (total <= 0)
                return ¤¤odd;

            HomeInstance h = SETT.ROOMS().HOME.getter.get(tx, ty);
            if (h == null)
                return ¤¤prob;

            if (h.occupants() >= h.occupantsMax())
            {
                return ¤¤prob;
            }

            HTypeBits a = h.availability();

            if (a == null)
                return ¤¤prob;

            for (int ti = 0; ti < HGROUP.all().Count; ti++)
            {
                HGROUP t = HGROUP.all()[ti];
                if (t.race == null)
                {
                    continue;
                }
                if (oddjobbers[t.type.index(), t.race.index()] > 0)
                    return null;
            }

            return ¤¤oddNo;
        }

        public override void placeInfo(GBox b, int oktiles, AREA a)
        {
            base.placeInfo(b, oktiles, a);
        }

        int ie = 0;

        public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            HomeInstance h = SETT.ROOMS().HOME.getter.get(tx, ty);

            if (h == null)
                return;

            if (tx == h.serviceX() && ty == h.serviceY())
            {
                HTypeBits t = h.availability();
                if (t != null && h.occupants() < h.occupantsMax())
                {
                    ENTITY[] ee = SETT.ENTITIES().getAllEnts();
                    for (int i = 0; i < ee.Length; i++)
                    {
                        if (ie >= ee.Length)
                            ie = 0;
                        ENTITY e = ee[ie];
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (STATS.WORK().EMPLOYED.get(a) == null && t.is(a))
                            {
                                STATS.HOME().GETTER.set(a, h);
                                t = h.availability();
                                if (t == null || h.occupants() >= h.occupantsMax())
                                {
                                    break;
                                }
                            }
                        }
                        ie++;

                    }
                }
            }
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            return SETT.ROOMS().HOME.is(fromX, fromY) && SETT.ROOMS().map.get(fromX, fromY).isSame(fromX, fromY, toX, toY);
        }
    }
}