using System;
using System.Collections.Generic;
using System.IO;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.sets;
using util.info;
using util.text;

public interface PERMISSION
{
    bool Get(HCLASS cl, Race race);
    void Set(HCLASS cl, Race race, bool value);
    default void Toggle(HCLASS cl, Race race)
    {
        Set(cl, race, !Get(cl, race));
    }
    default bool Get(Induvidual indu)
    {
        return Get(indu.hType().parentClass(), indu.race());
    }
    default bool Has(Humanoid h)
    {
        return Get(h.indu());
    }
    INFO Info();
}

public class Permission : PERMISSION, SAVABLE
{
    private static readonly string ¤¤name = "¤Permission";
    private static readonly string ¤¤desc = "¤Toggle permission";

    static Permission()
    {
        D.ts(typeof(PERMISSION));
    }

    private readonly Bitmap1D access = new Bitmap1D(RACES.all().Count * HCLASSES.ALL().Count, false);
    private bool def = false;
    private readonly INFO info;

    public Permission(INFO info)
    {
        this.info = info;
    }

    public Permission(string name, string desc)
    {
        this.info = new INFO(name, desc);
    }

    public Permission()
    {
        this.info = new INFO(¤¤name, ¤¤desc);
    }

    public bool Get(HCLASS cl, Race race)
    {
        if (race == null)
        {
            foreach (Race r in RACES.all())
            {
                if (Get(cl, r))
                    return true;
            }
            return false;
        }
        return access.Get(cl.index() * RACES.all().Count + race.index);
    }

    public void Set(HCLASS cl, Race race, bool value)
    {
        if (race == null)
        {
            foreach (Race r in RACES.all())
            {
                Set(cl, r, value);
            }
        }
        else
        {
            access.Set(cl.index() * RACES.all().Count + race.index, value);
        }
    }

    public INFO Info()
    {
        return info;
    }

    public void Save(FilePutter file)
    {
        access.Save(file);
    }

    public void Load(FileGetter file)
    {
        access.Load(file);
    }

    public void Clear()
    {
        access.SetAll(def);
    }

    public void SetDef(bool def)
    {
        this.def = def;
        Clear();
    }
}