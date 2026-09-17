using System;
using System.IO;
using game.battle;
using game.battle.formation;
using game.battle.util;
using init.constant;
using init.race;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;

public sealed class BattleOrderPath : COORDINATE, SAVABLE, Copyable<BattleOrderPath>
{
    public const int size = 128;
    private static readonly VectorImp vec = new VectorImp();

    private readonly int[] coos = new int[size * 2];
    private int current = 0;
    private int length = 0;
    private readonly Coo finalDest = new Coo(-1, -1);
    private bool isComplete;
    private int tilesToDest = 0;
    public int dCount;

    public BattleOrderPath()
    {
    }

    public void Save(FilePutter file)
    {
        file.Is(coos);
        file.I(current);
        file.I(length);
        finalDest.Save(file);
        file.Bool(isComplete);
        file.I(tilesToDest);
        file.I(dCount);
    }

    public void Load(FileGetter file) throws IOException
    {
        file.Is(coos);
        current = file.I();
        length = file.I();
        finalDest.Load(file);
        isComplete = file.Bool();
        tilesToDest = file.I();
        dCount = file.I();
    }

    public void Clear()
    {
        current = 0;
        length = 0;
        finalDest.Set(-1, -1);
        tilesToDest = 0;
        dCount = 0;
    }

    public int X()
    {
        return coos[current];
    }

    public int Y()
    {
        return coos[current + size];
    }

    private bool CanWalkTheLine2(int sx, int sy, int dx, int dy, double max, PathCost cost, Army a, Race race)
    {
        double c = 0;
        double l = vec.Set(sx, sy, dx, dy);
        int steps = (int)Math.Abs(l / C.TILE_SIZE);
        for (int i = 1; i <= steps; i++)
        {
            int x = (int)(sx + i * C.TILE_SIZE * vec.NX());
            int y = (int)(sy + i * C.TILE_SIZE * vec.NY());
            if (DivPlacability.PixelIsBlocked(x, y, C.TILE_SIZE, a))
                return false;
            if (i < steps)
            {
                int nx = (int)(sx + (i + 1) * C.TILE_SIZE * vec.NX());
                int ny = (int)(sy + (i + 1) * C.TILE_SIZE * vec.NY());
                if (!DivPlacability.CheckPixelStep(x, y, nx, ny, race, a))
                    return false;
            }
            c += GetCost(x, y, cost);
            if (c > max)
            {
                return false;
            }
        }

        return true;
    }

    void Init(int startPX, int startPY, PathGame.PathFancy p, int ftDestX, int ftDestY, PathCost cost, Army a, Race race)
    {
        finalDest.Set(ftDestX, ftDestY);
        coos[0] = startPX;
        coos[size] = startPY;
        length = 1;
        current = 0;
        int currentI = 0;
        if (p.HasNext())
        {
            p.SetNext();
            int distance = 1;

            while (++currentI < size)
            {
                int sx = coos[currentI - 1];
                int sy = coos[currentI - 1 + size];
                double costt = GetCost(sx, sy, cost);
                while (p.HasNext() && distance < 32)
                {
                    int px = p.X();
                    int py = p.Y();
                    p.SetNext();
                    int dx = (p.X() << C.T_SCROLL) + C.TILE_SIZEH;
                    int dy = (p.Y() << C.T_SCROLL) + C.TILE_SIZEH;
                    costt += GetCost(dx, dy, cost) * ((px != p.X() && py != p.Y()) ? C.SQR2 : 1.0);

                    if (!CanWalkTheLine2(sx, sy, dx, dy, costt, cost, a, race))
                    {
                        p.SetPrev();
                        break;
                    }
                    distance++;
                }
                int dx = (p.X() << C.T_SCROLL) + C.TILE_SIZEH;
                int dy = (p.Y() << C.T_SCROLL) + C.TILE_SIZEH;
                double d = vec.Set(sx, sy, dx, dy);
                length++;
                if (d < C.TILE_SIZE + C.TILE_SIZEH)
                {
                    coos[currentI] = dx;
                    coos[currentI + size] = dy;
                    if (!p.HasNext())
                        break;
                    p.SetNext();
                }
                else
                {
                    coos[currentI] = (int)(sx + vec.NX() * C.TILE_SIZE);
                    coos[currentI + size] = (int)(sy + vec.NY() * C.TILE_SIZE);
                }
                if (distance > 0)
                    distance--;
            }
        }

        SetCurrentI(length - 1);

        SetCurrentI(0);

        current = 0;

        isComplete = p.IsCompleate() && length < size;

        tilesToDest = p.LengthTotal() - p.GetCurrentI() + Length();
    }

    public int Length()
    {
        return length;
    }

    public int CurrentI()
    {
        return current;
    }

    public void SetCurrentI(int i)
    {
        if (i < 0 || i >= length)
            throw new RuntimeException(i + " " + length);
        current = i;
    }

    public void CurrentIInc(int d)
    {
        SetCurrentI(current + d);
    }

    public bool IsDest()
    {
        return current >= length - 1;
    }

    public COORDINATE FinalTDest()
    {
        return finalDest;
    }

    public bool IsComplete()
    {
        return isComplete;
    }

    public int TilesToDest()
    {
        return tilesToDest - CurrentI();
    }

    public void Copy(BattleOrderPath toBeCopied)
    {
        for (int i = 0; i < coos.Length; i++)
        {
            coos[i] = toBeCopied.coos[i];
        }
        current = toBeCopied.current;
        length = toBeCopied.length;
        tilesToDest = toBeCopied.tilesToDest;
        finalDest.Set(toBeCopied.finalDest);
        isComplete = toBeCopied.isComplete;
        current = toBeCopied.current;
        dCount = toBeCopied.dCount;
    }

    private static double GetCost(int x1, int y1, PathCost m)
    {
        return m.Cost(x1 >> C.T_SCROLL, y1 >> C.T_SCROLL);
    }

    //private static bool CheckStep(int fx, int fy, int tox, int toy, int tz)
    //{
    //    {
    //        fx = (fx) >> C.T_SCROLL;
    //        fy = (fy) >> C.T_SCROLL;
    //        int tx = (tox) >> C.T_SCROLL;
    //        int ty = (toy) >> C.T_SCROLL;
    //        if (tx != fx || ty != fy)
    //            if (player.GetCost(fx, fy, tx, ty) < 0)
    //                return false;
    //    }
    //    return true;
    //}
}