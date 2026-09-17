using System;
using settlement.main;
using game.faction;
using init.resources;
using settlement.tilemap.floor;
using snake2d.util.rnd;

namespace settlement.maintenance
{
    sealed class MFloor : MType
    {
        public MFloor()
        {
        }

        public override bool Degrade(int tx, int ty, int tile, double rate)
        {
            Floor f = FLOOR().getter.get(tx, ty);
            if (f == null)
            {
                return false;
            }
            if (PATH().solidity.is(tx, ty))
            {
                return false;
            }
            if (SETT.ROOMS().map.is(tx, ty))
            {
                return false;
            }
            double r = rate;
            r *= (SETT.MAINTENANCE().tilesPerDay + 0.25 * SETT.MAINTENANCE().resRate * f.resAmount);
            r *= (1.0 - f.durability);
            if (RND.rFloat() < r)
            {
                if (FLOOR().degrade(tx, ty) > 0)
                    FLOOR().degradeInc(tile, 1 + RND.rInt(3));
                else
                    FLOOR().degradeInc(tile, 1);
                if (RND.oneIn(2))
                    SETT.GRASS().grow(tx, ty);
            }
            return true;
        }

        public override void Vandalize(int tx, int ty)
        {
            FLOOR().degradeInc(tx + TWIDTH * ty, 3 + RND.rInt(2));
        }

        public override int ShouldPlaceResource(int tx, int ty)
        {
            Floor f = SETT.FLOOR().getter.get(tx, ty);

            double res = 0.25 * SETT.MAINTENANCE().resRate * f.resAmount;
            double tot = res + SETT.MAINTENANCE().tilesPerDay;

            if (RND.rFloat() * tot < res)
                return 1;
            return 0;
        }

        public override bool Validate(int tx, int ty)
        {
            Floor f = FLOOR().getter.get(tx, ty);
            if (f != null && f.isRoad && !PATH().solidity.is(tx, ty) && !SETT.ROOMS().map.is(tx, ty))
            {
                return true;
            }
            return false;
        }

        public override void Maintain(int tx, int ty)
        {
            if (FLOOR().getter.is(tx, ty))
            {
                int i = tx + ty * TWIDTH;
                FLOOR().degradeInc(i, -FLOOR().degrade(tx, ty));
                GRASS().current.set(tx, ty, 0);
            }
        }

        public override RESOURCE Res(int tx, int ty, int ri)
        {
            if (ri > 0)
                return SETT.FLOOR().getter.get(tx, ty).resource;
            return null;
        }

        public override bool ShouldPlace(int tx, int ty, bool was)
        {
            Floor f = FLOOR().getter.get(tx, ty);
            if (f != null && !f.reqs.passes(FACTIONS.player()))
                return false;
            return FLOOR().degrade(tx, ty) > 0;
        }

        public override double ResRate(int tx, int ty, int ri)
        {
            if (ri != 1)
            {
                return 0;
            }
            if (!Validate(tx, ty))
                return 0;
            Floor f = FLOOR().getter.get(tx, ty);
            return 0.25 * f.resAmount * SETT.MAINTENANCE().resRate * (1.0 - f.durability);
        }

        public override double Degrade(int tx, int ty)
        {
            return FLOOR().degrade.get(tx, ty);
        }
    }
}