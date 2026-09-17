using System;
using System.Linq;

namespace Settlement.Room.Main.Util
{
    public sealed class Deleter : PlacableMulti
    {
        private static readonly string ¤¤name = "Dismantle Room";
        private static readonly string ¤¤desc = "Dismantle room, recovering some of the resources used to build it. Cannot be undone.";

        static Deleter()
        {
            D.ts(typeof(Deleter));
        }

        public Deleter(ROOMS m) : base(¤¤name, ¤¤desc, SPRITES.icons().m.cancel.resized(IconS.L).twin(UI.icons().m.ceiling, DIR.NE, 1))
        {
        }

        public override string IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            return CanRemove(tx, ty) ? null : "";
        }

        public static bool CanRemove(int tx, int ty)
        {
            if (ROOMS().THRONE.Is(tx, ty))
                return false;
            Room r = ROOMS().map.Get(tx, ty);
            if (r == null)
                return false;
            if (r is ArtilleryInstance)
            {
                ArtilleryInstance i = (ArtilleryInstance)r;
                return i.Army() == GAME.ARMIES().player();
            }
            return true;
        }

        public override void Place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (ROOMS().THRONE.Is(tx, ty))
                return;
            Room r = ROOMS().map.Get(tx, ty);
            if (r == null)
                return;

            if (r is ArtilleryInstance)
            {
                ArtilleryInstance i = (ArtilleryInstance)r;
                if (i.Army() != GAME.ARMIES().player())
                    return;
            }
            bool rem = SETT.ROOMS().construction.isser.Is(tx, ty);
            TmpArea ar = r.Remove(tx, ty, true, this, false);
            if (!rem)
                ar.SetRemoveFloor();
            if (ar != null)
                ar.Clear();
        }

        public override bool CanBePlacedAs(PLACER_TYPE t)
        {
            return t == PLACER_TYPE.BRUSH || t == PLACER_TYPE.SQUARE;
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            if (ROOMS().THRONE.Is(fromX, fromY))
                return false;

            return ROOMS().map.Is(fromX, fromY) && ROOMS().map.Get(fromX, fromY).IsSame(fromX, fromY, toX, toY);
        }

        private static readonly double[] amountsd = new double[Furnisher.MAX_RESOURCES];
        private static readonly int[] amountsi = new int[Furnisher.MAX_RESOURCES];

        private static int[] GetResources(AREA r, Furnisher furnisher, int upgrade, double degrade)
        {
            for (int i = 0; i < amountsd.Length; i++)
            {
                amountsd[i] = 0;
                amountsi[i] = 0;
            }
            foreach (COORDINATE c in r.Body())
            {
                if (!r.Is(c))
                    continue;

                if (ROOMS().fData.isMaster.Is(c))
                {
                    FurnisherItem it = ROOMS().fData.item.Get(c);
                    for (int i = 0; i < furnisher.Resources(); i++)
                    {
                        amountsd[i] += it.Cost2(i, upgrade);
                    }
                }
            }

            degrade = CLAMP.d(1.0 - degrade, 0, 1);
            for (int i = 0; i < furnisher.Resources(); i++)
            {
                amountsd[i] += Math.Ceiling(r.Area() * furnisher.AreaCost(i, upgrade));
                double mm = amountsd[i] * 0.75 * degrade;
                amountsi[i] = (int)mm;
                if (amountsd[i] - amountsi[i] > RND.rFloat())
                    amountsi[i]++;
            }
            return amountsi;
        }

        public static void ScatterMaterials(AREA r, Furnisher furnisher, int upgrade, double degrade)
        {
            GetResources(r, furnisher, upgrade, degrade);

            int resAll = 0;
            int resPiles = 0;
            for (int i = 0; i < furnisher.Resources(); i++)
            {
                resAll += amountsi[i];
                resPiles += (int)Math.Ceiling(amountsi[i] / 32.0);
            }

            if (resPiles == 0)
                return;

            double resPerPile = (double)resAll / r.Area();
            double am = 0;

            foreach (COORDINATE c in r.Body())
            {
                if (!r.Is(c))
                    continue;
                am += resPerPile;
                if (am >= 1)
                {
                    int di = RND.rInt(furnisher.Resources());
                    for (int i = 0; i < furnisher.Resources() && am >= 1; i++)
                    {
                        int ri = (di + i) % furnisher.Resources();
                        if (amountsi[ri] > 0)
                        {
                            int a = CLAMP.i(amountsi[ri], 0, (int)am);
                            THINGS().resources.Create(c, furnisher.Resource(ri), a);
                            GAME.player().res().Inc(furnisher.Resource(ri), RTYPE.CONSTRUCTION, a);
                            am -= a;
                            amountsi[ri] -= a;
                        }
                    }
                }
            }
        }
    }
}