using System;
using System.Collections.Generic;

namespace Game.Battle.State
{
    public static class BattleStateArt
    {
        public static int PlaceArt(int cx, int cy, int depth, DIR dir, LIST<ROOM_ARTILLERY> art, Race race, bool player)
        {
            depth += 10;

            dir = dir.Perpendicular();
            int am = art.Size() - 1;
            for (int d = 0; d < 32 && am >= 0; d += 8)
            {
                int dx = cx + dir.Perpendicular().X() * depth;
                int dy = cy + dir.Perpendicular().Y() * depth;

                for (int w = 0; w < 128; w += 4)
                {
                    for (int i = -1; i <= 1; i += 2)
                    {
                        int x = dx + dir.Next(2).X() * i * w;
                        int y = dy + dir.Next(2).Y() * i * w;
                        ROOM_ARTILLERY a = art.Get(am);
                        if (Deploy(x, y, dir, a, race, player))
                        {
                            am--;
                            if (am < 0)
                            {
                                return depth;
                            }
                        }
                    }
                }
                depth += 8;
            }
            return depth;
        }

        private static bool Deploy(int sx, int sy, DIR d, ROOM_ARTILLERY art, Race race, bool player)
        {
            if (!d.IsOrtho())
                d = d.Next((int)RND.RSign());

            int index = -1;
            for (int i = 0; i < DIR.ORTHO.Size(); i++)
            {
                if (DIR.ORTHO.Get(i) == d)
                    index = i;
            }

            art.Eplacer.RotSet(index);

            for (int y = 0; y < art.Eplacer.Height(); y++)
            {
                for (int x = 0; x < art.Eplacer.Width(); x++)
                {
                    if (SETT.ENTITIES().AmountAtTile(sx + x, sy + y) > 0)
                        return false;
                    if (art.Eplacer.Placable(sx + x, sy + y, x, y) != null)
                        return false;
                }
            }

            if (art.Eplacer.PlacableWhole(sx, sy) != null)
                return false;

            for (int y = 0; y < art.Eplacer.Height(); y++)
            {
                for (int x = 0; x < art.Eplacer.Width(); x++)
                {
                    art.Eplacer.Place(sx + x, sy + y, x, y);
                }
            }
            art.Eplacer.AfterPlaced(sx, sy);

            ArtilleryInstance r = art.Getter.Get(sx, sy);

            if (r == null)
                return false;

            if (!player)
                r.SetEnemy();

            r.Muster(true);
            r.FireAtWill(true);
            r.SetVisible();
            int am = art.Services;
            foreach (COORDINATE c in r.Body())
            {
                if (r.Is(c) && SETT.ROOMS().GetAvailability(c.X(), c.Y()) == AVAILABILITY.ROOM)
                {
                    if (am > 0)
                        new Humanoid(c.X() * C.TILE_SIZE + C.TILE_SIZEH, c.Y() * C.TILE_SIZE + C.TILE_SIZEH, race, player ? HTYPES.SUBJECT() : HTYPES.ENEMY(), CAUSE_ARRIVES.SOLDIER_RETURN());
                    am--;
                }
            }

            return true;
        }
    }
}