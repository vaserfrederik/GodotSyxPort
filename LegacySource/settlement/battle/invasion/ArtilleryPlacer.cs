using System;
using System.Collections.Generic;
using game;
using game.battle.div;
using game.battle.util;
using game.faction;
using init.constant;
using init.race;
using init.type;
using settlement.battle.invasion.SpotMaker;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path.components;
using settlement.room.main.throne;
using settlement.room.military.artillery;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util;

internal static class ArtilleryPlacer
{
    internal static bool PlaceArt(LIST<DivGeneration> divs, InvasionSpot spot, int amount)
    {
        Mark();
        PathTile t = Find(spot);
        if (t == null)
            return false;
        PathTile target = Rewind(t, 2);
        if (target == null)
            return false;
        PathTile safe = Rewind(t, 1);
        if (safe == null)
            return false;
        PathTile dest = FindPos(spot, safe);
        if (dest == null)
            return false;
        Race r = GetRace(divs);
        return Deploy(r, spot, dest, DIR.Get(dest, target), amount);
    }

    private static Race GetRace(LIST<DivGeneration> divs)
    {
        int[] amount = Alloc.Ii(RACES.All().Size());
        foreach (DivGeneration d in divs)
        {
            amount[d.Race().Index] += d.Indus.Length;
        }
        if (divs.Size() == 0)
            return FACTIONS.Player().Race();
        Race best = divs.Get(0).Race();
        int bestV = 0;
        foreach (Race r in RACES.Playable())
        {
            if (amount[r.Index] > bestV)
            {
                best = r;
                bestV = amount[r.Index];
            }
        }
        return best;
    }

    private static void Mark()
    {
        Flooder f = GUTIL.Flooder();
        f.Init(ArtilleryPlacer.GetType());

        foreach (COORDINATE c in SETT.TILE_BOUNDS)
        {
            f.SetValue2(c, 0);
        }

        foreach (ROOM_ARTILLERY b in SETT.ROOMS().ARTILLERY)
        {
            for (int i = 0; i < b.InstancesSize(); i++)
            {
                ArtilleryInstance ins = b.GetInstance(i);
                if (ins.Army() == GAME.ARMIES().Enemy())
                {
                    f.PushGreater(ins.Body().CX(), ins.Body().CY(), ins.RangeMax() - 32);
                    f.SetValue2(ins.Body().CX(), ins.Body().CY(), 2);
                }
            }
        }

        f.PushGreater(THRONE.COO().X(), THRONE.COO().Y(), 48);
        f.SetValue2(THRONE.COO().X(), THRONE.COO().Y(), 2);

        foreach (Div d in GAME.ARMIES().Player().Divisions())
        {
            if (d.Active())
            {
                for (int i = 0; i < d.Current().Deployed(); i++)
                {
                    int tx = d.Current().Tile(i).X();
                    int ty = d.Current().Tile(i).Y();
                    if (SETT.IN_BOUNDS(tx, ty))
                    {
                        f.PushGreater(tx, ty, 128);
                        f.SetValue2(tx, ty, 2);
                    }
                }
            }
        }

        while (f.HasMore())
        {
            PathTile t = GUTIL.Flooder().PollGreatest();
            if (t.GetValue() <= 0)
            {
                break;
            }
            foreach (DIR d in DIR.ALL)
            {
                int dx = t.X() + d.X();
                int dy = t.Y() + d.Y();
                if (SETT.IN_BOUNDS(dx, dy))
                {
                    f.PushGreater(dx, dy, t.GetValue() - d.TileDistance());
                    f.SetValue2(dx, dy, 1);
                }
            }
        }
        f.Done();
    }

