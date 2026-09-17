using System;
using game.battle;
using init.constant;
using init.race;
using settlement.main;
using settlement.path;
using snake2d.util.datatypes;

namespace game.battle.formation
{
    public static class DivPlacability
    {
        private static readonly DIR[] dirIter = new DIR[] { DIR.NW, DIR.NE, DIR.SE, DIR.SW };

        public static bool PixelIsBlocked(int x1, int y1, Race race, Army defender)
        {
            int tileSize = race.physics.hitBoxsize() / 2;
            foreach (DIR d in dirIter)
            {
                int tx = (x1 + d.x() * (tileSize)) >> C.T_SCROLL;
                int ty = (y1 + d.y() * (tileSize)) >> C.T_SCROLL;
                if (!TileIsOK(tx, ty, defender))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PixelIsBlocked(int x1, int y1, int dist, Army defender)
        {
            int tileSize = dist / 2;
            foreach (DIR d in dirIter)
            {
                int tx = (x1 + d.x() * (tileSize)) >> C.T_SCROLL;
                int ty = (y1 + d.y() * (tileSize)) >> C.T_SCROLL;
                if (!TileIsOK(tx, ty, defender))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool TileIsOK(int tx, int ty, Army defender)
        {
            AVAILABILITY a = SETT.PATH().availability.get(tx, ty);
            if (a == null)
                return false;
            return !a.isSolid(defender);
        }

        public static bool CheckPixelStep(int fx, int fy, int tox, int toy, Race tz, Army defender)
        {
            if (PixelIsBlocked(fx, fy, tz, defender))
                return false;
            if (PixelIsBlocked(tox, toy, tz, defender))
                return false;
            if (PixelIsBlocked(fx, toy, tz, defender))
                return false;
            if (PixelIsBlocked(tox, fy, tz, defender))
                return false;
            return true;
        }

        public static bool CheckStep(int fx, int fy, int tx, int ty, Army defender)
        {
            if (!TileIsOK(tx, ty, defender))
                return false;

            if (tx != fx || ty != fy)
                if (!TileIsOK(tx, fy, defender) || !TileIsOK(fx, ty, defender))
                    return false;

            return true;
        }
    }
}