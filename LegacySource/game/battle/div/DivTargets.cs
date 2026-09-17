using System;
using System.Collections.Generic;
using System.IO;

public sealed class DivTargets
{
    private static readonly List<Humanoid> list = new List<Humanoid>(64);
    private float now = -10;
    private int[] targets = new int[64];
    private byte ti;
    private byte tl;
    private readonly short di;

    public DivTargets(Div div)
    {
        this.di = div.Index();
        Saver.Clear();
    }

    public Humanoid GetNextTarget()
    {
        double t = TIME.CurrentSecond();

        if (TIME.CurrentSecond() < now)
            return null;

        while (ti < tl)
        {
            Humanoid h = ValidateTarget(targets[ti]);
            ti++;
            if (h != null)
                return h;
        }

        ti = 0;
        tl = 0;

        if (Div().Deployed() == 0)
        {
            now = (float)(t + 10);
            return null;
        }

        COORDINATE c = Div().Position().CentreTile();

        if (c == null)
            return null;

        int x = c.X();
        int y = c.Y();
        if (!SETT.InBounds(x, y))
        {
            now = (float)(t + 10);
            return null;
        }

        list.Clear();
        SETT.PATH().Finders().Target.Add(list, x, y, !Player(), 128, 64);

        for (int i = 0; i < list.Count; i++)
        {
            targets[tl++] = list[i].ID();
        }

        if (tl == 0)
        {
            now = (float)(t + 10);
            return null;
        }
        ti++;
        return ValidateTarget(targets[ti - 1]);
    }

    private bool Player()
    {
        return GAME.ARMIES().Division(di).Army() == GAME.ARMIES().Player();
    }

    private Div Div()
    {
        return GAME.ARMIES().Division(di);
    }

    public Humanoid ValidateTarget(int pointer)
    {
        ENTITY e = SETT.ENTITIES().GetByID(pointer);
        if (e == null || !(e is Humanoid))
            return null;
        Humanoid a = (Humanoid)e;
        if (Player() == a.Indu().Hostile())
        {
            return a;
        }

        return null;
    }

    private static DivTargets s;
    private static readonly SAVABLE Saver = new SAVABLE
    {
        Save = file =>
        {
            file.F(s.now);
            file.Is(s.targets);
            file.B(s.ti);
            file.B(s.tl);
        },

        Load = file =>
        {
            s.now = file.F();
            file.Is(s.targets);
            s.ti = file.B();
            s.tl = file.B();
        },

        Clear = () =>
        {
            s.ti = 0;
            s.tl = 0;
            s.now = 0;
        }
    };

    public SAVABLE Saver()
    {
        s = this;
        return Saver;
    }
}

public interface SAVABLE
{
    Action<FilePutter> Save { get; }
    Action<FileGetter> Load { get; }
    Action Clear { get; }
}

public class FilePutter
{
    public void F(float value) { /* Implementation */ }
    public void Is(int[] value) { /* Implementation */ }
    public void B(byte value) { /* Implementation */ }
}

public class FileGetter
{
    public float F() { /* Implementation */ }
    public int[] Is() { /* Implementation */ }
    public byte B() { /* Implementation */ }
}

public class TIME
{
    public static double CurrentSecond() { /* Implementation */ }
}

public class GAME
{
    public static ARMIES ARMIES() { /* Implementation */ }
}

public class ARMIES
{
    public Div Division(short index) { /* Implementation */ }
    public Div Player() { /* Implementation */ }
}

public class Div
{
    public short Index() { /* Implementation */ }
    public int Deployed() { /* Implementation */ }
    public COORDINATE Position() { /* Implementation */ }
}

public class COORDINATE
{
    public int X() { /* Implementation */ }
    public int Y() { /* Implementation */ }
}

public class SETT
{
    public static bool InBounds(int x, int y) { /* Implementation */ }
    public PATH PATH() { /* Implementation */ }
}

public class PATH
{
    public FINDERS Finders() { /* Implementation */ }
}

public class FINDERS
{
    public void Add(List<Humanoid> list, int x, int y, bool condition, int value1, int value2) { /* Implementation */ }
}

public class ENTITY
{
    public bool IsInstanceOfType<T>() { /* Implementation */ }
}

public class Humanoid : ENTITY
{
    public int ID() { /* Implementation */ }
    public INDU indu() { /* Implementation */ }
}

public class INDU
{
    public bool Hostile() { /* Implementation */ }
}

public class ArrayList<T>
{
    public void ClearSloppy() { /* Implementation */ }
}

public class List<T>
{
    public void Clear() { /* Implementation */ }
    public int Count { get; }
}

public class Alloc
{
    public static int[] ii(int size) { /* Implementation */ }
}

public class FilePutter
{
    public void F(float value) { /* Implementation */ }
    public void Is(int[] value) { /* Implementation */ }
    public void B(byte value) { /* Implementation */ }
}

public class FileGetter
{
    public float F() { /* Implementation */ }
    public int[] Is() { /* Implementation */ }
    public byte B() { /* Implementation */ }
}