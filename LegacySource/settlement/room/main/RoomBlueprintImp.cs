using System;
using System.Collections.Generic;
using System.IO;
using game.audio;
using game.boosting;
using game.faction;
using init.sprite;
using init.type;
using init.value;
using settlement.path.finders;
using settlement.room.main.category;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using snake2d.util.color;
using snake2d.util.misc;
using util.info;

public abstract class RoomBlueprintImp : RoomBlueprint
{
    public readonly Icon icon;
    public readonly string type;
    public readonly INFO info;
    public readonly RoomCategorySub cat;
    protected double degradeRate;
    private readonly RoomUpgrades upgrades;
    private readonly int typeIndex;
    public readonly Lockable<Faction> reqs;

    public readonly Ambiance soundAmbiance;
    public readonly SoundRace clickSounds;

    protected Boostable bonus;

    static List<RoomBlueprintImp> IMPS = new List<RoomBlueprintImp>();
    static RoomBlueprintImp()
    {
        GameDisposable.Add(() =>
        {
            IMPS.Clear();
        });
    }

    protected RoomBlueprintImp(RoomInitData init, int typeIndex, string key, RoomCategorySub cat) : this(init, typeIndex, key, cat, null)
    {
    }

    protected RoomBlueprintImp(RoomInitData init, int typeIndex, string key, RoomCategorySub cat, ACTION wiki) : base(key)
    {
        init.Init(key);
        this.typeIndex = typeIndex;
        if (cat != null)
            cat.Add(this);
        this.type = init.Type();
        info = new INFO(init.Text(), wiki);
        icon = Icon(init);
        this.cat = cat;
        degradeRate = init.Data().Has("DEGRADE_RATE") ? init.Data().D("DEGRADE_RATE", 0, 1) : 0.75;
        upgrades = new RoomUpgrades(this, init);
        IMPS.Add(this);
        reqs = GVALUES.FACTION.LOCK.Push("ROOM_" + key, info.Name, info.Desc, icon);
        reqs.Push(init.Data());
        soundAmbiance = AUDIO.AMBI().Get("ROOM_" + key);
        clickSounds = AUDIO.Race("ROOM_CLICK_" + key);
    }

    private Icon Icon(RoomInitData init)
    {
        return SPRITES.icons().Get(init.Data());
    }

    public Boostable Bonus()
    {
        return bonus;
    }

    public Icon IconBig()
    {
        return icon;
    }

    public abstract SFinderFindable Service(int tx, int ty);

    public override COLOR MiniC(int tx, int ty)
    {
        return Constructor().MiniColor(tx, ty);
    }

    public override COLOR MiniCPimped(ColorImp original, int tx, int ty, bool northern, bool southern)
    {
        return Constructor().MiniColorPimped(original, tx, ty, northern, southern);
    }

    public abstract Furnisher Constructor();

    public bool IsAvailable(CLIMATE c)
    {
        return true;
    }

    public double DegradeRate()
    {
        return degradeRate;
    }

    public RoomUpgrades Upgrades()
    {
        return upgrades;
    }

    public int TypeIndex()
    {
        return typeIndex;
    }

    public override string ToString()
    {
        return "[" + Index() + "]" + Key;
    }
}