    private static PathTile Find(InvasionSpot spot)
    {
        Flooder f = GUTIL.Flooder();
        f.Init(ArtilleryPlacer.GetType());

        foreach (COORDINATE c in spot.Body)
        {
            f.PushSloppy(c, 0);
        }

        while (f.HasMore())
        {
            PathTile t = GUTIL.Flooder().PollSmallest();
            if (THRONE.COO().IsSameAs(t))
            {
                f.Done();
                return t;
            }

            foreach (DIR d in DIR.ALL)
            {
                int dx = t.X() + d.X();
                int dy = t.Y() + d.Y();
                if (SETT.IN_BOUNDS(dx, dy))
                {
                    f.PushSmaller(dx, dy, t.GetValue() + d.TileDistance() * (1 + SETT.PATH().Availability.Get(dx, dy).MovementSpeedI) * (11 - 10 * SETT.ENV().Map.SPACE.Get(dx, dy)), t);
                }
            }
        }
        f.Done();
        return null;
    }

    private static PathTile Rewind(PathTile t, int va)
    {
        PathTile safe = t;

        while (t.Parent != null)
        {
            if (t.GetValue2() >= va)
            {
                safe = t.Parent;
            }
            t = t.Parent;
        }

        if (GUTIL.Flooder().GetValue2(safe.X(), safe.Y()) >= va)
            return null;
        return safe;
    }

    private static PathTile FindPos(InvasionSpot spot, PathTile t)
    {
        SComponent sup = SETT.PATH().Comps.SuperComp.Get(spot.Body.CX(), spot.Body.CY());
        PathTile pos = t;

        while (pos != null && ((SETT.ENV().Map.SPACE.Get(pos) < 0.5 || SETT.PATH().Comps.SuperComp.Get(pos) != sup) || SETT.PATH().Availability.Get(pos.X(), pos.Y()).IsSolid(GAME.ARMIES().Enemy())))
        {
            pos = pos.Parent;
        }

        if (pos == t)
        {
            if (pos.Parent == null)
                return null;
            return pos.Parent;
        }

        return pos;
    }

    private static bool Deploy(Race r, InvasionSpot spot, COORDINATE c, DIR dir, int amount)
    {
        int am = 0;

        SComponent sup = SETT.PATH().Comps.SuperComp.Get(spot.Body.CX(), spot.Body.CY());

        for (int d = 0; d < 32; d += 8)
        {
            int dx = c.X() + dir.Perpendicular().X() * d;
            int dy = c.Y() + dir.Perpendicular().Y() * d;

            for (int w = 0; w < 64; w += 4)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    int x = dx + dir.Next(2).X() * i * w;
                    int y = dy + dir.Next(2).Y() * i * w;
                    ROOM_ARTILLERY a = SETT.ROOMS().ARTILLERY.Rnd();
                    if (Deploy(sup, x, y, dir, a))
                    {
                        amount--;
                        am += a.Services;
                        if (amount <= 0)
                        {
                            CreateDudes(sup, r, spot, am);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private static void CreateDudes(SComponent sup, Race r, InvasionSpot spot, int am)
    {
        Flooder f = GUTIL.Flooder();
        f.Init(ArtilleryPlacer.GetType());

        foreach (COORDINATE c in spot.Body)
        {
            f.PushSloppy(c, 0);
        }

        while (f.HasMore() && am > 0)
        {
            PathTile t = f.PollSmallest();
            if (SETT.PATH().Comps.SuperComp.Get(t.X(), t.Y()) == sup)
            {
                Humanoid dude = new Humanoid(r);
                dude.SetPosition(t.X(), t.Y());
                am--;
            }

            foreach (DIR d in DIR.ALL)
            {
                int dx = t.X() + d.X();
                int dy = t.Y() + d.Y();
                if (SETT.IN_BOUNDS(dx, dy))
                {
                    f.PushSmaller(dx, dy, t.GetValue() + d.TileDistance());
                    f.SetValue2(dx, dy, 1);
                }
            }
        }
        f.Done();
    }

    private static bool Deploy(SComponent sup, int sx, int sy, DIR d, ROOM_ARTILLERY art)
    {
        if (SETT.PATH().Comps.SuperComp.Get(sx, sy) != sup)
            return false;

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

        r.SetEnemy();

        return true;
    }
}