using System;
using System.Collections.Generic;
using System.Linq;

namespace view.sett.ui.bottom
{
    public class RoomUpgrader : PlacableMulti
    {
        private static readonly string ¤¤name = "Room upgrade";
        private static readonly string ¤¤desc = "Upgrade the rooms that can be upgraded.";

        private static readonly string ¤¤UPGRADE_MAX_REACHED = "¤Maximally Upgraded.";
        private static readonly string ¤¤RESOURCES = "¤Not enough resources.";
        private static readonly string ¤¤noSelect = "¤No rooms selected that can be upgraded.";

        private readonly int[] resources;
        private string error;
        private readonly double iunprocessed = -2;
        private readonly double ierror = -1;
        private readonly double iok = 0;
        private bool any = false;

        static RoomUpgrader()
        {
            D.ts(typeof(RoomUpgrader));
        }

        public RoomUpgrader() : base(¤¤name, ¤¤desc, UI.icons().l.upgrade)
        {
            resources = new int[RESOURCES.ALL().Count];
        }

        public override string IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            Room rr = SETT.ROOMS().map.get(tx, ty);
            if (rr == null)
                return E;
            return null;
        }

        public override string IsPlacable(AREA area, PLACER_TYPE type)
        {
            foreach (COORDINATE c in area.body())
            {
                int tx = c.x();
                int ty = c.y();
                if (area.is(tx, ty))
                {
                    Room rr = SETT.ROOMS().map.get(tx, ty);
                    if (rr == null)
                        continue;
                    GUTIL.flooder().setValue2(rr.mX(tx, ty), rr.mY(tx, ty), iunprocessed);
                }
            }
            resources.Fill(0);
            error = null;
            any = false;
            bool room = false;
            outer:
            foreach (COORDINATE c in area.body())
            {
                if (area.is(c))
                {
                    int tx = c.x();
                    int ty = c.y();
                    Room rr = SETT.ROOMS().map.get(tx, ty);
                    if (rr == null)
                        continue;
                    int mx = rr.mX(tx, ty);
                    int my = rr.mY(tx, ty);

                    if (GUTIL.flooder().getValue2(mx, my) != iunprocessed)
                        continue;
                    GUTIL.flooder().setValue2(mx, my, ierror);

                    string e = CanUpgrade(rr, mx, my);
                    if (e != null)
                    {
                        if (error == null)
                        {
                            error = e;
                        }
                        continue;
                    }

                    bool can = true;
                    room = true;
                    for (int ri = 0; ri < Resources(rr); ri++)
                    {
                        int am = ResAm(rr, tx, ty, ri);
                        RESOURCE res = Res(rr, ri);
                        resources[res.index()] += am;

                        if (SETT.ROOMS().STOCKPILE.tally().amountReservable.get(res) < resources[res.index()])
                        {
                            can = false;
                        }
                    }
                    if (!can)
                    {
                        if (error == null)
                        {
                            error = ¤¤RESOURCES;
                        }
                        continue outer;
                    }

                    any = true;
                    GUTIL.flooder().setValue2(mx, my, iok);
                }
            }

            if (!room && error == null)
                error = ¤¤noSelect;

            return null;
        }

        private RESOURCE Res(Room r, int ri)
        {
            return r.constructor().resource(ri);
        }

        private int Resources(Room r)
        {
            return r.constructor().resources();
        }

        private int ResAm(Room rr, int tx, int ty, int ri)
        {
            int current = rr.upgrade(tx, ty);
            return rr.resAmount(ri, current + 1) - rr.resAmount(ri, current);
        }

        public override void RenderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA area, PLACER_TYPE type, bool isPlacable, bool areaIsPlacable)
        {
            isPlacable = false;
            areaIsPlacable = true;
            Room rr = SETT.ROOMS().map.get(tx, ty);
            if (rr != null)
            {
                int mx = rr.mX(tx, ty);
                int my = rr.mY(tx, ty);
                if (GUTIL.flooder().getValue2(mx, my) == iok)
                    isPlacable = true;
                else if (error == ¤¤noSelect)
                    areaIsPlacable = false;
            }

            if (!isPlacable)
                GCOLOR.MAP().BAD.bind();
            else if (!areaIsPlacable)
                GCOLOR.MAP().SOSO.bind();
            else
                GCOLOR.MAP().BEST.bind();

            base.RenderPlaceHolder(r, mask, x, y, tx, ty, area, type, isPlacable, areaIsPlacable);
        }

        public override void PlaceInfo(GBox box, int oktiles, AREA area)
        {
            foreach (RESOURCE res in RESOURCES.ALL())
            {
                if (resources[res.index()] > 0)
                {
                    box.add(res.icon());
                    box.add(GFORMAT.iofk(box.text(), resources[res.index()], SETT.ROOMS().STOCKPILE.tally().amountReservable.get(res)));
                    box.NL();
                }
            }
            if (!any)
                box.error(error);
            else if (error != null)
            {
                box.add(box.text().warnify().add(error));
            }
        }

        public override void UpdateRegardless(GameWindow window, AREA selected)
        {
            resources.Fill(0);
            base.UpdateRegardless(window, selected);
        }

        public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            Room rr = SETT.ROOMS().map.get(tx, ty);
            if (rr == null)
                return;
            if (rr.mX(tx, ty) != tx || rr.mY(tx, ty) != ty)
                return;
            if (CanUpgrade(rr, tx, ty) != null)
                return;

            int current = rr.upgrade(tx, ty);

            for (int ri = 0; ri < Resources(rr); ri++)
            {
                int am = ResAm(rr, tx, ty, ri);
                RESOURCE res = Res(rr, ri);

                if (SETT.ROOMS().STOCKPILE.tally().amountReservable.get(res) < am)
                {
                    return;
                }
            }

            for (int ri = 0; ri < Resources(rr); ri++)
            {
                int am = ResAm(rr, tx, ty, ri);
                RESOURCE res = Res(rr, ri);

                res.remove(am, RTYPE.CONSTRUCTION);
            }

            rr.upgradeSet(tx, ty, current + 1);
        }

        private string CanUpgrade(Room rr, int tx, int ty)
        {
            if (!(rr.blueprint() is RoomBlueprintImp))
                return E;
            RoomBlueprintImp b = (RoomBlueprintImp)rr.blueprint();
            if (b.upgrades().max() == 0)
                return ¤¤UPGRADE_MAX_REACHED;
            int current = rr.upgrade(tx, ty);
            if (current >= b.upgrades().max())
                return ¤¤UPGRADE_MAX_REACHED;
            foreach (Lock<Faction> r in b.upgrades().requires(current + 1).all())
            {
                if (!r.unlocker.inUnlocked(FACTIONS.player()))
                {
                    return Str.TMP.clear().add(Dic.¤¤Requires).add(':').s().add(r.unlocker.name);
                }
            }
            return null;
        }

        public override bool ExpandsTo(int fromX, int fromY, int toX, int toY)
        {
            Room rr = SETT.ROOMS().map.get(fromX, fromY);
            if (rr == null)
                return false;
            if (!rr.isSame(fromX, fromY, toX, toY))
                return false;
            if (rr.blueprint() is RoomBlueprintImp)
            {
                RoomBlueprintImp b = (RoomBlueprintImp)rr.blueprint();
                if (b.upgrades().max() == 0)
                    return false;
                int current = rr.upgrade(fromX, fromY);
                if (current >= b.upgrades().max())
                    return false;
                return true;
            }
            return false;
        }
    }
}